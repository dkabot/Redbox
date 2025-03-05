using System;

namespace Redbox.KioskEngine.ComponentModel.ApiService.EngineCore
{
  public class StoreInfo
  {
    public long StoreId { get; set; }

    public string Address { get; set; }

    public string ReturnTime { get; set; }

    public Decimal RentalTaxRate { get; set; }

    public Decimal PurchaseTaxRate { get; set; }

    public Decimal DigitalPurchaseTaxRate { get; set; }

    public Decimal PurchasePrice { get; set; }

    public long VendorId { get; set; }

    public long MarketId { get; set; }

    public long BannerId { get; set; }

    public bool Sellthru { get; set; }

    public DateTime? OpenDate { get; set; }

    public bool InvSync { get; set; }

    public string Address1 { get; set; }

    public string Address2 { get; set; }

    public string City { get; set; }

    public string County { get; set; }

    public string State { get; set; }

    public string Zip { get; set; }

    public int? CollectionMethod { get; set; }

    public Decimal ServiceFee { get; set; }

    public Decimal ServiceFeeTaxRate { get; set; }

    public MarketInfo Market { get; set; }

    public DateTime? ReturnOnlyModeDate { get; set; }
  }
}
