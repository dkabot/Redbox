namespace Redbox.HAL.Client
{
    public sealed class ResetControlSystemJob : JobExecutor
    {
        public ResetControlSystemJob(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "reset-controlsystem-job";
    }
}