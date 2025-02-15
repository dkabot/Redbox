namespace Redbox.HAL.Client.Executors;

public sealed class ResetControlSystemJob(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "reset-controlsystem-job";
}