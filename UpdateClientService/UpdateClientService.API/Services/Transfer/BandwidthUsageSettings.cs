using System;
using Microsoft.Win32;

namespace UpdateClientService.API.Services.Transfer
{
    public static class BandwidthUsageSettings
    {
        private const string m_registryPath = "SOFTWARE\\Policies\\Microsoft\\Windows\\BITS";

        static BandwidthUsageSettings()
        {
            EnableMaximumBandwitdthThrottle = true;
            UseSystemMaxOutsideOfSchedule = false;
        }

        public static int MaxBandwidthWhileWithInSchedule
        {
            get => Get<int>("MaxTransferRateOnSchedule");
            set => Set(value, "MaxTransferRateOnSchedule");
        }

        public static int MaxBandwidthWhileOutsideOfSchedule
        {
            get => Get<int>("MaxTransferRateOffSchedule");
            set => Set(value, "MaxTransferRateOffSchedule");
        }

        public static byte StartOfScheduleInHoursFromMidnight
        {
            get => (byte)Get<int>("MaxBandwidthValidFrom");
            set => Set((int)value, "MaxBandwidthValidFrom");
        }

        public static byte EndOfScheduleInHoursFromMidnight
        {
            get => (byte)Get<int>("MaxBandwidthValidTo");
            set => Set((int)value, "MaxBandwidthValidTo");
        }

        public static bool UseSystemMaxOutsideOfSchedule
        {
            get => Get<bool>("UseSystemMaximum");
            set => Set(value, "UseSystemMaximum");
        }

        public static bool EnableMaximumBandwitdthThrottle
        {
            get => Get<bool>("EnableBITSMaxBandwidth");
            set => Set(value, "EnableBITSMaxBandwidth");
        }

        private static T Get<T>(string keyName)
        {
            using (var subKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\BITS"))
            {
                var obj = subKey.GetValue(keyName);
                return obj == null ? default : (T)Convert.ChangeType(obj, typeof(T));
            }
        }

        private static void Set(object value, string keyName)
        {
            using (var subKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\BITS"))
            {
                subKey.SetValue(keyName, value);
            }
        }
    }
}