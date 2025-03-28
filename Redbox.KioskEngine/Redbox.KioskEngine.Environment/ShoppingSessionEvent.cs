using Redbox.KioskEngine.ComponentModel;
using System;

namespace Redbox.KioskEngine.Environment
{
  internal class ShoppingSessionEvent : IShoppingSessionEvent
  {
    public DateTime TimeStamp { get; internal set; }

    public string Description { get; internal set; }

    public ShoppingSessionEventType Type { get; internal set; }
  }
}
