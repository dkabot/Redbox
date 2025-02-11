using TaskSchedulerInterop;

namespace TaskScheduler
{
    internal class DailyTrigger : StartableTrigger
    {
        public DailyTrigger(short hour, short minutes, short daysInterval)
        {
            this.SetStartTime((ushort)hour, (ushort)minutes);
            this.taskTrigger.Type = TaskTriggerType.TIME_TRIGGER_DAILY;
            this.taskTrigger.Data.daily.DaysInterval = (ushort)daysInterval;
        }

        public DailyTrigger(short hour, short minutes)
          : this(hour, minutes, (short)1)
        {
        }

        internal DailyTrigger(ITaskTrigger iTrigger)
          : base(iTrigger)
        {
        }

        public short DaysInterval
        {
            get => (short)this.taskTrigger.Data.daily.DaysInterval;
            set
            {
                this.taskTrigger.Data.daily.DaysInterval = (ushort)value;
                this.SyncTrigger();
            }
        }
    }
}
