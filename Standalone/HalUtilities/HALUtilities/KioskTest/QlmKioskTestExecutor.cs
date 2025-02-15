using Redbox.HAL.Client;

namespace HALUtilities.KioskTest
{
    internal sealed class QlmKioskTestExecutor : JobExecutor
    {
        private readonly TestLocation Destination;
        private readonly bool m_runQlm;
        private readonly bool m_vend;
        private readonly TestLocation Source;

        internal QlmKioskTestExecutor(
            HardwareService service,
            TestLocation source,
            TestLocation dest,
            bool vend,
            bool runQlm)
            : base(service)
        {
            Source = source;
            Destination = dest;
            m_vend = vend;
            m_runQlm = runQlm;
        }

        protected override string JobName => "qlm-kiosk-test";

        protected override void SetupJob()
        {
            Job.Push(Destination.Slot);
            Job.Push(Destination.Deck);
            Job.Push(Source.Slot);
            Job.Push(Source.Deck);
            Job.Push(m_vend.ToString());
            Job.Push(m_runQlm.ToString());
        }
    }
}