using System;
using TaskSchedulerInterop;

namespace TaskScheduler
{
    internal abstract class StartableTrigger : Trigger
    {
        internal StartableTrigger()
        {
        }

        internal StartableTrigger(ITaskTrigger iTrigger)
          : base(iTrigger)
        {
        }

        protected void SetStartTime(ushort hour, ushort minute)
        {
            this.StartHour = (short)hour;
            this.StartMinute = (short)minute;
        }

        public short StartHour
        {
            get => (short)this.taskTrigger.StartHour;
            set
            {
                this.taskTrigger.StartHour = value >= (short)0 && value <= (short)23 ? (ushort)value : throw new ArgumentOutOfRangeException("hour", (object)value, "hour must be between 0 and 23");
                this.SyncTrigger();
            }
        }

        public short StartMinute
        {
            get => (short)this.taskTrigger.StartMinute;
            set
            {
                this.taskTrigger.StartMinute = value >= (short)0 && value <= (short)59 ? (ushort)value : throw new ArgumentOutOfRangeException("minute", (object)value, "minute must be between 0 and 59");
                this.SyncTrigger();
            }
        }
    }
}
