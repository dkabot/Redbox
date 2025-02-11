namespace Redbox.HAL.Client.Executors;

public sealed class ClearImagesFolderExecutor(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "clear-images-folder";
}