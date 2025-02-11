using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.MSHALTester;

internal sealed class PickAtOffsetExecutor : JobExecutor
{
    private readonly Axis Axis;
    private readonly int Deck;
    private readonly int Offset;
    private readonly int Slot;

    internal PickAtOffsetExecutor(
        HardwareService service,
        int deck,
        int slot,
        Axis axis,
        int offset)
        : base(service)
    {
        Deck = deck;
        Slot = slot;
        Axis = axis;
        Offset = offset;
    }

    protected override string JobName => "pick-at-offset";

    protected override void SetupJob()
    {
        Job.Push(Offset);
        Job.Push(Axis.ToString());
        Job.Push(Slot);
        Job.Push(Deck);
    }
}