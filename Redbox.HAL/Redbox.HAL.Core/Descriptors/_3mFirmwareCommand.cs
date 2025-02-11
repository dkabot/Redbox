using System;
using Microsoft.Win32.SafeHandles;

namespace Redbox.HAL.Core.Descriptors;

internal sealed class _3mFirmwareCommand : Abstract3MCommand
{
    private readonly byte ResponseSize = 24;

    protected override byte[] OnReadResponse(SafeFileHandle handle)
    {
        return Read(handle, ResponseSize);
    }

    protected override byte OnFillIndex(int idx)
    {
        byte num = 0;
        switch (idx)
        {
            case 0:
                num = 192;
                break;
            case 1:
                num = 10;
                break;
            case 6:
                num = ResponseSize;
                break;
        }

        return num;
    }

    internal string GetFirmwareRevision()
    {
        var numArray = SendReceive();
        return numArray.Length != 0
            ? string.Format("{0}.{1}", BitConverter.ToString(numArray, 3, 1), BitConverter.ToString(numArray, 4, 1))
            : "UNKNOWN";
    }
}