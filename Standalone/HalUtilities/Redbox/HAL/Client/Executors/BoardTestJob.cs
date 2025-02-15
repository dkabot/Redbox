namespace Redbox.HAL.Client.Executors
{
    public sealed class BoardTestJob : JobExecutor
    {
        public BoardTestJob(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "test-boards-job";
    }
}