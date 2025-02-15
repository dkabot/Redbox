using Redbox.Services.KioskBrokerServices.KioskShared.Enums;
using System;

namespace Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects
{
  [Serializable]
  public class ReservationTitle2
  {
    public int ItemID { get; set; }

    public int TitleID { get; set; }

    public int ReservationTypeID { get; set; }

    public ReservationType ReservationType
    {
      get
      {
        return Enum.IsDefined(typeof (ReservationType), (object) this.ReservationTypeID) ? (ReservationType) Enum.ToObject(typeof (ReservationType), this.ReservationTypeID) : ReservationType._Unspecified;
      }
    }

    public int DiscFormatID { get; set; }

    public DiscFormat DiscFormat
    {
      get
      {
        return Enum.IsDefined(typeof (DiscFormat), (object) this.DiscFormatID) ? (DiscFormat) Enum.ToObject(typeof (DiscFormat), this.DiscFormatID) : DiscFormat._Unspecified;
      }
    }

    public Decimal Price { get; set; }

    public Decimal Discount { get; set; }

    public Decimal DiscountedPrice { get; set; }

    public int NumberOfCredits { get; set; }

    public long? CreditID { get; set; }
  }
}
