using Redbox.HAL.Client;
using Redbox.HAL.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace HALUtilities
{
  internal class InventoryChecker
  {
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
        log.WriteLine("The comparison file '{0}' doesn't exist.", (object) against);
        return false;
      }
      HardwareCommandResult inventoryState = service.GetInventoryState();
      if (!inventoryState.Success)
      {
        log.WriteLine("Unable to retrieve the inventory state.");
        return false;
      }
      string commandMessage = inventoryState.CommandMessages[0];
      XmlDocument document1 = new XmlDocument();
      document1.LoadXml(commandMessage);
      List<InventoryChecker.InventoryLocation> locations1 = this.GetLocations(document1);
      XmlDocument document2 = new XmlDocument();
      document2.Load(against);
      List<InventoryChecker.InventoryLocation> locations2 = this.GetLocations(document2);
      if (locations2.Count != locations1.Count)
      {
        log.WriteLine("The counts don't match.");
        return false;
      }
      int num = 0;
      for (int index = 0; index < locations2.Count; ++index)
      {
        InventoryChecker.InventoryLocation inventoryLocation = locations2[index];
        InventoryChecker.InventoryLocation loc = locations1[index];
        if (!inventoryLocation.IsEquivalent(loc))
        {
          log.WriteLine("{0} (service) didn't compare to {1} (inv-file)", (object) loc, (object) inventoryLocation);
          ++num;
        }
      }
      log.WriteLine("There were {0} comparison errors.", (object) num);
      return num == 0;
    }

    private List<InventoryChecker.InventoryLocation> GetLocations(XmlDocument document)
    {
      List<InventoryChecker.InventoryLocation> locations = new List<InventoryChecker.InventoryLocation>();
      foreach (XmlNode selectNode in document.SelectNodes("inventory-items/item"))
      {
        int int32_1 = Convert.ToInt32(selectNode.Attributes["deck"].Value);
        int int32_2 = Convert.ToInt32(selectNode.Attributes["slot"].Value);
        string str = selectNode.Attributes["id"].Value;
        locations.Add(new InventoryChecker.InventoryLocation()
        {
          Deck = int32_1,
          Slot = int32_2,
          ID = str
        });
      }
      return locations;
    }

    private InventoryChecker()
    {
    }

    private class InventoryLocation
    {
      internal bool IsEquivalent(InventoryChecker.InventoryLocation loc)
      {
        return this.Deck == loc.Deck && this.Slot == loc.Slot && !(this.ID != loc.ID) && !(this.ReturnTime != loc.ReturnTime) && this.StuckCount == loc.StuckCount && this.Flags == loc.Flags;
      }

      internal int Deck { get; set; }

      internal int Slot { get; set; }

      internal string ID { get; set; }

      internal string ReturnTime { get; set; }

      internal int StuckCount { get; set; }

      internal string Flags { get; set; }

      public override string ToString()
      {
        return string.Format("Deck = {0}, Slot = {1}, ID = {2} ReturnTime = {3} Count = {4} Flags = {5}", (object) this.Deck, (object) this.Slot, (object) this.ID, (object) this.ReturnTime, (object) this.StuckCount, (object) this.Flags);
      }
    }
  }
}
