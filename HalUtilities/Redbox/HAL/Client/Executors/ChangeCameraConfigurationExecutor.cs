using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Executors
{
  public sealed class ChangeCameraConfigurationExecutor : JobExecutor
  {
    private readonly ScannerServices ChangeTo;

    public ChangeCameraConfigurationExecutor(HardwareService service, ScannerServices newService)
      : base(service)
    {
      this.ChangeTo = newService;
    }

    protected override void SetupJob() => this.Job.Push((object) this.ChangeTo.ToString());

    protected override string JobName => "change-camera-configuration";
  }
}
