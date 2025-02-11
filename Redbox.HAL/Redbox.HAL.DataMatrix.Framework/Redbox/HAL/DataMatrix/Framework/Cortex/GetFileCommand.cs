using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework.Cortex
{
    internal sealed class GetFileCommand : TextCommand
    {
        private readonly List<byte> ImageData = new List<byte>();
        private readonly List<int> ProcessedPackets = new List<int>();

        internal GetFileCommand(ImagePacket packet)
        {
            Data = packet.ImageName;
        }

        protected override string Prefix => "^";

        protected override string Data { get; }

        protected override int ReadTimeout => 10000;

        internal byte[] ImageBytes => ImageData.ToArray();

        internal ImageDescriptorPacket Descriptor { get; private set; }

        protected override bool CoreValidate()
        {
            return Descriptor != null && ImageBytes.Length == Descriptor.ImageSize;
        }

        protected override bool CoreProcess()
        {
            if (LogDetails)
                LogHelper.Instance.Log("[GetFileCommand] CoreProcess");
            try
            {
                foreach (var packet1 in Packets)
                {
                    var packet = packet1;
                    if (packet.PacketType == PacketType.Data)
                    {
                        if (-1 == ProcessedPackets.FindIndex(each => each == packet.PacketNumber))
                        {
                            if (LogDetails)
                                LogHelper.Instance.Log(" [GetFileCommand] Process data packet ( data length = {0} )",
                                    packet.PayloadData.Length);
                            ImageData.AddRange(packet.PayloadData);
                            ProcessedPackets.Add(packet.PacketNumber);
                        }
                    }
                    else
                    {
                        if (packet.PacketType == PacketType.Incomplete)
                            return false;
                        if (packet.RawPacketType == 'g')
                        {
                            if (Descriptor == null)
                                Descriptor = new ImageDescriptorPacket(packet);
                            if (LogDetails)
                                LogHelper.Instance.Log(
                                    " [GetFileCommand] Process 'g' packet ( ImageSize = {0} ) packet size = {1}.",
                                    Descriptor.ImageSize, packet.Length);
                        }
                    }
                }

                return Descriptor != null && ImageData.Count == Descriptor.ImageSize;
            }
            finally
            {
                if (LogDetails)
                    LogHelper.Instance.Log("[GetFileCommand] End.");
            }
        }
    }
}