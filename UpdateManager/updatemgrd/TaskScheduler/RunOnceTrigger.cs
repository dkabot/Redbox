using System;
using TaskSchedulerInterop;

namespace TaskScheduler
{
    internal class RunOnceTrigger : StartableTrigger
    {
        public RunOnceTrigger(DateTime runDateTime)
        {
            this.taskTrigger.BeginYear = (ushort)runDateTime.Year;
            this.taskTrigger.BeginMonth = (ushort)runDateTime.Month;
            this.taskTrigger.BeginDay = (ushort)runDateTime.Day;
            this.SetStartTime((ushort)runDateTime.Hour, (ushort)runDateTime.Minute);
            this.taskTrigger.Type = TaskTriggerType.TIME_TRIGGER_ONCE;
        }

        internal RunOnceTrigger(ITaskTrigger iTrigger)
          : base(iTrigger)
        {
        }
    }
}
