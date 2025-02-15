using Redbox.Services.KioskBrokerServices.KioskShared.Enums;
using System;

namespace Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects
{
  [Serializable]
  public class DiscStatus
  {
    public string Barcode { get; set; }

    public int DiscTypeID { get; set; }

    public DiscType DiscType
    {
      get
      {
        return Enum.IsDefined(typeof (DiscType), (object) this.DiscTypeID) ? (DiscType) Enum.ToObject(typeof (DiscType), this.DiscTypeID) : DiscType._Unspecified;
      }
    }
  }
}
