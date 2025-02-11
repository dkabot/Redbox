using System.Collections.Generic;
using System.Text;

namespace Redbox.HAL.DataMatrix.Framework.Cortex
{
    internal class ImageDescriptorPacket : ImagePacket
    {
        internal ImageDescriptorPacket(CortexPacket packet)
            : base(packet)
        {
        }

        internal int ImageSize { get; private set; }

        protected override void ParseData()
        {
            var byteList = new List<byte>();
            var index1 = 22;
            while (Packet[index1] != 9)
                byteList.Add(Packet[index1++]);
            m_imageName = Encoding.ASCII.GetString(byteList.ToArray());
            var index2 = index1 + 2;
            var stringBuilder = new StringBuilder();
            for (; Packet[index2] != 41; ++index2)
                stringBuilder.Append((byte)(Packet[index2] - 48U));
            ImageSize = int.Parse(stringBuilder.ToString());
        }
    }
}