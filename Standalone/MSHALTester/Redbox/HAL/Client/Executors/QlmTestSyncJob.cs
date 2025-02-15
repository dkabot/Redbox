namespace Redbox.HAL.Client.Executors;

public sealed class QlmTestSyncJob(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "qlm-test-sync";
}