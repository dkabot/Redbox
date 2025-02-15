using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Redbox.HAL.Client;
using Redbox.HAL.Core;

namespace HALUtilities
{
    internal class InventoryChecker
    {
        private InventoryChecker()
        {
        }

        internal static InventoryChecker Instance => Singleton<InventoryChecker>.Instance;

        internal bool CheckInventory(HardwareService service, string against, TextWriter log)
        {
            if (service == null)
            {
                log.WriteLine("The service is null.");
                return false;
            }

            if (!File.Exists(against))
            {
                log.WriteLine("The comparison file '{0}' doesn't exist.", against);
                return false;
            }

            var inventoryState = service.GetInventoryState();
            if (!inventoryState.Success)
            {
                log.WriteLine("Unable to retrieve the inventory state.");
                return false;
            }

            var commandMessage = inventoryState.CommandMessages[0];
            var document1 = new XmlDocument();
            document1.LoadXml(commandMessage);
            var locations1 = GetLocations(document1);
            var document2 = new XmlDocument();
            document2.Load(against);
            var locations2 = GetLocations(document2);
            if (locations2.Count != locations1.Count)
            {
                log.WriteLine("The counts don't match.");
                return false;
            }

            var num = 0;
            for (var index = 0; index < locations2.Count; ++index)
            {
                var inventoryLocation = locations2[index];
                var loc = locations1[index];
                if (!inventoryLocation.IsEquivalent(loc))
                {
                    log.WriteLine("{0} (service) didn't compare to {1} (inv-file)", loc, inventoryLocation);
                    ++num;
                }
            }

            log.WriteLine("There were {0} comparison errors.", num);
            return num == 0;
        }

        private List<InventoryLocation> GetLocations(XmlDocument document)
        {
            var locations = new List<InventoryLocation>();
            foreach (XmlNode selectNode in document.SelectNodes("inventory-items/item"))
            {
                var int32_1 = Convert.ToInt32(selectNode.Attributes["deck"].Value);
                var int32_2 = Convert.ToInt32(selectNode.Attributes["slot"].Value);
                var str = selectNode.Attributes["id"].Value;
                locations.Add(new InventoryLocation
                {
                    Deck = int32_1,
                    Slot = int32_2,
                    ID = str
                });
            }

            return locations;
        }

        private class InventoryLocation
        {
            internal int Deck { get; set; }

            internal int Slot { get; set; }

            internal string ID { get; set; }

            internal string ReturnTime { get; set; }

            internal int StuckCount { get; set; }

            internal string Flags { get; set; }

            internal bool IsEquivalent(InventoryLocation loc)
            {
                return Deck == loc.Deck && Slot == loc.Slot && !(ID != loc.ID) && !(ReturnTime != loc.ReturnTime) &&
                       StuckCount == loc.StuckCount && Flags == loc.Flags;
            }

            public override string ToString()
            {
                return string.Format("Deck = {0}, Slot = {1}, ID = {2} ReturnTime = {3} Count = {4} Flags = {5}", Deck,
                    Slot, ID, ReturnTime, StuckCount, Flags);
            }
        }
    }
}