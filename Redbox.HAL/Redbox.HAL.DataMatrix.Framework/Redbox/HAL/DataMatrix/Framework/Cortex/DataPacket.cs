namespace Redbox.HAL.DataMatrix.Framework.Cortex
{
    internal sealed class DataPacket : CortexPacket
    {
        internal DataPacket(byte[] packet, ushort dataSize)
            : base(packet, dataSize)
        {
        }

        internal override PacketType PacketType => PacketType.Data;

        protected override void ParseData()
        {
            for (var index = 0; index < DataSize; ++index)
                PacketData.Add(Packet[index + 15]);
        }
    }
}