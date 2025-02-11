namespace Redbox.UpdateService.Model
{
    public class StoreScheduleInfo
    {
        public byte EndOfScheduleInHoursFromMidnight;
        public int MaxBandwidthWhileOutsideOfSchedule;
        public int MaxBandwidthWhileWithInSchedule;
        public uint NoProgressTimeout;
        public byte StartOfScheduleInHoursFromMidnight;
    }
}