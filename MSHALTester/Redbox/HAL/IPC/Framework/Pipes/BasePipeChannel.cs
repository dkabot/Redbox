using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Interop;

namespace Redbox.HAL.IPC.Framework.Pipes;

public abstract class BasePipeChannel : IIPCChannel, IDisposable
{
    private readonly bool FlushOnWrite;
    protected readonly string ID;
    private readonly int MessageHeaderSize = Marshal.SizeOf(typeof(int));
    private bool Disposed;
    protected SafeFileHandle PipeHandle;

    protected BasePipeChannel(string id)
    {
        FlushOnWrite = false;
        ID = id;
    }

    protected BasePipeChannel(SafeFileHandle handle, bool flushOnWrite, string id)
    {
        PipeHandle = handle;
        FlushOnWrite = flushOnWrite;
        ID = id;
    }

    protected BasePipeChannel(SafeFileHandle pipeHandle, string id)
        : this(pipeHandle, false, id)
    {
    }

    public void Dispose()
    {
        DisposeChannel(true);
        GC.SuppressFinalize(this);
    }

    public bool Disconnect()
    {
        if (!IsConnected)
            return false;
        IsConnected = false;
        return OnDisconnect();
    }

    public bool Connect()
    {
        return OnConnect();
    }

    public bool Write(byte[] message)
    {
        if (PipeHandle.IsInvalid)
            return false;
        var lpNumberOfBytesWritten = 0;
        var bytes = BitConverter.GetBytes(message.Length);
        var numArray = new byte[bytes.Length + message.Length];
        Array.Copy(bytes, numArray, bytes.Length);
        Array.Copy(message, 0, numArray, bytes.Length, message.Length);
        if (!Win32.WriteFile(PipeHandle, numArray, numArray.Length, out lpNumberOfBytesWritten, IntPtr.Zero))
        {
            LogHelper.Instance.Log(
                string.Format("[BasePipeChannel] Write failed: GetLastError() = {0}", Marshal.GetLastWin32Error()),
                LogEntryType.Error);
            return false;
        }

        if (FlushOnWrite)
            FlushFileBuffers(PipeHandle);
        return true;
    }

    public byte[] Read()
    {
        var numArray = ReadBytes(MessageHeaderSize);
        return numArray == null || numArray.Length != MessageHeaderSize
            ? null
            : ReadBytes(BitConverter.ToInt32(numArray, 0));
    }

    public byte[] Read(int timeout)
    {
        return Read();
    }

    public void Read(IIPCResponse response)
    {
        Read(response, 30000);
    }

    public void Read(IIPCResponse response, int timeout)
    {
        var rawResponse = Read();
        if (rawResponse == null)
            return;
        response.Accumulate(rawResponse);
    }

    public bool IsConnected { get; protected set; }

    protected abstract bool OnDisconnect();

    protected abstract bool OnConnect();

    protected byte[] ReadBytes(int count)
    {
        if (PipeHandle.IsInvalid)
            return null;
        var lpNumberOfBytesRead = 0;
        var lpBuffer = new byte[count];
        if (!Win32.ReadFile(PipeHandle, lpBuffer, count, out lpNumberOfBytesRead, IntPtr.Zero))
        {
            LogHelper.Instance.Log(
                string.Format("PipeClientSession.ReadFile() returned false; error code = {0}",
                    Marshal.GetLastWin32Error()), LogEntryType.Error);
            return null;
        }

        return lpNumberOfBytesRead == count ? lpBuffer : null;
    }

    ~BasePipeChannel()
    {
        DisposeChannel(false);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    protected static extern bool FlushFileBuffers(SafeFileHandle handle);

    private void DisposeChannel(bool fromDispose)
    {
        if (Disposed)
            return;
        Disposed = true;
        Disconnect();
        LogHelper.Instance.Log(LogEntryType.Debug, "[{0}-{1}] OnDispose", GetType().Name, ID);
    }
}