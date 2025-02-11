using TaskSchedulerInterop;

namespace TaskScheduler
{
    internal class OnSystemStartTrigger : Trigger
    {
        public OnSystemStartTrigger()
        {
            this.taskTrigger.Type = TaskTriggerType.EVENT_TRIGGER_AT_SYSTEMSTART;
        }

        internal OnSystemStartTrigger(ITaskTrigger iTrigger)
          : base(iTrigger)
        {
        }
    }
}
