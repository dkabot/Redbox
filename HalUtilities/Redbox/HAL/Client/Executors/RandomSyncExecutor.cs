namespace Redbox.HAL.Client.Executors
{
  public sealed class RandomSyncExecutor : JobExecutor
  {
    private readonly int VendTime;
    private readonly int VendFrequency;

    public RandomSyncExecutor(HardwareService service, int vendTime, int frequency)
      : base(service)
    {
      this.VendTime = vendTime;
      this.VendFrequency = frequency;
    }

    protected override void SetupJob()
    {
      this.Job.Push((object) this.VendTime);
      this.Job.Push((object) this.VendFrequency);
    }

    protected override string JobName => "random-sync";
  }
}
