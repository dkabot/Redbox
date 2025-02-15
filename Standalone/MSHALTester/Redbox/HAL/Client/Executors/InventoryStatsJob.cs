namespace Redbox.HAL.Client.Executors;

public sealed class InventoryStatsJob(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "get-inventory-stats";
}