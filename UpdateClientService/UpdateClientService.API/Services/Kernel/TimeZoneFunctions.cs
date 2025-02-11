using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace UpdateClientService.API.Services.Kernel
{
    public static class TimeZoneFunctions
    {
        public enum SetTimeResult
        {
            NotSet,
            InRange,
            Changed,
            Errored
        }

        public enum SetTimeZoneResult
        {
            NotSet,
            Same,
            Changed,
            Errored
        }

        [DllImport("kernel32.dll", EntryPoint = "SetSystemTime", SetLastError = true)]
        private static extern bool Win32SetSystemTime(ref SystemTime sysTime);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool SetDynamicTimeZoneInformation(
            ref DynamicTimeZoneInformation lpTimeZoneInformation);

        private static SystemTime ToSystemTime(this TimeZoneInfo.TransitionTime input)
        {
            var systemTime1 = new SystemTime();
            systemTime1.Year = 0;
            systemTime1.Month = (ushort)input.Month;
            systemTime1.Day = input.IsFixedDateRule ? (ushort)input.Day : (ushort)input.Week;
            systemTime1.Hour = (ushort)input.TimeOfDay.Hour;
            ref var local1 = ref systemTime1;
            var timeOfDay = input.TimeOfDay;
            var second = (int)(ushort)timeOfDay.Second;
            local1.Second = (ushort)second;
            ref var local2 = ref systemTime1;
            timeOfDay = input.TimeOfDay;
            var minute = (int)(ushort)timeOfDay.Minute;
            local2.Minute = (ushort)minute;
            ref var local3 = ref systemTime1;
            timeOfDay = input.TimeOfDay;
            var millisecond = (int)(ushort)timeOfDay.Millisecond;
            local3.Millisecond = (ushort)millisecond;
            var systemTime2 = systemTime1;
            switch (input.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    systemTime2.DayOfWeek = 0;
                    break;
                case DayOfWeek.Monday:
                    systemTime2.DayOfWeek = 1;
                    break;
                case DayOfWeek.Tuesday:
                    systemTime2.DayOfWeek = 2;
                    break;
                case DayOfWeek.Wednesday:
                    systemTime2.DayOfWeek = 3;
                    break;
                case DayOfWeek.Thursday:
                    systemTime2.DayOfWeek = 4;
                    break;
                case DayOfWeek.Friday:
                    systemTime2.DayOfWeek = 5;
                    break;
                case DayOfWeek.Saturday:
                    systemTime2.DayOfWeek = 6;
                    break;
            }

            return systemTime2;
        }

        private static void SetAdjustmentRules(
            ref DynamicTimeZoneInformation info,
            TimeZoneInfo.AdjustmentRule currentAdjustmentRule)
        {
            if (currentAdjustmentRule == null)
            {
                var systemTime = new SystemTime
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

        public static SetTimeResult SetTime(DateTime dateTime)
        {
            var utcNow = DateTime.UtcNow;
            SetTimeResult setTimeResult;
            if (dateTime.AddMinutes(-5.0) < utcNow && utcNow < dateTime.AddMinutes(5.0))
            {
                setTimeResult = SetTimeResult.InRange;
            }
            else
            {
                var sysTime = new SystemTime
                {
                    Year = (ushort)dateTime.Year,
                    Month = (ushort)dateTime.Month,
                    Day = (ushort)dateTime.Day,
                    Hour = (ushort)dateTime.Hour,
                    Minute = (ushort)dateTime.Minute,
                    Second = (ushort)dateTime.Second
                };
                setTimeResult = !Win32SetSystemTime(ref sysTime) ? SetTimeResult.Errored : SetTimeResult.Changed;
            }

            return setTimeResult;
        }

        public static SetTimeZoneResult SetTimeZone(TimeZoneInfo timeZoneInfo)
        {
            SetTimeZoneResult setTimeZoneResult;
            if (TimeZoneInfo.Local.Equals(timeZoneInfo))
            {
                setTimeZoneResult = SetTimeZoneResult.Same;
            }
            else
            {
                var adjustmentRules = timeZoneInfo.GetAdjustmentRules();
                var utcNow = DateTime.UtcNow;
                var timeZoneInformation = new DynamicTimeZoneInformation
                {
                    StandardName = timeZoneInfo.StandardName,
                    DaylightName = timeZoneInfo.DaylightName,
                    StandardBias = 0,
                    Bias = (int)timeZoneInfo.BaseUtcOffset.Negate().TotalMinutes,
                    TimeZoneKeyName = timeZoneInfo.StandardName,
                    DynamicDaylightTimeDisabled = false
                };
                var currentAdjustmentRule =
                    adjustmentRules.FirstOrDefault(ar => ar.DateStart < utcNow && ar.DateEnd > utcNow);
                SetAdjustmentRules(ref timeZoneInformation, currentAdjustmentRule);
                TokenPrivilegesAccess.EnablePrivilege("SeTimeZonePrivilege");
                setTimeZoneResult = !SetDynamicTimeZoneInformation(ref timeZoneInformation)
                    ? SetTimeZoneResult.Errored
                    : SetTimeZoneResult.Changed;
                TokenPrivilegesAccess.DisablePrivilege("SeTimeZonePrivilege");
            }

            return setTimeZoneResult;
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

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DynamicTimeZoneInformation
        {
            [MarshalAs(UnmanagedType.I4)] public int Bias;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string StandardName;

            public SystemTime StandardDate;
            [MarshalAs(UnmanagedType.I4)] public int StandardBias;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DaylightName;

            public SystemTime DaylightDate;
            [MarshalAs(UnmanagedType.I4)] public int DaylightBias;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string TimeZoneKeyName;

            public bool DynamicDaylightTimeDisabled;
        }
    }
}