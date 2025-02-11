using System;

namespace TaskScheduler
{
    [Flags]
    internal enum DaysOfTheWeek : short
    {
        Sunday = 1,
        Monday = 2,
        Tuesday = 4,
        Wednesday = 8,
        Thursday = 16, // 0x0010
        Friday = 32, // 0x0020
        Saturday = 64, // 0x0040
    }
}
