namespace Redbox.HAL.Client.Executors
{
  public sealed class OneDiskRandomSyncExecutor : JobExecutor
  {
    private readonly int VendTime;
    private readonly int VendFrequency;

    public OneDiskRandomSyncExecutor(HardwareService service, int vendTime, int frequency)
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

    protected override string JobName => "one-disk-random-sync";
  }
}
