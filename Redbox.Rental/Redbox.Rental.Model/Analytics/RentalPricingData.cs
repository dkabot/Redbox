using Redbox.Rental.Model.Pricing;
using System;

namespace Redbox.Rental.Model.Analytics
{
    public class RentalPricingData : AnalyticsData
    {
        public RentalPricingData()
        {
            DataType = "RentalPricing";
        }

        public long? ProductId { get; set; }

        public decimal InitialNight { get; set; }

        public decimal ExtraNight { get; set; }

        public long? ProductPricingId { get; set; }

        public int? InitialDays { get; set; }

        public bool IsDeal { get; set; }

        public decimal? DefaultInitialNightPrice { get; set; }

        public decimal? DefaultExtraNightPrice { get; set; }

        public static PricingData ToPricingData(long productId, IPricingRecord pricingRecord)
        {
            return new PricingData()
            {
                ProductId = new long?(productId),
                InitialNight = pricingRecord.InitialNight,
                ExtraNight = pricingRecord.ExtraNight,
                ProductPricingId = pricingRecord.ProductPricingId,
                InitialDays = pricingRecord.InitialDays,
                IsDeal = pricingRecord.IsDeal,
                DefaultInitialNightPrice = pricingRecord.DefaultInitialNightPrice,
                DefaultExtraNightPrice = pricingRecord.DefaultExtraNightPrice
            };
        }
    }
}