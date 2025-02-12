namespace Redbox.HAL.Client.Executors
{
  public sealed class VerticalSync : JobExecutor
  {
    private readonly int Slot;

    public VerticalSync(HardwareService service, int slot)
      : base(service)
    {
      this.Slot = slot;
    }

    protected override void SetupJob() => this.Job.Push((object) this.Slot);

    protected override string JobName => "vertical-sync";
  }
}
