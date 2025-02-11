namespace Redbox.HAL.Client.Executors;

public sealed class BoardTestJob(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "test-boards-job";
}