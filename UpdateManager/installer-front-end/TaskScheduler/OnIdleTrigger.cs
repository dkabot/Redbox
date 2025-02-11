using TaskSchedulerInterop;

namespace TaskScheduler
{
    internal class OnIdleTrigger : Trigger
    {
        public OnIdleTrigger() => this.taskTrigger.Type = TaskTriggerType.EVENT_TRIGGER_ON_IDLE;

        internal OnIdleTrigger(ITaskTrigger iTrigger)
          : base(iTrigger)
        {
        }
    }
}
