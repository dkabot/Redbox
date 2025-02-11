namespace Redbox.HAL.Client.Executors;

public sealed class AdjacentBinTest(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "adjacent-bin-test";
}