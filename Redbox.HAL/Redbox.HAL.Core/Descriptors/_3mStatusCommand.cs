using Microsoft.Win32.SafeHandles;

namespace Redbox.HAL.Core.Descriptors;

internal sealed class _3mStatusCommand : Abstract3MCommand
{
    private readonly byte ResponseSize = 20;

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
                num = 6;
                break;
            case 6:
                num = ResponseSize;
                break;
        }

        return num;
    }
}