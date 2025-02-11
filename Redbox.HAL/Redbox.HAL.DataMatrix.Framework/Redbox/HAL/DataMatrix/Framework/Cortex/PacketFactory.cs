using System;
using System.Collections.Generic;

namespace Redbox.HAL.DataMatrix.Framework.Cortex
{
    internal sealed class PacketFactory
    {
        private const ushort PacketOverhead = 17;
        private static readonly PacketFactory m_instance = new PacketFactory();
        private readonly BigEndianConverter Converter = new BigEndianConverter();

        private PacketFactory()
        {
        }

        internal static PacketFactory Instance => m_instance;

        internal CortexPacket ParseSingle(byte[] data)
        {
            var cortexPacket = (CortexPacket)new NilPacket(null);
            return data != null && data.Length != 0 ? ParseInner(data, 0) : cortexPacket;
        }

        internal List<CortexPacket> Parse(byte[] data)
        {
            var cortexPacketList = new List<CortexPacket>();
            CortexPacket inner;
            for (var index = 0; index < data.Length; index += inner.Length)
            {
                var startIndex = index;
                inner = ParseInner(data, startIndex);
                cortexPacketList.Add(inner);
                if (inner.PacketType == PacketType.Incomplete || inner.PacketType == PacketType.Invalid)
                    break;
            }

            return cortexPacketList;
        }

        private bool IsPacketStart(byte[] data, int startIndex)
        {
            return startIndex <= data.Length && startIndex + 3 <= data.Length && data[startIndex] == 1 &&
                   data[startIndex + 1] == 88 && data[startIndex + 2] == 82;
        }

        private bool IsNonDataPacket(byte[] data, int start)
        {
            return start <= data.Length && start + 18 <= data.Length && data[start + 15] == 1 &&
                   data[start + 16] == 88 && data[start + 17] == 30;
        }

        private CortexPacket ParseInner(byte[] data, int startIndex)
        {
            if (startIndex + 15 > data.Length)
                return new IncompletePacket();
            if (!IsPacketStart(data, startIndex))
                return new NilPacket(data);
            var uint16 = Converter.ToUInt16(data, startIndex + 13);
            var length = uint16 + 17;
            var numArray = new byte[length];
            if (startIndex + length > data.Length)
                return new IncompletePacket();
            Array.Copy(data, startIndex, numArray, 0, length);
            return !IsNonDataPacket(data, startIndex)
                ? new DataPacket(numArray, uint16)
                : (CortexPacket)new NonDataPacket(numArray, uint16);
        }
    }
}