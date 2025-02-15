namespace Redbox.HAL.Client.Executors;

public sealed class ResetMotionControlExecutor(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "reset-motioncontrol";
}