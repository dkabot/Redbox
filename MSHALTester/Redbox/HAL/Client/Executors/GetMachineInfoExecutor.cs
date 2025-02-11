using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Executors;

public sealed class GetMachineInfoExecutor : JobExecutor
{
    public GetMachineInfoExecutor(HardwareService s)
        : base(s)
    {
        Info = new MachineInformation();
    }

    public MachineInformation Info { get; }

    protected override string JobName => "get-machine-info";

    protected override void DisposeInner()
    {
        Info.Dispose();
    }

    protected override void OnJobCompleted()
    {
        var list = new List<DeckConfiguration>();
        try
        {
            var flag = false;
            foreach (var result in Results)
                switch (result.Code)
                {
                    case "QlmInfo":
                        flag = true;
                        LocateOrCreate(result.Deck, list).IsQlm = true;
                        continue;
                    case "DeckInfo":
                        LocateOrCreate(result.Deck, list).SlotCount = result.Slot;
                        continue;
                    default:
                        continue;
                }

            list.Sort((x, y) => x.Number.CompareTo(y.Number));
            Info.Configuration =
                flag
                    ? list[0].SlotCount == 90 ? KioskConfiguration.R630 : KioskConfiguration.R504
                    : KioskConfiguration.R717;
            list.ForEach(each => Info.DecksConfiguration.Add(each));
        }
        finally
        {
            list.Clear();
        }
    }

    private DeckConfiguration LocateOrCreate(
        int deck,
        List<DeckConfiguration> list)
    {
        var deckConfiguration = list.Find(each => each.Number == deck);
        if (deckConfiguration == null)
        {
            deckConfiguration = new DeckConfiguration
            {
                Number = deck
            };
            list.Add(deckConfiguration);
        }

        return deckConfiguration;
    }

    private class DeckConfiguration : IDeckConfig
    {
        public int Number { get; internal set; }

        public int SlotCount { get; internal set; }

        public bool IsQlm { get; internal set; }
    }
}