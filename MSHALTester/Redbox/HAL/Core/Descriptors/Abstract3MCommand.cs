using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Interop;

namespace Redbox.HAL.Core.Descriptors;

internal abstract class Abstract3MCommand
{
    protected readonly byte[] Command;
    protected readonly byte[] ErrorResponse = new byte[0];

    protected Abstract3MCommand()
    {
        Command = new byte[CommandSize];
        for (var idx = 0; idx < Command.Length; ++idx)
            Command[idx] = OnFillIndex(idx);
    }

    protected virtual int CommandSize => 8;

    protected abstract byte[] OnReadResponse(SafeFileHandle handle);

    protected abstract byte OnFillIndex(int idx);

    protected SafeFileHandle Connect()
    {
        var file = Win32.CreateFile("\\\\.\\TwTouchDevice1", Win32.AccessFlags.GENERIC_READ_WRITE,
            Win32.ShareFlags.FILE_SHARE_READ_WRITE, IntPtr.Zero, 3U, 128U, IntPtr.Zero);
        if (file.IsInvalid)
            LogHelper.Instance.Log("[MicrotouchDescriptor] CreateFile failed: Error = {0}",
                Marshal.GetLastWin32Error());
        return file;
    }

    protected bool Write(SafeFileHandle handle, byte[] command)
    {
        var flag = true;
        if (!Win32.WriteFile(handle, command, command.Length, out var _, IntPtr.Zero))
        {
            LogHelper.Instance.Log("[{0}] WriteFile failed: Error = {1}", GetType().Name, Marshal.GetLastWin32Error());
            flag = false;
        }

        return flag;
    }

    protected byte[] Read(SafeFileHandle handle, int responseLength)
    {
        var lpBuffer = new byte[responseLength];
        if (!Win32.ReadFile(handle, lpBuffer, lpBuffer.Length, out var _, IntPtr.Zero))
        {
            lpBuffer = ErrorResponse;
            LogHelper.Instance.Log("[{0}] ReadFile failed: Error = {1}", GetType().Name, Marshal.GetLastWin32Error());
        }

        return lpBuffer;
    }

    internal byte[] SendReceive()
    {
        using (var handle = Connect())
        {
            return handle.IsInvalid ? ErrorResponse : !Write(handle, Command) ? ErrorResponse : OnReadResponse(handle);
        }
    }
}