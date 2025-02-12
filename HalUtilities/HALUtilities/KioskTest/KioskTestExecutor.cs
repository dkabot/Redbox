using Redbox.HAL.Client;

namespace HALUtilities.KioskTest
{
  internal sealed class KioskTestExecutor : JobExecutor
  {
    private readonly TestLocation Source;
    private readonly TestLocation Destination;
    private readonly bool m_vend;

    internal KioskTestExecutor(
      HardwareService service,
      TestLocation source,
      TestLocation dest,
      bool vend)
      : base(service)
    {
      this.Source = source;
      this.Destination = dest;
      this.m_vend = vend;
    }

    protected override void SetupJob()
    {
      this.Job.Push((object) this.Destination.Slot);
      this.Job.Push((object) this.Destination.Deck);
      this.Job.Push((object) this.Source.Slot);
      this.Job.Push((object) this.Source.Deck);
      this.Job.Push((object) this.m_vend.ToString());
    }

    protected override string JobName => "refurbish-kiosk-test";
  }
}
