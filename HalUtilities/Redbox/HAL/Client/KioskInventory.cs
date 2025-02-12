using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Redbox.HAL.Client
{
    public sealed class KioskInventory : IDisposable
    {
        private readonly HardwareService Service;
        private bool Disposed;

        public KioskInventory(HardwareService service)
        {
            Service = service;
            Load();
        }

        public List<IInventoryLocation> DeckInventory { get; } = new List<IInventoryLocation>();

        public List<IDumpbinItem> DumpbinItems { get; } = new List<IDumpbinItem>();

        public void Dispose()
        {
            Dispose(true);
        }

        ~KioskInventory()
        {
            Dispose(false);
        }

        private void Dispose(bool fromDispose)
        {
            if (Disposed)
                return;
            Disposed = true;
            if (!fromDispose)
                return;
            DeckInventory.Clear();
            DumpbinItems.Clear();
        }

        private void Load()
        {
            var inventoryState = Service.GetInventoryState();
            if (!inventoryState.Success)
                return;
            var xmlDocument = new XmlDocument();
            using (var txtReader = new StringReader(inventoryState.CommandMessages[0]))
            {
                xmlDocument.Load(txtReader);
                if (xmlDocument.DocumentElement == null)
                    return;
                var elementsByTagName1 = xmlDocument.GetElementsByTagName("item");
                for (var i = 0; i < elementsByTagName1.Count; ++i)
                {
                    var matrix = elementsByTagName1[i].Attributes["id"].Value;
                    var s = elementsByTagName1[i].Attributes["ReturnTime"].Value;
                    var returnTime = new DateTime?();
                    if (s != "NONE")
                        returnTime = DateTime.Parse(s);
                    var str1 = elementsByTagName1[i].Attributes["excluded"].Value;
                    var str2 = elementsByTagName1[i].Attributes["emptyStuckCount"].Value;
                    var merch = elementsByTagName1[i].Attributes["merchFlags"].Value;
                    DeckInventory.Add(new InventoryLocation(
                        Convert.ToInt32(elementsByTagName1[i].Attributes["deck"].Value),
                        Convert.ToInt32(elementsByTagName1[i].Attributes["slot"].Value), matrix, Convert.ToInt32(str2),
                        returnTime, merch, Convert.ToBoolean(str1)));
                }

                DeckInventory.Sort(new InventoryLocationSorter());
                var elementsByTagName2 = xmlDocument.GetElementsByTagName("bin-item");
                for (var i = 0; i < elementsByTagName2.Count; ++i)
                {
                    var putTime = DateTime.Parse(elementsByTagName2[i].Attributes["PutTime"].Value);
                    DumpbinItems.Add(new DumpbinItem(elementsByTagName2[i].Attributes["id"].Value, putTime));
                }
            }
        }

        private class InventoryLocationSorter : IComparer<IInventoryLocation>
        {
            public int Compare(IInventoryLocation x, IInventoryLocation y)
            {
                if (x.Location.Deck > y.Location.Deck)
                    return 1;
                if (x.Location.Deck < y.Location.Deck)
                    return -1;
                if (x.Location.Slot == y.Location.Slot)
                    return 0;
                return x.Location.Slot <= y.Location.Slot ? -1 : 1;
            }
        }
    }
}