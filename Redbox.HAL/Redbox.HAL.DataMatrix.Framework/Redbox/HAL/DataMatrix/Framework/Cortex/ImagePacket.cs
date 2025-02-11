using System.Collections.Generic;
using System.Text;

namespace Redbox.HAL.DataMatrix.Framework.Cortex
{
    internal class ImagePacket : CortexPacket
    {
        protected const int DataStart = 22;
        protected string m_imageName;

        internal ImagePacket(CortexPacket packet)
            : base(packet)
        {
        }

        internal string ImageName => m_imageName;

        protected override void ParseData()
        {
            var byteList = new List<byte>();
            var index = 22;
            while (Packet[index] != 4)
                byteList.Add(Packet[index++]);
            m_imageName = Encoding.ASCII.GetString(byteList.ToArray());
        }
    }
}