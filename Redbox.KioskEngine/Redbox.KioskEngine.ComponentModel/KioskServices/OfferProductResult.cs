using System;

namespace Redbox.KioskEngine.ComponentModel.KioskServices
{
  public class OfferProductResult
  {
    public long ProductId { get; set; }

    public byte? NumberOfNights { get; set; }

    public Decimal? DiscountedPrice { get; set; }

    public Decimal? TotalDiscountAmount { get; set; }
  }
}
