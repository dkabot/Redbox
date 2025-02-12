using Redbox.HAL.Client;

namespace HALUtilities.KioskTest
{
    internal sealed class KioskTestExecutor : JobExecutor
    {
        private readonly TestLocation Destination;
        private readonly bool m_vend;
        private readonly TestLocation Source;

        internal KioskTestExecutor(
            HardwareService service,
            TestLocation source,
            TestLocation dest,
            bool vend)
            : base(service)
        {
            Source = source;
            Destination = dest;
            m_vend = vend;
        }

        protected override string JobName => "refurbish-kiosk-test";

        protected override void SetupJob()
        {
            Job.Push(Destination.Slot);
            Job.Push(Destination.Deck);
            Job.Push(Source.Slot);
            Job.Push(Source.Deck);
            Job.Push(m_vend.ToString());
        }
    }
}