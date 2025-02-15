namespace Redbox.HAL.Client.Executors;

public sealed class MSPullInDvdJob(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "ms-pull-in-dvd";
}