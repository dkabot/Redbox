using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Redbox.HAL.Client
{
  public sealed class KioskInventory : IDisposable
  {
    private bool Disposed;
    private readonly HardwareService Service;
    private readonly List<IDumpbinItem> DumpbinInfo = new List<IDumpbinItem>();
    private readonly List<IInventoryLocation> DeckInfo = new List<IInventoryLocation>();

    public void Dispose() => this.Dispose(true);

    ~KioskInventory() => this.Dispose(false);

    public List<IInventoryLocation> DeckInventory => this.DeckInfo;

    public List<IDumpbinItem> DumpbinItems => this.DumpbinInfo;

    public KioskInventory(HardwareService service)
    {
      this.Service = service;
      this.Load();
    }

    private void Dispose(bool fromDispose)
    {
      if (this.Disposed)
        return;
      this.Disposed = true;
      if (!fromDispose)
        return;
      this.DeckInfo.Clear();
      this.DumpbinInfo.Clear();
    }

    private void Load()
    {
      HardwareCommandResult inventoryState = this.Service.GetInventoryState();
      if (!inventoryState.Success)
        return;
      XmlDocument xmlDocument = new XmlDocument();
      using (StringReader txtReader = new StringReader(inventoryState.CommandMessages[0]))
      {
        xmlDocument.Load((TextReader) txtReader);
        if (xmlDocument.DocumentElement == null)
          return;
        XmlNodeList elementsByTagName1 = xmlDocument.GetElementsByTagName("item");
        for (int i = 0; i < elementsByTagName1.Count; ++i)
        {
          string matrix = elementsByTagName1[i].Attributes["id"].Value;
          string s = elementsByTagName1[i].Attributes["ReturnTime"].Value;
          DateTime? returnTime = new DateTime?();
          if (s != "NONE")
            returnTime = new DateTime?(DateTime.Parse(s));
          string str1 = elementsByTagName1[i].Attributes["excluded"].Value;
          string str2 = elementsByTagName1[i].Attributes["emptyStuckCount"].Value;
          string merch = elementsByTagName1[i].Attributes["merchFlags"].Value;
          this.DeckInfo.Add((IInventoryLocation) new InventoryLocation(Convert.ToInt32(elementsByTagName1[i].Attributes["deck"].Value), Convert.ToInt32(elementsByTagName1[i].Attributes["slot"].Value), matrix, Convert.ToInt32(str2), returnTime, merch, Convert.ToBoolean(str1)));
        }
        this.DeckInfo.Sort((IComparer<IInventoryLocation>) new KioskInventory.InventoryLocationSorter());
        XmlNodeList elementsByTagName2 = xmlDocument.GetElementsByTagName("bin-item");
        for (int i = 0; i < elementsByTagName2.Count; ++i)
        {
          DateTime putTime = DateTime.Parse(elementsByTagName2[i].Attributes["PutTime"].Value);
          this.DumpbinInfo.Add((IDumpbinItem) new DumpbinItem(elementsByTagName2[i].Attributes["id"].Value, putTime));
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
