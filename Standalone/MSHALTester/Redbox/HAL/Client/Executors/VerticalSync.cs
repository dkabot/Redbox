namespace Redbox.HAL.Client.Executors;

public sealed class VerticalSync : JobExecutor
{
    private readonly int Slot;

    public VerticalSync(HardwareService service, int slot)
        : base(service)
    {
        Slot = slot;
    }

    protected override string JobName => "vertical-sync";

    protected override void SetupJob()
    {
        Job.Push(Slot);
    }
}