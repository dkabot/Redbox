namespace Redbox.HAL.Client.Executors
{
    public sealed class ClearImagesFolderExecutor : JobExecutor
    {
        public ClearImagesFolderExecutor(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "clear-images-folder";
    }
}