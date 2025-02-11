namespace Redbox.HAL.DataMatrix.Framework.Cortex
{
    internal sealed class NilPacket : CortexPacket
    {
        internal NilPacket(byte[] packet)
        {
        }

        internal override PacketType PacketType => PacketType.Invalid;
    }
}