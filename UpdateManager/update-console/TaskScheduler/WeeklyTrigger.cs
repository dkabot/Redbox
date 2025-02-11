using TaskSchedulerInterop;

namespace TaskScheduler
{
    internal class WeeklyTrigger : StartableTrigger
    {
        public WeeklyTrigger(
          short hour,
          short minutes,
          DaysOfTheWeek daysOfTheWeek,
          short weeksInterval)
        {
            this.SetStartTime((ushort)hour, (ushort)minutes);
            this.taskTrigger.Type = TaskTriggerType.TIME_TRIGGER_WEEKLY;
            this.taskTrigger.Data.weekly.WeeksInterval = (ushort)weeksInterval;
            this.taskTrigger.Data.weekly.DaysOfTheWeek = (ushort)daysOfTheWeek;
        }

        public WeeklyTrigger(short hour, short minutes, DaysOfTheWeek daysOfTheWeek)
          : this(hour, minutes, daysOfTheWeek, (short)1)
        {
        }

        internal WeeklyTrigger(ITaskTrigger iTrigger)
          : base(iTrigger)
        {
        }

        public short WeeksInterval
        {
            get => (short)this.taskTrigger.Data.weekly.WeeksInterval;
            set
            {
                this.taskTrigger.Data.weekly.WeeksInterval = (ushort)value;
                this.SyncTrigger();
            }
        }

        public DaysOfTheWeek WeekDays
        {
            get => (DaysOfTheWeek)this.taskTrigger.Data.weekly.DaysOfTheWeek;
            set
            {
                this.taskTrigger.Data.weekly.DaysOfTheWeek = (ushort)value;
                this.SyncTrigger();
            }
        }
    }
}
