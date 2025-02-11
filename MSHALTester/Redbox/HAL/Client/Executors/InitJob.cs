namespace Redbox.HAL.Client.Executors;

public sealed class InitJob(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "init";

    protected override string Label => "MS Tester Init";
}