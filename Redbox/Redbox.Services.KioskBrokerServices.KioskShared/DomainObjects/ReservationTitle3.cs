using Redbox.Services.KioskBrokerServices.KioskShared.Enums;
using System;

namespace Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects
{
  [Serializable]
  public class ReservationTitle3
  {
    public int ItemID { get; set; }

    public int TitleID { get; set; }

    public ReservationType ReservationType { get; set; }

    public Decimal Price { get; set; }

    public Decimal Discount { get; set; }

    public DiscountType DiscountType { get; set; }

    public Decimal DiscountedPrice { get; set; }

    public int NumberOfCredits { get; set; }

    public Decimal LoyaltyPoints { get; set; }
  }
}
