using Redbox.HAL.Client;

namespace HALUtilities.KioskTest
{
  internal sealed class QlmKioskTestExecutor : JobExecutor
  {
    private readonly TestLocation Source;
    private readonly TestLocation Destination;
    private readonly bool m_vend;
    private readonly bool m_runQlm;

    internal QlmKioskTestExecutor(
      HardwareService service,
      TestLocation source,
      TestLocation dest,
      bool vend,
      bool runQlm)
      : base(service)
    {
      this.Source = source;
      this.Destination = dest;
      this.m_vend = vend;
      this.m_runQlm = runQlm;
    }

    protected override void SetupJob()
    {
      this.Job.Push((object) this.Destination.Slot);
      this.Job.Push((object) this.Destination.Deck);
      this.Job.Push((object) this.Source.Slot);
      this.Job.Push((object) this.Source.Deck);
      this.Job.Push((object) this.m_vend.ToString());
      this.Job.Push((object) this.m_runQlm.ToString());
    }

    protected override string JobName => "qlm-kiosk-test";
  }
}
