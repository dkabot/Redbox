using System;
using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework.Cortex
{
    internal sealed class DecodeCommand : AbstractCortexCommand
    {
        private readonly List<int> DataPackets = new List<int>();
        internal readonly List<string> FoundCodes = new List<string>();

        private readonly char[] Separator = new char[1] { ' ' };

        protected override byte[] CoreCommand { get; } = new byte[4]
        {
            36,
            1,
            5,
            0
        };

        protected override int ReadTimeout => BarcodeConfiguration.Instance.ScanTimeout;

        internal bool DecodeFailure { get; private set; }

        protected override bool CoreValidate()
        {
            var num1 = 0;
            var num2 = 0;
            var num3 = 0;
            foreach (var packet in Packets)
                if (packet.RawPacketType == 'd')
                    ++num1;
                else if (packet.RawPacketType == 'r')
                    ++num2;
                else if (packet.PacketType == PacketType.Data)
                    ++num3;
            if (num1 == 0)
                return false;
            return num2 > 0 || num3 > 0;
        }

        protected override bool CoreProcess()
        {
            var flag = false;
            if (LogDetails)
                LogHelper.Instance.Log("[Decode Command] start.");
            try
            {
                foreach (var packet1 in Packets)
                {
                    var packet = packet1;
                    if (packet.PacketType == PacketType.Incomplete)
                        return false;
                    if (packet.PacketType == PacketType.Data)
                    {
                        var str = Encoding.ASCII.GetString(packet.PayloadData);
                        if (LogDetails)
                            LogHelper.Instance.Log(" [DecodeCommand] Data packet: barcode data = {0} number = {1}", str,
                                packet.PacketNumber);
                        if (DataPackets.FindAll(each => each == packet.PacketNumber).Count == 0)
                        {
                            DataPackets.Add(packet.PacketNumber);
                            var strArray = str.Split(Separator);
                            if ((int)Convert.ChangeType(strArray[2], typeof(int)) == 0)
                                DecodeFailure = true;
                            else
                                for (var index = 3; index < strArray.Length; ++index)
                                    FoundCodes.Add(strArray[index]);
                        }
                    }
                    else
                    {
                        if (LogDetails)
                            LogHelper.Instance.Log(" [DecodeCommand] Reponse packet '{0}'", packet.RawPacketType);
                        switch (packet.RawPacketType)
                        {
                            case 'd':
                                flag = true;
                                continue;
                            case 'e':
                            case 'r':
                                DecodeFailure = true;
                                continue;
                            default:
                                continue;
                        }
                    }
                }

                if (!flag || DataPackets.Count == 0)
                    return false;
                DumpPackets(Packets);
                if (LogDetails)
                {
                    LogHelper.Instance.Log("  [DecodeCommand] Decode results:");
                    FoundCodes.ForEach(code => LogHelper.Instance.Log("  Scanned code {0}", code));
                }

                return DataPackets.Count > 0 || DecodeFailure;
            }
            finally
            {
                if (LogDetails)
                    LogHelper.Instance.Log("[Decode Command] End.");
            }
        }

        protected override void OnDispose(bool disposing)
        {
            DataPackets.Clear();
        }
    }
}