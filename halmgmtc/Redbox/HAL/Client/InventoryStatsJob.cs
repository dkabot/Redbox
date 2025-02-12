namespace Redbox.HAL.Client
{
    public sealed class InventoryStatsJob : JobExecutor
    {
        public InventoryStatsJob(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "get-inventory-stats";
    }
}