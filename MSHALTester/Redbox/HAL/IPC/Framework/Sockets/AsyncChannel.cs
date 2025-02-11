using System;
using System.IO;
using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework.Sockets;

internal sealed class AsyncChannel : IDisposable, IIPCChannel
{
    private readonly string Identifier;
    private readonly bool LogDetails;
    private readonly byte[] ReadBuffer;
    private readonly AutoResetEvent ReadEvent = new(false);
    private readonly Stream Stream;
    private bool Disposed;

    internal AsyncChannel(Stream stream, int bufferSize, string id)
        : this(stream, bufferSize, id, false)
    {
    }

    internal AsyncChannel(Stream stream, int bufferSize, string id, bool logDetails)
    {
        Stream = stream;
        ReadBuffer = new byte[bufferSize];
        LogDetails = logDetails;
        Identifier = id;
    }

    public void Dispose()
    {
        if (Disposed)
            return;
        if (LogDetails)
            LogHelper.Instance.Log("[AsyncChannel-{0}] Channel disposed.", Identifier);
        Disposed = true;
        Disconnect();
    }

    public byte[] Read()
    {
        return Read(30000);
    }

    public byte[] Read(int timeout)
    {
        var response = new ByteAccumulator();
        Read(response, timeout);
        return !response.IsComplete ? new byte[0] : response.Accumulator.ToArray();
    }

    public void Read(IIPCResponse response)
    {
        Read(response, 30000);
    }

    public void Read(IIPCResponse response, int timeout)
    {
        OnRead(response);
        if (ReadEvent.WaitOne(timeout))
            return;
        response.Clear();
    }

    public bool Write(byte[] bytes)
    {
        if (!Stream.CanWrite)
            return false;
        try
        {
            Stream.Write(bytes, 0, bytes.Length);
            return true;
        }
        catch (Exception ex)
        {
            LogHelper.Instance.Log("[AsyncChannel] There was an unhandled exception during write.", ex);
            return false;
        }
    }

    public bool Connect()
    {
        return IsConnected = true;
    }

    public bool Disconnect()
    {
        if (IsConnected)
        {
            Stream.Dispose();
            ReadEvent.Close();
            IsConnected = false;
        }

        return true;
    }

    public bool IsConnected { get; private set; }

    private void OnRead(IIPCResponse response)
    {
        try
        {
            Stream.BeginRead(ReadBuffer, 0, ReadBuffer.Length, EndReadCallback, response);
        }
        catch (Exception ex)
        {
            LogHelper.Instance.Log("[AsyncChannel] Read caught an exception", ex);
            SafeSet();
        }
    }

    private void EndReadCallback(IAsyncResult result)
    {
        try
        {
            var length = Stream.EndRead(result);
            var asyncState = result.AsyncState as IIPCResponse;
            if (length == 0)
            {
                SafeSet();
            }
            else
            {
                Interlocked.Add(ref Statistics.Instance.NumberOfBytesReceived, length);
                if (!asyncState.Accumulate(ReadBuffer, 0, length))
                    OnRead(asyncState);
                else
                    SafeSet();
            }
        }
        catch (Exception ex)
        {
            LogHelper.Instance.Log(
                string.Format("[AsyncChannel -{0}] EndReadCallback has caught an unhandled exception.", Identifier),
                ex);
            SafeSet();
        }
    }

    private bool SafeSet()
    {
        try
        {
            ReadEvent.Set();
            return true;
        }
        catch (Exception ex)
        {
            LogHelper.Instance.Log("[AsyncChannel] Exception caught during event set.", ex);
            return false;
        }
    }
}