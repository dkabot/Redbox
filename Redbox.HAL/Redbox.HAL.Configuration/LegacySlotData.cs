using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Configuration
{
    public sealed class LegacySlotData : IConfigurationFile, IDisposable
    {
        private bool Disposed;

        public LegacySlotData()
            : this(true)
        {
        }

        public LegacySlotData(bool removeZeroes)
        {
            FullSourcePath = System.IO.Path.Combine(Path, FileName);
            SlotData = new List<PlatterConfig>();
            if (File.Exists(FullSourcePath))
                ReadLegacyFile(removeZeroes);
            KioskConfig = KioskConfiguration.None;
            if (SlotData.Count <= 0)
                return;
            if (SlotData[1].Type == PlatterType.Sparse)
                KioskConfig = KioskConfiguration.R504;
            else
                KioskConfig = SlotData[SlotData.Count - 1].Type == PlatterType.Qlm
                    ? KioskConfiguration.R630
                    : KioskConfiguration.R717;
        }

        public List<PlatterConfig> SlotData { get; }

        public KioskConfiguration KioskConfig { get; private set; }

        public SystemConfigurations Type => SystemConfigurations.SlotData;

        public string Path => "c:\\Gamp";

        public string FileName => "SlotData.dat";

        public string FullSourcePath { get; }

        public void ImportFrom(IConfigurationFile config, ErrorList errors)
        {
            throw new NotImplementedException();
        }

        public ConversionResult ConvertTo(KioskConfiguration newConfig, ErrorList errors)
        {
            if (SlotData.Count == 0)
                return ConversionResult.InvalidFile;
            if (KioskConfiguration.R504 == KioskConfig)
                return ConversionResult.UnsupportedConversion;
            return KioskConfiguration.R630 == KioskConfig && (!ConvertToVMZ() || !WriteSlotData())
                ? ConversionResult.Failure
                : ConversionResult.Success;
        }

        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            SlotData.Clear();
        }

        public bool WriteSlotData()
        {
            if (SlotData.Count == 0)
                return false;
            using (var streamWriter = new StreamWriter(FullSourcePath))
            {
                foreach (var platterConfig in SlotData)
                    streamWriter.WriteLine(platterConfig.Data.ToString());
            }

            return true;
        }

        private bool ConvertToVMZ()
        {
            if (SlotData.Count == 0)
                return false;
            SlotData[SlotData.Count - 1].Data.SegmentOffsets = SlotData[SlotData.Count - 2].Data.SegmentOffsets;
            KioskConfig = KioskConfiguration.R717;
            return true;
        }

        private void ReadLegacyFile(bool removeZeroes)
        {
            var num = 1;
            var flag = false;
            var stringBuilder = new StringBuilder();
            foreach (var readAllLine in File.ReadAllLines(FullSourcePath))
            {
                var data = new List<int>();
                int result;
                for (var index = 0; index < readAllLine.Length; ++index)
                    if (char.IsWhiteSpace(readAllLine[index]) || readAllLine[index] == ',')
                    {
                        if (stringBuilder.Length > 0)
                        {
                            if (!int.TryParse(stringBuilder.ToString(), out result))
                            {
                                result = 0;
                                LogHelper.Instance.Log(LogEntryType.Error,
                                    "The value {0} in slotdata.dat (line {1}) is invalid", stringBuilder.ToString(),
                                    num);
                                flag = true;
                            }
                            else
                            {
                                data.Add(result);
                            }

                            stringBuilder.Remove(0, stringBuilder.Length);
                        }
                    }
                    else
                    {
                        stringBuilder.Append(readAllLine[index]);
                    }

                if (stringBuilder.Length > 0)
                {
                    if (!int.TryParse(stringBuilder.ToString(), out result))
                    {
                        result = 0;
                        LogHelper.Instance.Log(
                            string.Format("The value {0} in slotdata.dat (line {1}) is invalid",
                                stringBuilder, num), LogEntryType.Error);
                        flag = true;
                    }
                    else
                    {
                        data.Add(result);
                    }

                    stringBuilder.Remove(0, stringBuilder.Length);
                }

                ++num;
                if (data.Count > 0)
                    SlotData.Add(PlatterConfig.Get(new PlatterData(data, removeZeroes)));
            }

            if (!flag)
                return;
            SlotData.Clear();
        }
    }
}