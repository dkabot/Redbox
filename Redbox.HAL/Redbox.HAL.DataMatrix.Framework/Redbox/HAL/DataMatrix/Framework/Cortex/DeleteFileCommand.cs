namespace Redbox.HAL.DataMatrix.Framework.Cortex
{
    internal sealed class DeleteFileCommand : TextCommand
    {
        internal DeleteFileCommand(ImagePacket packet)
        {
            Data = packet.ImageName;
        }

        protected override string Prefix => "9";

        protected override string Data { get; }
    }
}