using Redbox.HAL.Client;

namespace Redbox.HAL.MSHALTester;

internal sealed class TesterMoveToSlotExecutor : JobExecutor
{
    private readonly int Deck;
    private readonly int Slot;

    internal TesterMoveToSlotExecutor(HardwareService service, int deck, int slot)
        : base(service)
    {
        Deck = deck;
        Slot = slot;
    }

    protected override string JobName => "tester-move-to-slot";

    protected override void SetupJob()
    {
        Job.Push(Slot);
        Job.Push(Deck);
    }
}