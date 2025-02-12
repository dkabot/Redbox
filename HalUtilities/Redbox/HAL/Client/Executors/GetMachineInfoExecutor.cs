using Redbox.HAL.Component.Model;
using System;
using System.Collections.Generic;

namespace Redbox.HAL.Client.Executors
{
  public sealed class GetMachineInfoExecutor : JobExecutor
  {
    public MachineInformation Info { get; private set; }

    public GetMachineInfoExecutor(HardwareService s)
      : base(s)
    {
      this.Info = new MachineInformation();
    }

    protected override string JobName => "get-machine-info";

    protected override void DisposeInner() => this.Info.Dispose();

    protected override void OnJobCompleted()
    {
      List<GetMachineInfoExecutor.DeckConfiguration> list = new List<GetMachineInfoExecutor.DeckConfiguration>();
      try
      {
        bool flag = false;
        foreach (ProgramResult result in this.Results)
        {
          switch (result.Code)
          {
            case "QlmInfo":
              flag = true;
              this.LocateOrCreate(result.Deck, list).IsQlm = true;
              continue;
            case "DeckInfo":
              this.LocateOrCreate(result.Deck, list).SlotCount = result.Slot;
              continue;
            default:
              continue;
          }
        }
        list.Sort((Comparison<GetMachineInfoExecutor.DeckConfiguration>) ((x, y) => x.Number.CompareTo(y.Number)));
        this.Info.Configuration = flag ? (list[0].SlotCount == 90 ? KioskConfiguration.R630 : KioskConfiguration.R504) : KioskConfiguration.R717;
        list.ForEach((Action<GetMachineInfoExecutor.DeckConfiguration>) (each => this.Info.DecksConfiguration.Add((IDeckConfig) each)));
      }
      finally
      {
        list.Clear();
      }
    }

    private GetMachineInfoExecutor.DeckConfiguration LocateOrCreate(
      int deck,
      List<GetMachineInfoExecutor.DeckConfiguration> list)
    {
      GetMachineInfoExecutor.DeckConfiguration deckConfiguration = list.Find((Predicate<GetMachineInfoExecutor.DeckConfiguration>) (each => each.Number == deck));
      if (deckConfiguration == null)
      {
        deckConfiguration = new GetMachineInfoExecutor.DeckConfiguration()
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
}
