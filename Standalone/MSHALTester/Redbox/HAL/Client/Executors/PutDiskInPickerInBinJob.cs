namespace Redbox.HAL.Client.Executors;

public sealed class PutDiskInPickerInBinJob(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "put-disk-in-picker-in-bin";
}