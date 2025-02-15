using Redbox.HAL.Client;

namespace Redbox.HAL.MSHALTester;

internal class DecodeExecutor : JobExecutor
{
    private readonly string File;

    internal DecodeExecutor(HardwareService service, string file)
        : base(service)
    {
        File = file;
    }

    protected override string JobName => "decode-image";

    internal ScanResult ScanResult { get; private set; }

    protected override void SetupJob()
    {
        Job.Push(File);
    }

    protected override void OnJobCompleted()
    {
        ScanResult = ScanResult.From(Results);
    }
}