namespace Redbox.HAL.Client.Executors
{
    public sealed class RebuildInventoryExecutor : JobExecutor
    {
        public RebuildInventoryExecutor(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "rebuild-inventory-database";
    }
}