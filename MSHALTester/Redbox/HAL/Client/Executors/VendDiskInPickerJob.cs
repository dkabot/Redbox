namespace Redbox.HAL.Client.Executors;

public sealed class VendDiskInPickerJob(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "ms-vend-disk-in-picker";
}