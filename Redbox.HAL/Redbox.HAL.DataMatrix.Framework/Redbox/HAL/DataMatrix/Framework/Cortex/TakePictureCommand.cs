namespace Redbox.HAL.DataMatrix.Framework.Cortex
{
    internal sealed class TakePictureCommand : AbstractCortexCommand
    {
        protected override byte[] CoreCommand { get; } = new byte[4]
        {
            36,
            1,
            7,
            0
        };

        protected override int ReadTimeout => 8000;

        internal ImagePacket ImagePacket
        {
            get
            {
                var imagePacket = (ImagePacket)null;
                foreach (var parsedPacket in ParsedPackets)
                    if (parsedPacket.RawPacketType == 'm')
                        imagePacket = new ImagePacket(parsedPacket);
                return imagePacket;
            }
        }

        protected override bool CoreValidate()
        {
            var num = 0;
            foreach (var packet in Packets)
                if (packet.RawPacketType == 'm')
                    ++num;
                else if (packet.RawPacketType == 'd')
                    ++num;
            return num >= 2;
        }

        protected override bool CoreProcess()
        {
            var flag1 = false;
            var flag2 = false;
            foreach (var packet in Packets)
                if (packet.RawPacketType == 'd')
                {
                    flag1 = true;
                }
                else if (packet.RawPacketType == 'm')
                {
                    flag2 = true;
                }
                else if (packet.PacketType == PacketType.Incomplete)
                {
                    flag2 = false;
                    flag1 = false;
                }

            return flag1 & flag2;
        }
    }
}