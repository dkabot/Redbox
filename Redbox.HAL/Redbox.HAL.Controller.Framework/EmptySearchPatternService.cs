using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class EmptySearchPatternService : IInventoryObserver, IEmptySearchPatternService
    {
        private readonly IInventoryService InventoryService;
        private readonly object Lock = new object();

        private readonly List<IExcludeEmptySearchLocationObserver> Observers =
            new List<IExcludeEmptySearchLocationObserver>();

        private readonly List<ESPNode> Pattern = new List<ESPNode>();

        internal EmptySearchPatternService(IInventoryService iis)
        {
            InventoryService = iis;
            InventoryService.AddObserver(this);
        }

        public void DumpESP(bool dumpStore)
        {
            var path = Path.Combine(
                ServiceLocator.Instance.GetService<IFormattedLogFactoryService>().CreateSubpath("Service"), "ESP.log");
            using (var log = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read)))
            {
                var str = string.Format("{0} {1}: *** Dump of ESP list ***", DateTime.Now.ToShortDateString(),
                    DateTime.Now.ToShortTimeString());
                log.WriteLine(str);
                Pattern.ForEach(node =>
                {
                    var location = InventoryService.Get(node.Deck, node.Slot);
                    log.WriteLine("{0} Excluded = {1}", location.ToString(), location.Excluded.ToString());
                });
                log.WriteLine();
            }
        }

        public void AddObserver(IExcludeEmptySearchLocationObserver veto)
        {
            lock (Lock)
            {
                Observers.Add(veto);
            }
        }

        public void RemoveObserver(IExcludeEmptySearchLocationObserver v)
        {
            lock (Lock)
            {
                Observers.Remove(v);
            }
        }

        public IEmptySearchResult FindEmptyLocations()
        {
            var emptyLocations = new EmptySearchResult();
            var num = SlotsAvailable();
            if (num == 0)
                return emptyLocations;
            foreach (var espNode in Pattern)
            {
                var location = InventoryService.Get(espNode.Deck, espNode.Slot);
                if (location.IsEmpty)
                {
                    emptyLocations.EmptyLocations.Add(location);
                    if (emptyLocations.EmptyLocations.Count == num)
                        break;
                }
            }

            return emptyLocations;
        }

        public IEmptySearchResult FindEmptyOutsideOf(ILocation top)
        {
            var emptyOutsideOf = new EmptySearchResult();
            var num = SlotsAvailable();
            if (num == 0)
                return emptyOutsideOf;
            foreach (var espNode in Pattern)
            {
                var location = InventoryService.Get(espNode.Deck, espNode.Slot);
                if (!top.Equals(location))
                {
                    if (location.IsEmpty)
                        emptyOutsideOf.EmptyLocations.Add(location);
                    if (emptyOutsideOf.EmptyLocations.Count == num)
                        break;
                }
                else
                {
                    break;
                }
            }

            return emptyOutsideOf;
        }

        public void OnInventoryInitialize()
        {
            LogHelper.Instance.Log("[EmptySearchService] Initialize.");
            Rebuild();
        }

        public void OnInventoryChange()
        {
            LogHelper.Instance.Log("[EmptySearchService] OnInventoryRebuild.");
            Rebuild();
        }

        public void OnInventoryRebuild()
        {
            LogHelper.Instance.Log("[EmptySearchService] OnInventoryRebuild.");
            Rebuild();
        }

        private void Rebuild()
        {
            lock (Lock)
            {
                Pattern.Clear();
                ComputeList();
                LogHelper.Instance.Log(" ESP statistics:");
                LogHelper.Instance.Log("  Total entries: {0}", Pattern.Count);
                LogHelper.Instance.Log("  There are {0} total excluded slots.",
                    InventoryService.GetExcludedSlotsCount());
            }
        }

        private int SlotsAvailable()
        {
            if (InventoryService.CheckIntegrity() != ErrorCodes.Success)
                return 0;
            var returnSlotBuffer = ControllerConfiguration.Instance.ReturnSlotBuffer;
            var num = InventoryService.GetMachineEmptyCount() - returnSlotBuffer;
            if (num <= 0)
            {
                LogHelper.Instance.Log("[EmptySearchService] There are no empty slots available.");
                return 0;
            }

            if (num > ControllerConfiguration.Instance.PutAwayItemAttempts)
                num = ControllerConfiguration.Instance.PutAwayItemAttempts;
            return num;
        }

        private void AddLocation(IDeck deck, int slot, ref int idx)
        {
            var location = InventoryService.Get(deck.Number, slot);
            if (location.Excluded)
            {
                LogHelper.Instance.Log(LogEntryType.Debug, "Location {0} is already excluded.", location);
            }
            else
            {
                foreach (var observer in Observers)
                    if (observer.ShouldExclude(location))
                    {
                        LogHelper.Instance.Log("[EmptySearchService] Apply exclude policy to {0}", location);
                        location.Excluded = true;
                        InventoryService.Save(location);
                        return;
                    }

                Pattern.Add(new ESPNode(deck.Number, slot, idx));
            }
        }

        private void ComputeList()
        {
            var service = ServiceLocator.Instance.GetService<IDecksService>();
            var numArray = new int[5] { 3, 2, 4, 1, 5 };
            var idx = 0;
            if (!ControllerConfiguration.Instance.IsVMZMachine)
            {
                for (var index = 0; index < 6; ++index)
                    foreach (var number in numArray)
                    {
                        var byNumber = service.GetByNumber(number);
                        var num1 = byNumber.IsSparse ? 12 : 15;
                        var num2 = index * num1 + 1;
                        for (var slot = num2; slot <= num2 + num1 - 1; ++slot)
                            AddLocation(byNumber, slot, ref idx);
                    }

                var byNumber1 = service.GetByNumber(6);
                for (var slot = 1; slot <= byNumber1.NumberOfSlots; ++slot)
                    AddLocation(byNumber1, slot, ref idx);
                var byNumber2 = service.GetByNumber(7);
                for (var slot = 1; slot <= byNumber2.NumberOfSlots; ++slot)
                    AddLocation(byNumber2, slot, ref idx);
                var byNumber3 = service.GetByNumber(8);
                if (byNumber3.IsQlm)
                    return;
                for (var slot = 1; slot <= byNumber3.NumberOfSlots; ++slot)
                    AddLocation(byNumber3, slot, ref idx);
            }
            else
            {
                for (var index = 1; index < 6; ++index)
                    foreach (var number in numArray)
                    {
                        var byNumber = service.GetByNumber(number);
                        var num = index * byNumber.SlotsPerQuadrant + 1;
                        for (var slot = num; slot <= num + byNumber.SlotsPerQuadrant - 1; ++slot)
                            AddLocation(byNumber, slot, ref idx);
                    }

                var byNumber4 = service.GetByNumber(6);
                for (var slot = 16; slot <= byNumber4.NumberOfSlots; ++slot)
                    AddLocation(byNumber4, slot, ref idx);
                var byNumber5 = service.GetByNumber(8);
                for (var slot = 16; slot <= byNumber5.NumberOfSlots; ++slot)
                    AddLocation(byNumber5, slot, ref idx);
                var byNumber6 = service.GetByNumber(7);
                for (var slot = 16; slot <= byNumber6.NumberOfSlots; ++slot)
                    AddLocation(byNumber6, slot, ref idx);
                for (var number = 1; number <= 8; ++number)
                {
                    var byNumber7 = service.GetByNumber(number);
                    for (var slot = 15; slot >= 1; --slot)
                        AddLocation(byNumber7, slot, ref idx);
                }
            }
        }

        private class ESPNode
        {
            internal readonly int Deck;
            internal readonly int Index;
            internal readonly int Slot;

            internal ESPNode(int deck, int slot, int idx)
            {
                Deck = deck;
                Slot = slot;
                Index = idx;
            }
        }
    }
}