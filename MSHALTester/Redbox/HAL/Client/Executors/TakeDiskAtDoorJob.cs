namespace Redbox.HAL.Client.Executors;

public sealed class TakeDiskAtDoorJob(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "ms-take-disk-at-door";
}