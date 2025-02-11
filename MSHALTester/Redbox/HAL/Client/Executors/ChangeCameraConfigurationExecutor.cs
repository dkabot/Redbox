using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Executors;

public sealed class ChangeCameraConfigurationExecutor : JobExecutor
{
    private readonly ScannerServices ChangeTo;

    public ChangeCameraConfigurationExecutor(HardwareService service, ScannerServices newService)
        : base(service)
    {
        ChangeTo = newService;
    }

    protected override string JobName => "change-camera-configuration";

    protected override void SetupJob()
    {
        Job.Push(ChangeTo.ToString());
    }
}