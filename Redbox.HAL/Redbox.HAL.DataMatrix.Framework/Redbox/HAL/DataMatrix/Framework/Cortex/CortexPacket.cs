using System;
using System.Collections.Generic;

namespace Redbox.HAL.DataMatrix.Framework.Cortex
{
    internal abstract class CortexPacket
    {
        protected readonly BigEndianConverter Converter = new BigEndianConverter();
        protected readonly byte[] Packet;
        protected readonly List<byte> PacketData = new List<byte>();

        protected CortexPacket()
        {
        }

        protected CortexPacket(CortexPacket cortexPacket_0)
        {
            Packet = new byte[cortexPacket_0.Packet.Length];
            Array.Copy(cortexPacket_0.Packet, 0, Packet, 0, cortexPacket_0.Packet.Length);
            DataSize = cortexPacket_0.DataSize;
            ProtocolVersion = cortexPacket_0.ProtocolVersion;
            ID = Converter.ToInt32(Packet, 4);
            PacketNumber = cortexPacket_0.PacketNumber;
            Timestamp = cortexPacket_0.Timestamp;
            CRC = cortexPacket_0.CRC;
            ChecksumOk = cortexPacket_0.ChecksumOk;
            RawPacketType = cortexPacket_0.RawPacketType;
            ParseData();
        }

        protected CortexPacket(byte[] data)
        {
            Packet = data;
        }

        protected CortexPacket(byte[] packet, ushort dataSize)
        {
            Packet = new byte[packet.Length];
            Array.Copy(packet, 0, Packet, 0, packet.Length);
            ProtocolVersion = Packet[3];
            ID = Converter.ToInt32(Packet, 4);
            PacketNumber = Packet[8];
            Timestamp = Converter.ToInt32(Packet, 9);
            DataSize = Converter.ToUInt16(Packet, 13);
            if (dataSize != DataSize)
                DataSize = dataSize;
            ParseData();
            var num = (ushort)(((uint)Packet[Packet.Length - 2] << 8) | Packet[Packet.Length - 1]);
            ushort initialValue = 0;
            for (var index = 0; index < Packet.Length - 2; ++index)
                initialValue = CRC16.CRC(initialValue, Packet[index]);
            CRC = initialValue;
            ChecksumOk = CRC == num;
        }

        internal virtual PacketType PacketType => PacketType.None;

        internal int ProtocolVersion { get; }

        internal int ID { get; private set; }

        internal byte PacketNumber { get; }

        internal int Timestamp { get; }

        internal ushort DataSize { get; }

        internal char RawPacketType { get; set; }

        internal ushort CRC { get; set; }

        internal bool ChecksumOk { get; }

        internal byte[] PayloadData => PacketData.ToArray();

        internal int Length => Packet != null ? Packet.Length : 0;

        protected virtual void ParseData()
        {
        }
    }
}