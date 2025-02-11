using TaskSchedulerInterop;

namespace TaskScheduler
{
    internal class OnLogonTrigger : Trigger
    {
        public OnLogonTrigger() => this.taskTrigger.Type = TaskTriggerType.EVENT_TRIGGER_AT_LOGON;

        internal OnLogonTrigger(ITaskTrigger iTrigger)
          : base(iTrigger)
        {
        }
    }
}
