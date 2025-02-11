namespace Redbox.HAL.Client.Executors;

public sealed class ResetFraudSensorJob(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "reset-fraud-sensor";
}