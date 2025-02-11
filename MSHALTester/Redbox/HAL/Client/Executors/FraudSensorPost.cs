namespace Redbox.HAL.Client.Executors;

public sealed class FraudSensorPost(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "fraud-sensor-post-test";
}