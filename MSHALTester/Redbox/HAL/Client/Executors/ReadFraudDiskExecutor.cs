namespace Redbox.HAL.Client.Executors;

public sealed class ReadFraudDiskExecutor(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "read-fraud-disc";
}