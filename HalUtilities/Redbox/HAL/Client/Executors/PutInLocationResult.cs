namespace Redbox.HAL.Client.Executors
{
  public sealed class PutInLocationResult : JobExecutor
  {
    private readonly int Deck;
    private readonly int Slot;

    public PutInLocationResult(HardwareService service, int deck, int slot)
      : base(service)
    {
      this.Deck = deck;
      this.Slot = slot;
    }

    protected override void SetupJob() => this.Job.Push((object) this.Deck, (object) this.Slot);

    protected override string JobName => "put-disk-to-location";
  }
}
