namespace Redbox.HAL.Client
{
    public sealed class QlmTestSyncJob : JobExecutor
    {
        public QlmTestSyncJob(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "qlm-test-sync";
    }
}