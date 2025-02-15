using Redbox.HAL.Client;

namespace Redbox.HAL.MSHALTester;

internal sealed class ReadDiskJob : JobExecutor
{
    internal ReadDiskJob(HardwareService service)
        : base(service)
    {
    }

    protected override string JobName => "read-barcode";

    protected override string Label => "MS read barcode";
}