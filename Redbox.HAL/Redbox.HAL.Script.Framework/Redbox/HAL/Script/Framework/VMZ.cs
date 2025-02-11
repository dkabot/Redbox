using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    internal sealed class VMZ
    {
        private readonly Range m_slots = new Range(1, 15);

        private VMZ()
        {
        }

        internal static VMZ Instance => Singleton<VMZ>.Instance;

        public IRange<int> Bounds => new Range(m_slots.Start, m_slots.End);

        internal ILocation First => ServiceLocator.Instance.GetService<IInventoryService>()
            .Get(ServiceLocator.Instance.GetService<IDecksService>().Last.Number, m_slots.Start);

        internal ILocation Last => ServiceLocator.Instance.GetService<IInventoryService>()
            .Get(ServiceLocator.Instance.GetService<IDecksService>().First.Number, m_slots.End);

        internal int ReserveUnload(int lowDeck)
        {
            var reserved = 0;
            var inventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
            using (var executionTimer = new ExecutionTimer())
            {
                ServiceLocator.Instance.GetService<IDecksService>().ForAllDecksDo(deck =>
                {
                    if (deck.Number <= lowDeck)
                        reserved += inventoryService.SwapEmptyWith(deck, "UNKNOWN", MerchFlags.Unload, m_slots).Count;
                    return true;
                });
                executionTimer.Stop();
                LogHelper.Instance.Log("Reserved unloads through deck {0} ( {1}ms )", lowDeck,
                    executionTimer.ElapsedMilliseconds);
            }

            return reserved;
        }

        internal IteratorComareResult RelativeTo(ILocation location, ILocation reference)
        {
            if (location.Deck < reference.Deck)
                return IteratorComareResult.Above;
            if (location.Deck > reference.Deck)
                return IteratorComareResult.Below;
            if (reference.Slot == location.Slot)
                return IteratorComareResult.Equal;
            return reference.Slot <= location.Slot ? IteratorComareResult.Above : IteratorComareResult.Below;
        }

        internal void ForAllBetweenDo(Predicate<ILocation> callback, ILocation end)
        {
            if (!InVMZ(end))
                return;
            Execute(callback, First, end);
        }

        internal void ForAllBetweenDo(Predicate<ILocation> callback, ILocation low, ILocation high)
        {
            if (!InVMZ(low) || !InVMZ(high))
                return;
            Execute(callback, low, high);
        }

        internal ILocation FindFirstLocation(MerchFlags flags)
        {
            var service = ServiceLocator.Instance.GetService<IDecksService>();
            var inventory = ServiceLocator.Instance.GetService<IInventoryService>();
            var found = (ILocation)null;
            var predicate = (Predicate<IDeck>)(deck =>
            {
                for (var start = m_slots.Start; start <= m_slots.End; ++start)
                {
                    var location = inventory.Get(deck.Number, start);
                    if (location.Flags == flags)
                    {
                        found = location;
                        return false;
                    }
                }

                return true;
            });
            service.ForAllReverseDecksDo(predicate);
            return found;
        }

        internal ILocation FindHighestLocation(MerchFlags flags)
        {
            var service = ServiceLocator.Instance.GetService<IDecksService>();
            var inventory = ServiceLocator.Instance.GetService<IInventoryService>();
            var found = (ILocation)null;
            var predicate = (Predicate<IDeck>)(deck =>
            {
                for (var end = m_slots.End; end >= m_slots.Start; --end)
                {
                    var location = inventory.Get(deck.Number, end);
                    if (location.Flags == flags)
                    {
                        found = location;
                        return false;
                    }
                }

                return true;
            });
            service.ForAllDecksDo(predicate);
            return found;
        }

        internal int ResetCompressedZone(ILocation high)
        {
            var num = 0;
            var service = ServiceLocator.Instance.GetService<IInventoryService>();
            var updates = new List<ILocation>();
            using (new DisposeableList<ILocation>(updates))
            {
                using (var executionTimer = new ExecutionTimer())
                {
                    Execute(loc =>
                    {
                        if (loc.ID != "EMPTY")
                        {
                            loc.ID = "EMPTY";
                            loc.Flags = MerchFlags.None;
                            updates.Add(loc);
                        }

                        return true;
                    }, First, high);
                    num = updates.Count;
                    service.Save(updates);
                    executionTimer.Stop();
                    LogHelper.Instance.Log("VMZ.ClearZone: reset {0} locations ( time = {1}ms ).", num,
                        executionTimer.ElapsedMilliseconds);
                }
            }

            return num;
        }

        internal ILocation ComputeNextHigherLocation(ILocation location)
        {
            var slot = location.Slot + 1;
            var deck = location.Deck;
            if (slot > m_slots.End)
            {
                slot = m_slots.Start;
                --deck;
                var service = ServiceLocator.Instance.GetService<IDecksService>();
                if (deck < service.First.Number)
                    return null;
            }

            return ServiceLocator.Instance.GetService<IInventoryService>().Get(deck, slot);
        }

        internal void DumpZone(IFormattedLog log)
        {
            if (!ControllerConfiguration.Instance.DumpVMZState)
                return;
            log.WriteFormatted("-- Dump VMZ Locations --");
            Execute(loc =>
            {
                log.WriteFormatted("{0} Inventory {1} Flags {2} Stuck {3}", loc.ToString(), loc.ID,
                    loc.Flags.ToString(), loc.StuckCount);
                return true;
            }, First, Last);
        }

        private void Execute(Predicate<ILocation> match, ILocation start, ILocation end)
        {
            ServiceLocator.Instance.GetService<IDecksService>();
            ServiceLocator.Instance.GetService<IInventoryService>();
            var location = start;
            var flag = true;
            while (flag)
            {
                if (location == end)
                    flag = false;
                if (!match(location))
                    break;
                location = ComputeNextHigherLocation(location);
                if (location == null)
                    flag = false;
            }
        }

        private bool InVMZ(ILocation location)
        {
            return location != null && location.Slot >= m_slots.Start && location.Slot <= m_slots.End;
        }
    }
}