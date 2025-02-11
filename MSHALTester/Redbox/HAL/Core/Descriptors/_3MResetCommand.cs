using Microsoft.Win32.SafeHandles;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors;

internal abstract class _3MResetCommand : Abstract3MCommand
{
    private readonly ResetType_3M Type;

    protected _3MResetCommand(ResetType_3M type)
    {
        Type = type;
    }

    protected abstract byte ResetByte { get; }

    protected abstract byte CompletionByte { get; }

    protected override byte[] OnReadResponse(SafeFileHandle handle)
    {
        return new byte[1];
    }

    protected override byte OnFillIndex(int idx)
    {
        byte num = 0;
        switch (idx)
        {
            case 0:
                num = 64;
                break;
            case 1:
                num = 7;
                break;
            case 2:
                num = ResetByte;
                break;
        }

        return num;
    }

    internal bool Reset()
    {
        return SendReceive().Length != 0 && GetStatus(Type == ResetType_3M.Soft ? 750 : 15000);
    }

    private bool GetStatus(int pause)
    {
        var service = ServiceLocator.Instance.GetService<IRuntimeService>();
        var status = false;
        var obj = new _3mStatusCommand();
        for (var index = 0; index < 5; ++index)
        {
            service.Wait(pause);
            var numArray = obj.SendReceive();
            if (numArray.Length != 0)
            {
                if (numArray[3] == CompletionByte)
                {
                    status = true;
                    break;
                }
            }
            else
            {
                break;
            }
        }

        return status;
    }

    protected enum ResetType_3M
    {
        Soft,
        Hard
    }
}