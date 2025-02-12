using Redbox.HAL.Client;
using System;
using System.Collections.Generic;

namespace HALUtilities
{
  internal sealed class RandomInventorySelector : IDisposable
  {
    private readonly List<IInventoryLocation> m_locs = new List<IInventoryLocation>();

    public void Dispose() => this.m_locs.Clear();

    internal IInventoryLocation Select()
    {
      if (0 >= this.m_locs.Count)
        return (IInventoryLocation) null;
      int index = new Random(DateTime.Now.Millisecond).Next(0, this.m_locs.Count - 1);
      IInventoryLocation loc = this.m_locs[index];
      this.m_locs.RemoveAt(index);
      return loc;
    }

    internal RandomInventorySelector(HardwareService service)
    {
      using (KioskInventory kioskInventory = new KioskInventory(service))
      {
        if (kioskInventory.DeckInventory.Count == 0)
          throw new Exception("Cannot load inventory.");
        kioskInventory.DeckInventory.ForEach((Action<IInventoryLocation>) (element =>
        {
          if (!(element.Matrix != "EMPTY"))
            return;
          this.m_locs.Add(element);
        }));
      }
    }
  }
}
