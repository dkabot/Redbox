using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class DumpbinService : IDumpbinService
    {
        private readonly int Deck;
        private readonly IRange<int> Slots;
        private readonly IDataTable<IDumpBinInventoryItem> Table;
        private int m_itemCount;

        internal DumpbinService()
        {
            Deck = ServiceLocator.Instance.GetService<IDecksService>().Last.Number;
            Slots = new Range(82, 84);
            Table = ServiceLocator.Instance.GetService<IDataTableService>().GetTable<IDumpBinInventoryItem>();
            m_itemCount = Table.GetRowCount();
            LogHelper.Instance.Log("[DumpbinService] Loaded {0} items.", m_itemCount);
        }

        public bool IsFull()
        {
            return CurrentCount() >= Capacity;
        }

        public bool IsBin(ILocation loc)
        {
            return loc.Deck == Deck && loc.Slot >= Slots.Start && loc.Slot <= Slots.End;
        }

        public int CurrentCount()
        {
            return m_itemCount;
        }

        public int RemainingSpace()
        {
            return !IsFull() ? Capacity - CurrentCount() : 0;
        }

        public bool ClearItems()
        {
            LogHelper.Instance.Log("Reset dumpbin counter: current bin count = {0}", CurrentCount());
            ServiceLocator.Instance.GetService<IRuntimeService>();
            try
            {
                using (var log = new StreamWriter(File.Open(
                           Path.Combine(
                               ServiceLocator.Instance.GetService<IFormattedLogFactoryService>()
                                   .CreateSubpath("Service"), "DumpBinInventory.log"), FileMode.Append,
                           FileAccess.Write, FileShare.Read)))
                {
                    DumpContents(log);
                    log.WriteLine();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Failed to log dump bin contents.", ex);
            }

            var num = CurrentCount();
            m_itemCount = 0;
            return Table.DeleteAll() == num;
        }

        public void DumpContents(TextWriter log)
        {
            var list = Table.LoadEntries();
            using (new DisposeableList<IDumpBinInventoryItem>(list))
            {
                log.WriteLine("-- Dump bin inventory ( {0} total items ) --", list.Count);
                foreach (var binInventoryItem in list)
                    log.WriteLine("Barcode {0} put in at {1}", binInventoryItem.ID,
                        binInventoryItem.PutTime.ToString());
            }
        }

        public IList<IDumpBinInventoryItem> GetBarcodesInBin()
        {
            return Table.LoadEntries();
        }

        public bool AddBinItem(string matrix)
        {
            return AddBinItem(ServiceLocator.Instance.GetService<ITableTypeFactory>().NewBinItem(matrix, DateTime.Now));
        }

        public bool AddBinItem(IDumpBinInventoryItem item)
        {
            ++m_itemCount;
            return Table.Insert(item);
        }

        public void GetState(XmlTextWriter writer)
        {
            var list = Table.LoadEntries();
            using (new DisposeableList<IDumpBinInventoryItem>(list))
            {
                foreach (var binInventoryItem in list)
                {
                    writer.WriteStartElement("bin-item");
                    writer.WriteAttributeString("PutTime", binInventoryItem.PutTime.ToString());
                    writer.WriteAttributeString("id", binInventoryItem.ID);
                    writer.WriteEndElement();
                }
            }
        }

        public void ResetState(XmlDocument xmlDocument, ErrorList errors)
        {
            var service = ServiceLocator.Instance.GetService<ITableTypeFactory>();
            ClearItems();
            var xmlNodeList = xmlDocument.DocumentElement.SelectNodes("bin-item");
            if (xmlNodeList == null || xmlNodeList.Count <= 0)
                return;
            var binInventoryItemList = new List<IDumpBinInventoryItem>();
            using (new DisposeableList<IDumpBinInventoryItem>(binInventoryItemList))
            {
                foreach (XmlNode node in xmlNodeList)
                {
                    var attributeValue1 = node.GetAttributeValue("id", "UNKNOWN");
                    var attributeValue2 = node.GetAttributeValue("PutTime", (string)null);
                    var now = DateTime.Now;
                    if (attributeValue2 != null)
                        try
                        {
                            now = DateTime.Parse(attributeValue2);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Instance.Log(LogEntryType.Error,
                                "[DumpbinService] ResetState: unable to parse date time {0}", attributeValue2);
                            now = DateTime.Now;
                        }

                    binInventoryItemList.Add(service.NewBinItem(attributeValue1, now));
                }

                if (binInventoryItemList.Count <= 0)
                    return;
                Table.Insert(binInventoryItemList);
                m_itemCount += binInventoryItemList.Count;
            }
        }

        public ILocation PutLocation =>
            ServiceLocator.Instance.GetService<IInventoryService>().Get(Deck, Slots.Start + 1);

        public int Capacity => 60;

        public ILocation RotationLocation => ServiceLocator.Instance.GetService<IInventoryService>().Get(Deck, 1);
    }
}