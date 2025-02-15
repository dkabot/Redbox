namespace Redbox.HAL.Client.Executors
{
    public sealed class ResetMotionControlExecutor : JobExecutor
    {
        public ResetMotionControlExecutor(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "reset-motioncontrol";
    }
}