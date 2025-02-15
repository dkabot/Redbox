namespace Redbox.HAL.Client.Executors;

public sealed class UpdateQREEPROMJob(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "update-qr-eeprom";

    protected override string Label => "Utilities scheduled update";
}