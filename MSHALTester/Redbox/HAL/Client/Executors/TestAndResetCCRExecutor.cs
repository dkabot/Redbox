namespace Redbox.HAL.Client.Executors;

public sealed class TestAndResetCCRExecutor(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "test-and-reset-ccr";
}