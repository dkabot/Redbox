using Redbox.HAL.Client;

namespace Redbox.HAL.MSHALTester;

internal sealed class TransferDisk
{
    private readonly HardwareService Service;

    internal TransferDisk(HardwareService service)
    {
        Service = service;
    }

    internal string Transfer(TransferLocation source, TransferLocation target)
    {
        HardwareJob job;
        if (!Service.ExecuteImmediate(
                string.Format("XFER SRC-DECK={0} SRC-SLOT={1} DEST-DECK={2} DEST-SLOT={3}", source.Deck, source.Slot,
                    target.Deck, target.Slot), 120000, out job).Success)
            return "SERVICE COMM ERROR";
        var topOfStack = job.GetTopOfStack();
        return !string.IsNullOrEmpty(topOfStack) ? topOfStack : "SERVICE COMM ERROR";
    }
}