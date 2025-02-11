using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Component.Model.Threading;
using Redbox.HAL.Component.Model.Timers;
using Redbox.HAL.Configuration;

namespace Redbox.HAL.Controller.Framework.Services
{
    public sealed class InventoryService : IConfigurationObserver, IInventoryService
    {
        private readonly string EmptyStuckLogFile;
        private readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
        private readonly List<IInventoryObserver> Observers = new List<IInventoryObserver>();
        private readonly IDataTable<ILocation> Table;
        private ILocation[][] LocationInfo;
        private ErrorCodes m_storeState;

        internal InventoryService()
        {
            Table = ServiceLocator.Instance.GetService<IDataTableService>().GetTable<ILocation>();
            EmptyStuckLogFile =
                Path.Combine(
                    ServiceLocator.Instance.GetService<IFormattedLogFactoryService>().CreateSubpath("ErrorLogs"),
                    "ErrorLocationsHistory.txt");
            ControllerConfiguration.Instance.AddObserver(this);
        }

        public void NotifyConfigurationLoaded()
        {
            LogHelper.Instance.Log("[InventoryService] Notify configuration load");
            var service = ServiceLocator.Instance.GetService<IEmptySearchPatternService>();
            if (ControllerConfiguration.Instance.IsVMZMachine)
                service.AddObserver(new VmzExcludePolicy());
            else
                service.AddObserver(new QlmExcludePolicy());
            using (new WithWriteLock(Lock))
            {
                InitializeUnderLock(new ErrorList());
            }

            if (m_storeState != ErrorCodes.Success)
                return;
            Observers.ForEach(each => each.OnInventoryInitialize());
        }

        public void NotifyConfigurationChangeStart()
        {
        }

        public void NotifyConfigurationChangeEnd()
        {
            LogHelper.Instance.Log("[InventoryService] Configuration change end.");
            var num = 0;
            using (new WithWriteLock(Lock))
            {
                var service = ServiceLocator.Instance.GetService<IDecksService>();
                var updates = new List<ILocation>();
                using (new DisposeableList<ILocation>(updates))
                {
                    service.ForAllDecksDo(deck =>
                    {
                        if (LocationInfo[deck.Number - 1].Length == deck.NumberOfSlots)
                            deck.Quadrants.ForEach(q =>
                            {
                                for (var start = q.Slots.Start; start <= q.Slots.End; ++start)
                                {
                                    var underLock = GetUnderLock(deck.Number, start);
                                    if (underLock.Excluded != q.IsExcluded)
                                    {
                                        ResetLocation(underLock);
                                        underLock.Excluded = q.IsExcluded;
                                        updates.Add(underLock);
                                    }
                                }
                            });
                        return true;
                    });
                    if (updates.Count > 0)
                    {
                        num = updates.Count;
                        Table.Update(updates);
                    }
                }
            }

            if (num > 0)
                Observers.ForEach(each => each.OnInventoryChange());
            LogHelper.Instance.Log("[InventoryService] The service {0} configured to track problem locations.",
                IsTrackingEmptyStuck ? "is" : (object)"is not");
        }

        public void AddObserver(IInventoryObserver o)
        {
            Observers.Add(o);
        }

        public bool MachineIsFull()
        {
            var machineEmptyCount = GetMachineEmptyCount();
            var flag = machineEmptyCount <= ControllerConfiguration.Instance.ReturnSlotBuffer;
            if (flag)
                LogHelper.Instance.WithContext(LogEntryType.Info, "The machine is full; there is {0} empty slot(s).",
                    machineEmptyCount);
            return flag;
        }

        public bool EmptyCountExceeds(int numberOfEmpty)
        {
            return GetMachineEmptyCount() > numberOfEmpty;
        }

        public ILocation Lookup(string id)
        {
            using (new WithReadLock(Lock))
            {
                var location = LookupUnderLock(id);
                if (location == null)
                {
                    var msg = string.Format("LOOKUP of item {0} returned NOT FOUND.", id);
                    LogHelper.Instance.WithContext(LogEntryType.Error, msg);
                    ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext().ContextLog
                        .WriteFormatted(msg);
                }

                return location;
            }
        }

        public ILocation Get(int deck, int slot)
        {
            using (new WithReadLock(Lock))
            {
                return GetUnderLock(deck, slot);
            }
        }

        public bool Reset(ILocation loc)
        {
            using (new WithWriteLock(Lock))
            {
                ResetLocation(loc);
                return Table.Update(loc);
            }
        }

        public bool Reset(IList<ILocation> locs)
        {
            using (new WithWriteLock(Lock))
            {
                foreach (var loc in locs)
                    ResetLocation(loc);
                return Table.Update(locs);
            }
        }

        public bool Save(ILocation location)
        {
            using (new WithWriteLock(Lock))
            {
                return Table.Update(location);
            }
        }

        public bool Save(IList<ILocation> locations)
        {
            using (new WithWriteLock(Lock))
            {
                return Table.Update(locations);
            }
        }

        public int GetExcludedSlotsCount()
        {
            return ComputeCount(loc => loc.Excluded);
        }

        public List<ILocation> GetExcludedSlots()
        {
            var list = ComputeList(loc => loc.Excluded);
            list.Sort((x, y) => x.Deck == y.Deck ? x.Slot.CompareTo(y.Slot) : x.Deck.CompareTo(y.Deck));
            return list;
        }

        public List<ILocation> GetEmptySlots()
        {
            return ComputeList(loc => !loc.Excluded && loc.ID == "EMPTY");
        }

        public List<ILocation> GetUnknowns()
        {
            return ComputeList(loc => !loc.Excluded && loc.ID == "UNKNOWN");
        }

        public bool IsBarcodeDuplicate(string id, out ILocation original)
        {
            new DuplicateSearchResult().Original = null;
            original = null;
            if (InventoryConstants.IsKnownInventoryToken(id))
                return false;
            using (new WithReadLock(Lock))
            {
                original = LookupUnderLock(id);
                return original != null;
            }
        }

        public int GetMachineEmptyCount()
        {
            return ComputeCount(loc => !loc.Excluded && loc.ID == "EMPTY");
        }

        public bool UpdateEmptyStuck(ILocation location)
        {
            if (!IsTrackingEmptyStuck)
            {
                LogHelper.Instance.Log("[UpdateEmptyStuck] Service is not tracking.");
                return false;
            }

            using (new WithWriteLock(Lock))
            {
                ++location.StuckCount;
                LogFormattedMessage("Incrementing Deck = {0} Slot = {1}; current stuck count = {2}", location.Deck,
                    location.Slot, location.StuckCount);
                if (location.StuckCount >= ControllerConfiguration.Instance.MarkLocationUnknownThreshold &&
                    location.ID != "UNKNOWN")
                {
                    LogFormattedMessage(" ** Error threshold met: set inventory to {0} at Deck = {1} Slot = {2}",
                        "UNKNOWN", location.Deck, location.Slot);
                    location.ID = "UNKNOWN";
                }

                return Table.Update(location);
            }
        }

        public bool IsStuck(ILocation location)
        {
            return IsTrackingEmptyStuck && location.StuckCount > 0;
        }

        public void DumpStore(TextWriter writer)
        {
            using (new WithReadLock(Lock))
            {
                var list = Table.LoadEntries();
                writer.WriteLine("-- {0} Dump inventory store ( {1} total items )--", DateTime.Now.ToLongDateString(),
                    list.Count);
                using (new DisposeableList<ILocation>(list))
                {
                    foreach (var location in list)
                    {
                        var textWriter = writer;
                        var objArray = new object[6]
                        {
                            location.ToString(),
                            location.ID,
                            null,
                            null,
                            null,
                            null
                        };
                        var returnDate = location.ReturnDate;
                        string str;
                        if (!returnDate.HasValue)
                        {
                            str = "NONE";
                        }
                        else
                        {
                            returnDate = location.ReturnDate;
                            str = returnDate.ToString();
                        }

                        objArray[2] = str;
                        objArray[3] = location.Excluded;
                        objArray[4] = location.StuckCount;
                        objArray[5] = location.Flags;
                        textWriter.WriteLine(
                            "{0} ID = {1} ReturnTime = {2} Excluded = {3} StuckCount = {4} MerchFlags = {5}", objArray);
                    }
                }
            }
        }

        public bool MarkDeckInventory(IDeck deck, string newMatrix)
        {
            var locationList = new List<ILocation>();
            using (new WithWriteLock(Lock))
            {
                using (new DisposeableList<ILocation>(locationList))
                {
                    for (var slot = 1; slot <= deck.NumberOfSlots; ++slot)
                    {
                        var underLock = GetUnderLock(deck.Number, slot);
                        underLock.ID = newMatrix;
                        locationList.Add(underLock);
                    }

                    return Table.Update(locationList);
                }
            }
        }

        public List<int> SwapEmptyWith(IDeck deck, string id, MerchFlags flags, IRange<int> range)
        {
            var intList = new List<int>();
            var locationList = new List<ILocation>();
            using (new WithWriteLock(Lock))
            {
                using (new DisposeableList<ILocation>(locationList))
                {
                    for (var start = range.Start; start <= range.End; ++start)
                    {
                        var underLock = GetUnderLock(deck.Number, start);
                        if (underLock.ID == "EMPTY" && !underLock.Excluded)
                        {
                            underLock.ID = id;
                            underLock.Flags = flags;
                            locationList.Add(underLock);
                            intList.Add(start);
                        }
                    }

                    Table.Update(locationList);
                }
            }

            return intList;
        }

        public bool ResetAndMark(IDeck deck, string id)
        {
            var locationList = new List<ILocation>();
            using (new DisposeableList<ILocation>(locationList))
            {
                using (new WithWriteLock(Lock))
                {
                    for (var slot = 1; slot <= deck.NumberOfSlots; ++slot)
                    {
                        var underLock = GetUnderLock(deck.Number, slot);
                        ResetLocation(underLock);
                        underLock.ID = id;
                        locationList.Add(underLock);
                    }

                    return Table.Update(locationList);
                }
            }
        }

        public void GetState(XmlTextWriter writer)
        {
            using (new WithReadLock(Lock))
            {
                ForeachUnderLock(loc =>
                {
                    writer.WriteStartElement("item");
                    writer.WriteAttributeString("deck", XmlConvert.ToString(loc.Deck));
                    writer.WriteAttributeString("slot", XmlConvert.ToString(loc.Slot));
                    writer.WriteAttributeString("id", loc.ID);
                    writer.WriteAttributeString("ReturnTime",
                        !loc.ReturnDate.HasValue ? "NONE" : loc.ReturnDate.Value.ToString());
                    writer.WriteAttributeString("excluded", loc.Excluded.ToString());
                    writer.WriteAttributeString("emptyStuckCount", loc.StuckCount.ToString());
                    writer.WriteAttributeString("merchFlags", loc.Flags.ToString());
                    writer.WriteEndElement();
                    return true;
                }, false);
            }
        }

        public void ResetState(XmlDocument xmlDocument, ErrorList errors)
        {
            var inventory = new List<ILocation>();
            using (new WithUpgradeableReadLock(Lock))
            {
                using (new DisposeableList<ILocation>(inventory))
                {
                    try
                    {
                        var xmlNodeList = xmlDocument.DocumentElement.SelectNodes("item");
                        if (xmlNodeList == null)
                        {
                            errors.Add(Error.NewError("I001", "Invalid document.",
                                "The specified document has no nodes."));
                        }
                        else
                        {
                            foreach (XmlNode node in xmlNodeList)
                            {
                                var location = FromXML(node, errors);
                                if (location == null)
                                {
                                    errors.Add(Error.NewError("I002", "Load error.",
                                        "Unable to load the deck from the XML."));
                                    return;
                                }

                                inventory.Add(location);
                            }

                            using (new WithWriteLock(Lock))
                            {
                                ServiceLocator.Instance.GetService<IDecksService>().ForAllDecksDo(deck =>
                                {
                                    if (deck.Number - 1 < 0 || deck.Number - 1 > LocationInfo.GetLength(0))
                                    {
                                        errors.Add(Error.NewError("P004",
                                            string.Format("The deck {0} was not present in the deck configuration.",
                                                deck.Number), "Ensure the inventory is valid."));
                                        return false;
                                    }

                                    var all = inventory.FindAll(each => each.Deck == deck.Number);
                                    if (all.Count != deck.NumberOfSlots)
                                    {
                                        errors.Add(Error.NewError("P004",
                                            string.Format(
                                                "The slot count {0} did not match the deck slot count for deck {1} ({2} slots).",
                                                all.Count, deck.Number, deck.NumberOfSlots),
                                            "Ensure the inventory is valid."));
                                        LogHelper.Instance.Log(
                                            string.Format(
                                                "Deck {0} expected {1} slots to be imported but received {2}.",
                                                deck.Number, deck.NumberOfSlots, all.Count), LogEntryType.Error);
                                        return true;
                                    }

                                    Table.Update(all);
                                    LocationInfo[deck.Number - 1] = Sort(all);
                                    return true;
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add(Error.NewError("P999",
                            "An unhandled exception was raised in InventoryService ResetState.", ex));
                        LogHelper.Instance.Log("RESET INVENTORY Failed to import inventory.", ex, LogEntryType.Error);
                    }
                }
            }
        }

        public void Rebuild(ErrorList errors)
        {
            m_storeState = ErrorCodes.StoreError;
            using (new WithWriteLock(Lock))
            {
                if (!Table.Recreate(errors))
                {
                    LogHelper.Instance.Log("[InventoryService] Failed to recreate inventory table.");
                    errors.DumpToLog();
                    return;
                }

                InitializeUnderLock(errors);
            }

            if (m_storeState != ErrorCodes.Success)
                return;
            Observers.ForEach(each => each.OnInventoryRebuild());
        }

        public void InstallFromLegacy(ErrorList errors, TextWriter writer)
        {
            var platterSlots = new GampHelper().GetPlatterSlots();
            var registrySlotCount = ControllerConfiguration.Instance.RegistrySlotCount;
            var factory = ServiceLocator.Instance.GetService<ITableTypeFactory>();
            if (platterSlots == registrySlotCount)
            {
                var locs = new List<ILocation>();
                ServiceLocator.Instance.GetService<IDecksService>().ForAllDecksDo(deck =>
                {
                    for (var slot = 1; slot <= deck.NumberOfSlots; ++slot)
                        locs.Add(factory.NewLocation(deck.Number, slot));
                    return true;
                });
                if (Table.Insert(locs))
                    return;
                errors.Add(Error.NewError("I055", "Database insert failed.",
                    "Failed to insert entries into inventory table."));
            }
            else
            {
                writer.WriteLine(
                    "The machine is configured for {0} slots; the gamp data says there are {1} slots - mark db UNKNOWN with {2} slots",
                    registrySlotCount, platterSlots, registrySlotCount);
            }
        }

        public ErrorCodes CheckIntegrity()
        {
            return CheckIntegrity(false);
        }

        public ErrorCodes CheckIntegrity(bool testStore)
        {
            if (!ControllerConfiguration.Instance.EnableInventoryDatabaseCheck)
                return ErrorCodes.Success;
            if (!testStore)
                return m_storeState;
            using (new WithReadLock(Lock))
            {
                using (var executionTimer = new ExecutionTimer())
                {
                    var inventory = Table.LoadEntries();
                    using (new DisposeableList<ILocation>(inventory))
                    {
                        var total = 0;
                        ServiceLocator.Instance.GetService<IDecksService>().ForAllDecksDo(deck =>
                        {
                            var all = inventory.FindAll(each => each.Deck == deck.Number);
                            if (all.Count != deck.NumberOfSlots)
                                LogHelper.Instance.WithContext(LogEntryType.Error,
                                    "[InventoryService] Integrity mismatch: Found {0} entries for deck {1} ( # slots = {2} ) ",
                                    all.Count, deck.Number, deck.NumberOfSlots);
                            else
                                total += deck.NumberOfSlots;
                            return true;
                        });
                        executionTimer.Stop();
                        LogHelper.Instance.Log("[CheckStoreIntegrity] Execution time = {0}ms",
                            executionTimer.ElapsedMilliseconds);
                        return total != inventory.Count ? ErrorCodes.StoreError : ErrorCodes.Success;
                    }
                }
            }
        }

        public bool IsTrackingEmptyStuck => ControllerConfiguration.Instance.TrackProblemLocations;

        private void LogFormattedMessage(string format, params object[] parms)
        {
            if (!IsTrackingEmptyStuck)
                return;
            var str1 = string.Format(format, parms);
            try
            {
                using (var streamWriter1 = new StreamWriter(EmptyStuckLogFile, true))
                {
                    var streamWriter2 = streamWriter1;
                    var now = DateTime.Now;
                    var shortDateString = now.ToShortDateString();
                    now = DateTime.Now;
                    var shortTimeString = now.ToShortTimeString();
                    var str2 = str1;
                    streamWriter2.WriteLine("{0} {1} {2}", shortDateString, shortTimeString, str2);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("[InventoryService] Failed to write to empty/stuck log.", ex);
            }
        }

        private List<ILocation> ComputeList(Predicate<ILocation> predicate)
        {
            return ComputeList(predicate, true);
        }

        private List<ILocation> ComputeList(Predicate<ILocation> predicate, bool excludeQLM)
        {
            using (new WithReadLock(Lock))
            {
                var rv = new List<ILocation>();
                ForeachUnderLock(location =>
                {
                    if (predicate(location))
                        rv.Add(location);
                    return true;
                }, excludeQLM);
                return rv;
            }
        }

        private int ComputeCount(Predicate<ILocation> predicate)
        {
            return ComputeCount(predicate, true);
        }

        private int ComputeCount(Predicate<ILocation> predicate, bool excludeQLM)
        {
            using (new WithReadLock(Lock))
            {
                var count = 0;
                ForeachUnderLock(location =>
                {
                    if (predicate(location))
                        ++count;
                    return true;
                }, excludeQLM);
                return count;
            }
        }

        private void ForeachUnderLock(Predicate<ILocation> action, bool excludeQLM)
        {
            var service = ServiceLocator.Instance.GetService<IDecksService>();
            var length1 = LocationInfo.GetLength(0);
            for (var index1 = 0; index1 < length1; ++index1)
            {
                var byNumber = service.GetByNumber(index1 + 1);
                if (!excludeQLM || !byNumber.IsQlm)
                {
                    var length2 = LocationInfo[index1].Length;
                    for (var index2 = 0; index2 < length2; ++index2)
                    {
                        var location = LocationInfo[index1][index2];
                        if (!action(location))
                            return;
                    }
                }
            }
        }

        private ILocation LookupUnderLock(string id)
        {
            var location = (ILocation)null;
            ForeachUnderLock(loc =>
            {
                if (!(loc.ID == id))
                    return true;
                location = loc;
                return false;
            }, true);
            return location;
        }

        private void InitializeUnderLock(ErrorList errors)
        {
            m_storeState = ErrorCodes.Success;
            var service = ServiceLocator.Instance.GetService<IDecksService>();
            try
            {
                var inventory = Table.LoadEntries();
                using (new DisposeableList<ILocation>(inventory))
                {
                    LocationInfo = new ILocation[service.DeckCount][];
                    service.ForAllDecksDo(deck =>
                    {
                        var all = inventory.FindAll(each => each.Deck == deck.Number);
                        if (all.Count == deck.NumberOfSlots)
                        {
                            LogHelper.Instance.Log(
                                "[InventoryService] Loaded {0} entries for deck {1} ( # slots = {2} )", all.Count,
                                deck.Number, deck.NumberOfSlots);
                            LocationInfo[deck.Number - 1] = Sort(all);
                            return true;
                        }

                        if (all.Count > 0)
                        {
                            LogHelper.Instance.Log(LogEntryType.Error,
                                "[InventoryService] LoadInventory: res.count = {0}; deck count = {1}; deleting deck & initializing to UNKNOWN",
                                all.Count, deck.NumberOfSlots);
                            if (!Table.Delete(all))
                            {
                                m_storeState = ErrorCodes.StoreError;
                                LogHelper.Instance.Log("[InventoryService] Delete returned false.");
                                return false;
                            }

                            all.Clear();
                        }

                        var locationArray = InitializeDeck(deck);
                        if (locationArray != null)
                        {
                            LocationInfo[deck.Number - 1] = locationArray;
                            return true;
                        }

                        m_storeState = ErrorCodes.StoreError;
                        return false;
                    });
                }

                LogHelper.Instance.Log("[InventoryService] The service {0} configured to track problem locations.",
                    IsTrackingEmptyStuck ? "is" : (object)"is not");
                LogHelper.Instance.Log("[InventoryService] Store integrity -> {0}",
                    m_storeState == ErrorCodes.Success ? "OK" : (object)"CORRUPT");
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("[InventoryService] Initialize under lock caught an exception.", ex);
                m_storeState = ErrorCodes.StoreError;
            }
        }

        private ILocation[] InitializeDeck(IDeck deck)
        {
            var service = ServiceLocator.Instance.GetService<ITableTypeFactory>();
            var locationList = new List<ILocation>();
            using (new DisposeableList<ILocation>(locationList))
            {
                for (var slot = 1; slot <= deck.NumberOfSlots; ++slot)
                {
                    var location = service.NewLocation(deck.Number, slot);
                    location.ID = "UNKNOWN";
                    locationList.Add(location);
                }

                if (Table.Insert(locationList))
                    return Sort(locationList);
                LogHelper.Instance.Log("[InventoryService] Insert of new locations returned false.");
                return null;
            }
        }

        private ILocation[] Sort(List<ILocation> deckEntries)
        {
            deckEntries.Sort((x, y) => x.Slot.CompareTo(y.Slot));
            return deckEntries.ToArray();
        }

        private ILocation FromXML(XmlNode node, ErrorList errors)
        {
            var service = ServiceLocator.Instance.GetService<IDecksService>();
            var attributeValue1 = node.GetAttributeValue("deck", new int?());
            var attributeValue2 = node.GetAttributeValue("slot", new int?());
            var attributeValue3 = node.GetAttributeValue("id", (string)null);
            var attributeValue4 = node.GetAttributeValue("ReturnTime", (string)null);
            var returnTime = new DateTime?();
            if (!string.IsNullOrEmpty(attributeValue4))
                if (!attributeValue4.Equals("NONE"))
                    try
                    {
                        returnTime = DateTime.Parse(attributeValue4);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log(string.Format("Unable to parse date time {0}", attributeValue4),
                            LogEntryType.Error);
                        returnTime = new DateTime?();
                    }

            var attributeValue5 = node.GetAttributeValue("excluded", false);
            var attributeValue6 = node.GetAttributeValue("emptyStuckCount", 0);
            var ignoringCase =
                Enum<MerchFlags>.ParseIgnoringCase(node.GetAttributeValue("merchFlags", "None"), MerchFlags.None);
            if (!attributeValue1.HasValue || !attributeValue2.HasValue || string.IsNullOrEmpty(attributeValue3))
            {
                errors.Add(Error.NewError("P004", "A valid deck, slot, and id must be specified for each item element.",
                    "Add a valid item element with a deck, slot, and id attributes."));
                LogHelper.Instance.Log(string.Format("Attempt to import invalid item. XmlNode: {0}", node.OuterXml),
                    LogEntryType.Error);
                return null;
            }

            var byNumber = service.GetByNumber(attributeValue1.Value);
            if (byNumber == null)
            {
                errors.Add(Error.NewError("P003",
                    string.Format("The deck value must be between 1 and {0}.", service.DeckCount),
                    "Ensure the deck attribute value is within the valid range."));
                LogHelper.Instance.Log(
                    string.Format("Deck {0} doesn't match any decks in the configuration.", attributeValue1),
                    LogEntryType.Error);
                return null;
            }

            var nullable1 = attributeValue2;
            var num = 1;
            if (!((nullable1.GetValueOrDefault() < num) & nullable1.HasValue))
            {
                var numberOfSlots = byNumber.NumberOfSlots;
                var nullable2 = attributeValue2;
                var valueOrDefault = nullable2.GetValueOrDefault();
                if (!((numberOfSlots < valueOrDefault) & nullable2.HasValue))
                    return ServiceLocator.Instance.GetService<ITableTypeFactory>().NewLocation(attributeValue1.Value,
                        attributeValue2.Value, attributeValue3, returnTime, attributeValue5, attributeValue6,
                        ignoringCase);
            }

            errors.Add(Error.NewError("P003",
                string.Format("The slot must be between 1 and {0}.", byNumber.NumberOfSlots),
                "Ensure the slot number is within the valid range."));
            LogHelper.Instance.Log(
                string.Format("Deck {0} expects a value between 1 and {1}, received {2}", byNumber.Number,
                    byNumber.NumberOfSlots, attributeValue2), LogEntryType.Error);
            return null;
        }

        private ILocation GetUnderLock(int deck, int slot)
        {
            return LocationInfo[deck - 1][slot - 1];
        }

        private void ResetLocation(ILocation loc)
        {
            loc.ReturnDate = new DateTime?();
            loc.ID = "EMPTY";
            loc.StuckCount = 0;
            loc.Flags = MerchFlags.None;
        }
    }
}