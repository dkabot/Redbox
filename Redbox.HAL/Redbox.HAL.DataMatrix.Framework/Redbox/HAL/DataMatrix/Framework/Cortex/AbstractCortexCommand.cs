using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.DataMatrix.Framework.Cortex
{
    internal abstract class AbstractCortexCommand : IDisposable
    {
        protected readonly bool LogDetails = BarcodeConfiguration.Instance.LogDetailedScan;
        protected readonly List<CortexPacket> Packets = new List<CortexPacket>();

        private readonly byte[] Prefix = new byte[4]
        {
            238,
            238,
            238,
            238
        };

        private bool Disposed;
        private byte[] m_preppedCommand;

        public byte[] Command
        {
            get
            {
                if (m_preppedCommand == null)
                    m_preppedCommand = PrepCommand(CoreCommand);
                return m_preppedCommand;
            }
        }

        public CortexPacket[] ParsedPackets => Packets.ToArray();

        protected virtual int ReadTimeout => 2000;

        protected abstract byte[] CoreCommand { get; }

        internal bool PortError { get; private set; }

        internal TimeSpan ExecutionTime { get; private set; }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Send(ICommPort port)
        {
            port.ValidateResponse = OnTest;
            if (port != null && port.IsOpen)
                using (var executionTimer = new ExecutionTimer())
                {
                    if (!port.SendRecv(Command, ReadTimeout).CommOk)
                    {
                        LogHelper.Instance.Log("[Cortex Service] Communication error with the device.");
                        PortError = true;
                    }
                    else if (!OnValidateResponse())
                    {
                        PortError = true;
                    }

                    executionTimer.Stop();
                    ExecutionTime = executionTimer.Elapsed;
                }
            else
                LogHelper.Instance.Log("[Cortex Service] The port is not open sending a command.");
        }

        protected virtual bool CoreValidate()
        {
            return true;
        }

        protected virtual bool CoreProcess()
        {
            foreach (var packet in Packets)
            {
                if (packet.PacketType == PacketType.Incomplete)
                    return false;
                if (packet.RawPacketType == 'd')
                    return true;
            }

            return false;
        }

        protected virtual void OnDispose(bool disposing)
        {
        }

        protected bool OnTest(IChannelResponse ichannelResponse_0)
        {
            return OnTest(ichannelResponse_0.RawResponse);
        }

        protected virtual bool OnTest(byte[] array)
        {
            Packets.Clear();
            var collection = PacketFactory.Instance.Parse(array);
            if (collection.Count == 0 || collection.FindAll(each => each.PacketType == PacketType.Incomplete).Count > 0)
                return false;
            Packets.AddRange(collection);
            return CoreProcess();
        }

        protected virtual bool OnValidateResponse()
        {
            if (Packets.Count == 0)
            {
                LogHelper.Instance.Log("[Cortex Commands] Validate response found no response packets.");
                return false;
            }

            foreach (var packet in Packets)
                if (packet.PacketType == PacketType.Incomplete)
                {
                    LogHelper.Instance.Log("[Cortex Commands] Validate response found an incomplete packet.");
                    return false;
                }

            return CoreValidate();
        }

        protected void DumpPackets(List<CortexPacket> packets)
        {
            if (!LogDetails)
                return;
            LogHelper.Instance.Log(" [AbstractCortexCommand] packet statistics ");
            LogHelper.Instance.Log("  Packet received count: {0}", packets.Count);
            packets.ForEach(cortexPacket_0 => LogHelper.Instance.Log("  packet type {0} raw {1} size {2}",
                cortexPacket_0.PacketType, cortexPacket_0.RawPacketType, cortexPacket_0.Length));
        }

        private bool Test(IChannelResponse ichannelResponse_0)
        {
            return OnTest(ichannelResponse_0);
        }

        private byte[] PrepCommand(byte[] baseCommand)
        {
            var byteList = new List<byte>();
            try
            {
                byteList.AddRange(Prefix);
                ushort initialValue = 0;
                for (var index = 0; index < baseCommand.Length; ++index)
                {
                    initialValue = CRC16.CRC(initialValue, baseCommand[index]);
                    byteList.Add(baseCommand[index]);
                }

                var num1 = (byte)((initialValue >> 8) & sbyte.MaxValue);
                byteList.Add(num1);
                var num2 = (byte)(initialValue & (uint)sbyte.MaxValue);
                byteList.Add(num2);
                return byteList.ToArray();
            }
            finally
            {
                byteList.Clear();
            }
        }

        private void Dispose(bool disposing)
        {
            if (Disposed)
                return;
            Disposed = true;
            OnDispose(disposing);
            Packets.Clear();
        }
    }
}