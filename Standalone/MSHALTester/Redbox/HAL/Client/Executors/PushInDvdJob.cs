namespace Redbox.HAL.Client.Executors;

public sealed class PushInDvdJob(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "push-in-dvd";
}