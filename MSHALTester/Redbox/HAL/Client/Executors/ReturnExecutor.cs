namespace Redbox.HAL.Client.Executors;

public sealed class ReturnExecutor(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "return";
}