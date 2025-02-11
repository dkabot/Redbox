namespace Redbox.HAL.Core.Descriptors;

internal sealed class _3mHardResetCommand : _3MResetCommand
{
    internal _3mHardResetCommand()
        : base(ResetType_3M.Hard)
    {
    }

    protected override byte ResetByte => 2;

    protected override byte CompletionByte => 5;
}