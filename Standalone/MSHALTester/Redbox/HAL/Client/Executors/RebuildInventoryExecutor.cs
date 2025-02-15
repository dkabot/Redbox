namespace Redbox.HAL.Client.Executors;

public sealed class RebuildInventoryExecutor(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "rebuild-inventory-database";
}