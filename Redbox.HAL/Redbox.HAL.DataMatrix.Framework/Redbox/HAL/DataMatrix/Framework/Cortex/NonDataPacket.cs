using System.Text;

namespace Redbox.HAL.DataMatrix.Framework.Cortex
{
    internal class NonDataPacket : CortexPacket
    {
        internal NonDataPacket(CortexPacket packet)
            : base(packet)
        {
        }

        internal NonDataPacket(byte[] packet, ushort dataSize)
            : base(packet, dataSize)
        {
        }

        internal override PacketType PacketType => PacketType.Response;

        protected override void ParseData()
        {
            RawPacketType = Encoding.ASCII.GetChars(Packet, 21, 1)[0];
            if (DataSize == 0)
                return;
            for (var index = 22; Packet[index] != 4; ++index)
                PacketData.Add(Packet[index]);
        }
    }
}