using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Interop;

namespace Redbox.HAL.IPC.Framework.Pipes;

public sealed class NamedPipeServer : IDisposable
{
    private const int PIPE_FLAGS_OVERLAPPED = 1073741824;
    private const int PIPE_ACCESS_OUTBOUND = 2;
    private const int PIPE_ACCESS_DUPLEX = 3;
    private const int PIPE_ACCESS_INBOUND = 1;
    private const int PIPE_WAIT = 0;
    private const int PIPE_NOWAIT = 1;
    private const int PIPE_READMODE_BYTE = 0;
    private const int PIPE_READMODE_MESSAGE = 2;
    private const int PIPE_TYPE_BYTE = 0;
    private const int PIPE_TYPE_MESSAGE = 4;
    private const int PIPE_CLIENT_END = 0;
    private const int PIPE_SERVER_END = 1;
    private const int PIPE_UNLIMITED_INSTANCES = 255;
    private const uint NMPWAIT_WAIT_FOREVER = 4294967295;
    private const uint NMPWAIT_NOWAIT = 1;
    private const uint NMPWAIT_USE_DEFAULT_WAIT = 0;
    private const uint SECURITY_DESCRIPTOR_REVISION = 1;
    private static IntPtr INVALID_HANDLE_VALUE = (IntPtr)(-1);
    private readonly int BufferSize = 4096;
    private readonly Win32.SecurityAttributes m_securityAttributes;
    private readonly IntPtr m_securityAttributesPointer = IntPtr.Zero;
    private readonly Win32.SecurityDescriptor m_securityDescriptor;
    private readonly IntPtr m_securityDescriptorPointer = IntPtr.Zero;
    private readonly string PipeName;
    private readonly string PipePath;
    private bool Disposed;

    private NamedPipeServer(string pipeName)
    {
        m_securityDescriptor = new Win32.SecurityDescriptor();
        Win32.InitializeSecurityDescriptor(ref m_securityDescriptor, 1U);
        Win32.SetSecurityDescriptorDacl(ref m_securityDescriptor, true, IntPtr.Zero, false);
        m_securityDescriptorPointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(m_securityDescriptor));
        Marshal.StructureToPtr(m_securityDescriptor, m_securityDescriptorPointer, false);
        m_securityAttributes = new Win32.SecurityAttributes
        {
            nLength = Marshal.SizeOf(m_securityDescriptor),
            lpSecurityDescriptor = m_securityDescriptorPointer,
            bInheritHandle = 1
        };
        m_securityAttributesPointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(m_securityAttributes));
        Marshal.StructureToPtr(m_securityAttributes, m_securityAttributesPointer, false);
        PipeName = pipeName;
        PipePath = "\\\\.\\PIPE\\" + PipeName;
    }

    public void Dispose()
    {
        OnDispose(true);
        GC.SuppressFinalize(this);
    }

    public static NamedPipeServer Create(string pipeName)
    {
        return new NamedPipeServer(pipeName);
    }

    public BasePipeChannel WaitForClientConnect()
    {
        var namedPipe = CreateNamedPipe(PipePath, 3U, 0U, byte.MaxValue, (uint)BufferSize, (uint)BufferSize,
            uint.MaxValue, IntPtr.Zero);
        if (namedPipe.IsInvalid)
            return null;
        var lastWin32Error = ConnectNamedPipe(namedPipe, IntPtr.Zero) ? 0 : Marshal.GetLastWin32Error();
        return 535 == lastWin32Error || lastWin32Error == 0 ? ServerPipeChannel.Make(namedPipe) : (BasePipeChannel)null;
    }

    ~NamedPipeServer()
    {
        OnDispose(false);
    }

    private void OnDispose(bool disposing)
    {
        if (Disposed)
            return;
        Marshal.FreeCoTaskMem(m_securityDescriptorPointer);
        Marshal.FreeCoTaskMem(m_securityAttributesPointer);
        Disposed = true;
        LogHelper.Instance.Log("[NamedPipeServer] OnDispose");
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern SafeFileHandle CreateNamedPipe(
        string pipeName,
        uint dwOpenMode,
        uint dwPipeMode,
        uint nMaxInstances,
        uint nOutBufferSize,
        uint nInBufferSize,
        uint nDefaultTimeOut,
        IntPtr lpSecurityAttributes);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ConnectNamedPipe(SafeFileHandle hNamedPipe, IntPtr lpOverlapped);
}