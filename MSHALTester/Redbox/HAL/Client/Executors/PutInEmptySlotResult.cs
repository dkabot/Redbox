namespace Redbox.HAL.Client.Executors;

public sealed class PutInEmptySlotResult(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "file-disk-in-picker";
}