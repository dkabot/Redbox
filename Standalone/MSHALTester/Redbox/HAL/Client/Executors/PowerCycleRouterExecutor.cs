namespace Redbox.HAL.Client.Executors;

public sealed class PowerCycleRouterExecutor(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "power-cycle-router";
}