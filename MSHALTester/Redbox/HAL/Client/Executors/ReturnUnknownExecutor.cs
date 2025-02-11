namespace Redbox.HAL.Client.Executors;

public sealed class ReturnUnknownExecutor(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "return-unknown";
}