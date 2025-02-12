namespace Redbox.HAL.Client.Executors
{
  public sealed class GetAndReadExecutor : JobExecutor
  {
    private readonly int Deck;
    private readonly int Slot;
    private readonly bool Center;

    public GetAndReadExecutor(HardwareService service, int deck, int slot, bool center)
      : base(service)
    {
      this.Deck = deck;
      this.Slot = slot;
      this.Center = center;
    }

    protected override void SetupJob()
    {
      this.Job.Push((object) this.Center.ToString(), (object) this.Deck, (object) this.Slot);
    }

    protected override string JobName => "get-and-read";
  }
}
