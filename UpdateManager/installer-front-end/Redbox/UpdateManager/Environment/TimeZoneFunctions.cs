using Microsoft.Win32;
using Redbox.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Redbox.UpdateManager.Environment
{
    internal static class TimeZoneFunctions
    {
        private static readonly bool IsOsVistaOrGreater = System.Environment.OSVersion.Version.Major >= 6;

        public static bool SetTime(DateTime dateTime)
        {
            if (dateTime.AddMinutes(-5.0) < DateTime.UtcNow && DateTime.UtcNow < dateTime.AddMinutes(5.0))
            {
                LogHelper.Instance.Log("Not changing time because the set time and the current time are within 5 minutes.", LogEntryType.Debug);
                return true;
            }
            try
            {
                TimeZoneFunctions.SystemTime sysTime = new TimeZoneFunctions.SystemTime()
                {
                    Year = (ushort)dateTime.Year,
                    Month = (ushort)dateTime.Month,
                    Day = (ushort)dateTime.Day,
                    Hour = (ushort)dateTime.Hour,
                    Minute = (ushort)dateTime.Minute,
                    Second = (ushort)dateTime.Second
                };
                LogHelper instance1 = LogHelper.Instance;
                string[] strArray1 = new string[8];
                strArray1[0] = "Attempting to change date and time (UTC) from ";
                DateTime utcNow = DateTime.UtcNow;
                strArray1[1] = utcNow.ToLongDateString();
                strArray1[2] = " ";
                utcNow = DateTime.UtcNow;
                strArray1[3] = utcNow.ToLongTimeString();
                strArray1[4] = " to ";
                strArray1[5] = dateTime.ToLongDateString();
                strArray1[6] = " ";
                strArray1[7] = dateTime.ToLongTimeString();
                string message1 = string.Concat(strArray1);
                instance1.Log(message1, LogEntryType.Debug);
                bool flag = TimeZoneFunctions.Win32SetSystemTime(ref sysTime);
                LogHelper instance2 = LogHelper.Instance;
                string[] strArray2 = new string[6]
                {
          "Timechange success: ",
          flag.ToString(),
          " - current date and time (UTC) is: ",
          null,
          null,
          null
                };
                utcNow = DateTime.UtcNow;
                strArray2[3] = utcNow.ToLongDateString();
                strArray2[4] = " ";
                utcNow = DateTime.UtcNow;
                strArray2[5] = utcNow.ToLongTimeString();
                string message2 = string.Concat(strArray2);
                instance2.Log(message2, LogEntryType.Debug);
                return flag;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Unhandled exception in TimeZoneFunctions.SetTime", ex);
                return false;
            }
        }

        public static bool SetTimeZone(TimeZoneInfo timeZoneInfo)
        {
            if (TimeZoneInfo.Local.Equals(timeZoneInfo))
                return true;
            try
            {
                TimeZoneInfo.AdjustmentRule currentAdjustmentRule = ((IEnumerable<TimeZoneInfo.AdjustmentRule>)timeZoneInfo.GetAdjustmentRules()).FirstOrDefault<TimeZoneInfo.AdjustmentRule>((Func<TimeZoneInfo.AdjustmentRule, bool>)(ar => ar.DateStart < DateTime.UtcNow && ar.DateEnd > DateTime.UtcNow));
                LogHelper.Instance.Log("Attempting to change timezone from " + TimeZoneInfo.Local.DisplayName + " to " + timeZoneInfo.DisplayName, LogEntryType.Debug);
                bool flag;
                if (TimeZoneFunctions.IsOsVistaOrGreater)
                {
                    TimeZoneFunctions.DynamicTimeZoneInformation timeZoneInformation = new TimeZoneFunctions.DynamicTimeZoneInformation()
                    {
                        StandardName = timeZoneInfo.StandardName,
                        DaylightName = timeZoneInfo.DaylightName,
                        StandardBias = 0,
                        Bias = (int)timeZoneInfo.BaseUtcOffset.Negate().TotalMinutes,
                        TimeZoneKeyName = timeZoneInfo.StandardName,
                        DynamicDaylightTimeDisabled = false
                    };
                    TimeZoneFunctions.SetAdjustmentRules(ref timeZoneInformation, currentAdjustmentRule);
                    TokenPrivilegesAccess.EnablePrivilege("SeTimeZonePrivilege");
                    flag = TimeZoneFunctions.SetDynamicTimeZoneInformation(ref timeZoneInformation);
                    TokenPrivilegesAccess.DisablePrivilege("SeTimeZonePrivilege");
                }
                else
                {
                    TimeZoneFunctions.TimeZoneInformation timeZoneInformation = new TimeZoneFunctions.TimeZoneInformation()
                    {
                        StandardName = timeZoneInfo.StandardName,
                        DaylightName = timeZoneInfo.DaylightName,
                        StandardBias = 0,
                        Bias = (int)timeZoneInfo.BaseUtcOffset.Negate().TotalMinutes
                    };
                    TimeZoneFunctions.SetAdjustmentRules(ref timeZoneInformation, currentAdjustmentRule);
                    flag = TimeZoneFunctions.SetTimeZoneInformation(ref timeZoneInformation);
                    TimeZoneFunctions.DeleteDisableAutoDaylightTimeSet();
                }
                LogHelper.Instance.Log("TimeZone change success: " + flag.ToString(), LogEntryType.Debug);
                return flag;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Unhandled exception in SetTimeZone", ex, LogEntryType.Error);
                return false;
            }
        }

        private static void SetAdjustmentRules(
          ref TimeZoneFunctions.TimeZoneInformation info,
          TimeZoneInfo.AdjustmentRule currentAdjustmentRule)
        {
            if (currentAdjustmentRule == null)
            {
                TimeZoneFunctions.SystemTime systemTime = new TimeZoneFunctions.SystemTime()
                {
                    Year = 0,
                    Month = 0,
                    DayOfWeek = 0,
                    Day = 0,
                    Hour = 0,
                    Minute = 0,
                    Second = 0,
                    Millisecond = 0
                };
                info.StandardDate = systemTime;
                info.DaylightDate = systemTime;
                info.DaylightBias = 0;
            }
            else
            {
                info.StandardDate = currentAdjustmentRule.DaylightTransitionEnd.ToSystemTime();
                info.DaylightDate = currentAdjustmentRule.DaylightTransitionStart.ToSystemTime();
                info.DaylightBias = (int)currentAdjustmentRule.DaylightDelta.Negate().TotalMinutes;
            }
        }

        private static void SetAdjustmentRules(
          ref TimeZoneFunctions.DynamicTimeZoneInformation info,
          TimeZoneInfo.AdjustmentRule currentAdjustmentRule)
        {
            if (currentAdjustmentRule == null)
            {
                TimeZoneFunctions.SystemTime systemTime = new TimeZoneFunctions.SystemTime()
                {
                    Year = 0,
                    Month = 0,
                    DayOfWeek = 0,
                    Day = 0,
                    Hour = 0,
                    Minute = 0,
                    Second = 0,
                    Millisecond = 0
                };
                info.StandardDate = systemTime;
                info.DaylightDate = systemTime;
                info.DaylightBias = 0;
            }
            else
            {
                info.StandardDate = currentAdjustmentRule.DaylightTransitionEnd.ToSystemTime();
                info.DaylightDate = currentAdjustmentRule.DaylightTransitionStart.ToSystemTime();
                info.DaylightBias = (int)currentAdjustmentRule.DaylightDelta.Negate().TotalMinutes;
            }
        }

        private static TimeZoneFunctions.SystemTime ToSystemTime(this TimeZoneInfo.TransitionTime input)
        {
            try
            {
                TimeZoneFunctions.SystemTime systemTime1 = new TimeZoneFunctions.SystemTime();
                systemTime1.Year = (ushort)0;
                systemTime1.Month = (ushort)input.Month;
                systemTime1.Day = input.IsFixedDateRule ? (ushort)input.Day : (ushort)input.Week;
                systemTime1.Hour = (ushort)input.TimeOfDay.Hour;
                ref TimeZoneFunctions.SystemTime local1 = ref systemTime1;
                DateTime timeOfDay = input.TimeOfDay;
                int second = (int)(ushort)timeOfDay.Second;
                local1.Second = (ushort)second;
                ref TimeZoneFunctions.SystemTime local2 = ref systemTime1;
                timeOfDay = input.TimeOfDay;
                int minute = (int)(ushort)timeOfDay.Minute;
                local2.Minute = (ushort)minute;
                ref TimeZoneFunctions.SystemTime local3 = ref systemTime1;
                timeOfDay = input.TimeOfDay;
                int millisecond = (int)(ushort)timeOfDay.Millisecond;
                local3.Millisecond = (ushort)millisecond;
                TimeZoneFunctions.SystemTime systemTime2 = systemTime1;
                switch (input.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        systemTime2.DayOfWeek = (ushort)0;
                        break;
                    case DayOfWeek.Monday:
                        systemTime2.DayOfWeek = (ushort)1;
                        break;
                    case DayOfWeek.Tuesday:
                        systemTime2.DayOfWeek = (ushort)2;
                        break;
                    case DayOfWeek.Wednesday:
                        systemTime2.DayOfWeek = (ushort)3;
                        break;
                    case DayOfWeek.Thursday:
                        systemTime2.DayOfWeek = (ushort)4;
                        break;
                    case DayOfWeek.Friday:
                        systemTime2.DayOfWeek = (ushort)5;
                        break;
                    case DayOfWeek.Saturday:
                        systemTime2.DayOfWeek = (ushort)6;
                        break;
                }
                systemTime1 = systemTime2;
                return systemTime1;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Unhandles exception in TimeZoneInfo.TransitionTime.ToSystemTime.", ex);
                return new TimeZoneFunctions.SystemTime();
            }
        }

        private static void DeleteDisableAutoDaylightTimeSet()
        {
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\TimeZoneInformation", true);
                if (registryKey == null)
                    return;
                registryKey.DeleteValue("DisableAutoDaylightTimeSet", false);
                registryKey.Close();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("There was an error in deleting the DisableAutoDaylightTimeSet registry value", ex, LogEntryType.Error);
            }
        }

        [DllImport("Kernel32.dll", EntryPoint = "SetSystemTime", SetLastError = true)]
        private static extern bool Win32SetSystemTime(ref TimeZoneFunctions.SystemTime sysTime);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool SetTimeZoneInformation(
          ref TimeZoneFunctions.TimeZoneInformation lpTimeZoneInformation);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool SetDynamicTimeZoneInformation(
          ref TimeZoneFunctions.DynamicTimeZoneInformation lpTimeZoneInformation);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct TimeZoneInformation
        {
            [MarshalAs(UnmanagedType.I4)]
            public int Bias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string StandardName;
            public TimeZoneFunctions.SystemTime StandardDate;
            [MarshalAs(UnmanagedType.I4)]
            public int StandardBias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DaylightName;
            public TimeZoneFunctions.SystemTime DaylightDate;
            [MarshalAs(UnmanagedType.I4)]
            public int DaylightBias;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DynamicTimeZoneInformation
        {
            [MarshalAs(UnmanagedType.I4)]
            public int Bias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string StandardName;
            public TimeZoneFunctions.SystemTime StandardDate;
            [MarshalAs(UnmanagedType.I4)]
            public int StandardBias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DaylightName;
            public TimeZoneFunctions.SystemTime DaylightDate;
            [MarshalAs(UnmanagedType.I4)]
            public int DaylightBias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string TimeZoneKeyName;
            public bool DynamicDaylightTimeDisabled;
        }

        private struct SystemTime
        {
            public ushort Year;
            public ushort Month;
            public ushort DayOfWeek;
            public ushort Day;
            public ushort Hour;
            public ushort Minute;
            public ushort Second;
            public ushort Millisecond;
        }
    }
}
