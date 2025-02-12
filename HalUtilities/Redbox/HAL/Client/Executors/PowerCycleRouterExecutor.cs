namespace Redbox.HAL.Client.Executors
{
    public sealed class PowerCycleRouterExecutor : JobExecutor
    {
        public PowerCycleRouterExecutor(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "power-cycle-router";
    }
}