using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.DataMatrix.Framework.Cortex
{
    internal sealed class GetImageCommand : TextCommand
    {
        private readonly ImagePacket ImagePacket;
        private byte[] ImageBytes;

        internal GetImageCommand(ImagePacket packet)
        {
            ImagePacket = packet;
            Data = packet.ImageName;
        }

        protected override string Prefix => "^";

        protected override string Data { get; }

        protected override int ReadTimeout => 10000;

        internal ImageDescriptorPacket Descriptor { get; private set; }

        protected override bool OnValidateResponse()
        {
            return Descriptor != null && ImageBytes != null;
        }

        protected override bool OnTest(byte[] array)
        {
            if (Descriptor == null)
            {
                var packet = PacketFactory.Instance.Parse(array).Find(each => each.RawPacketType == 'g');
                if (packet != null)
                {
                    Descriptor = new ImageDescriptorPacket(packet);
                    if (LogDetails)
                        LogHelper.Instance.Log(
                            " [GetImageCommand] Process 'g' packet ( ImageSize = {0} ) packet size = {1}.",
                            Descriptor.ImageSize, packet.Length);
                }

                return false;
            }

            if (array.Length < Descriptor.ImageSize + 37 + Descriptor.Length)
                return false;
            var packets = PacketFactory.Instance.Parse(array);
            if (packets.Count == 0 || packets[0].RawPacketType != 'g' ||
                packets[packets.Count - 1].RawPacketType != 'd')
                return false;
            var all = packets.FindAll(each => each.PacketType == PacketType.Data);
            ImageBytes = new byte[Descriptor.ImageSize];
            var offset = 0;
            var action = (Action<CortexPacket>)(each =>
            {
                Array.Copy(each.PayloadData, 0, ImageBytes, offset, each.PayloadData.Length);
                offset += each.PayloadData.Length;
            });
            all.ForEach(action);
            DumpPackets(packets);
            return true;
        }

        protected override void OnDispose(bool disposing)
        {
            ImageBytes = null;
        }

        internal bool CreateImage(string fileName)
        {
            if (ImageBytes != null && Descriptor != null)
                if (ImageBytes.Length == Descriptor.ImageSize)
                    try
                    {
                        using (var executionTimer = new ExecutionTimer())
                        {
                            using (var image = Image.FromStream(new MemoryStream(ImageBytes)))
                            {
                                image.Save(fileName, ImageFormat.Jpeg);
                            }

                            executionTimer.Stop();
                            LogHelper.Instance.Log("[GetImageCommand] image construction time = {0}ms",
                                executionTimer.ElapsedMilliseconds);
                        }

                        return true;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log(
                            string.Format(
                                "[GetImageCommand] Exception: unable to construct image '{0}'. Image bytes = {1} ( expected {2} )",
                                fileName, ImageBytes.Length, Descriptor.ImageSize), ex);
                        return false;
                    }

            LogHelper.Instance.Log("[GetImageCommand] Unable to construct image. Imag bytes = {0} ( expected {1} )",
                ImageBytes.Length, Descriptor.ImageSize);
            return false;
        }
    }
}