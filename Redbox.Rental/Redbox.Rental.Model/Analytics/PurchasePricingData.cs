using Redbox.Rental.Model.Pricing;
using System;

namespace Redbox.Rental.Model.Analytics
{
    public class PurchasePricingData : AnalyticsData
    {
        public PurchasePricingData()
        {
            DataType = "PurchasePricing";
        }

        public long? ProductId { get; set; }

        public long? ProductPricingId { get; set; }

        public decimal Purchase { get; set; }

        public static PricingData ToPricingData(long productId, IPricingRecord pricingRecord)
        {
            return new PricingData()
            {
                ProductId = new long?(productId),
                ProductPricingId = pricingRecord.ProductPricingId,
                Purchase = pricingRecord.Purchase
            };
        }
    }
}