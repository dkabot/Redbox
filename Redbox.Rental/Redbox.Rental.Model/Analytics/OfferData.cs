using System;

namespace Redbox.Rental.Model.Analytics
{
    public class OfferData : AnalyticsData
    {
        public OfferData(string discountType, string promotionCode, decimal? amount)
        {
            DataType = "Offer";
            DiscountType = discountType;
            PromotionCode = promotionCode;
            Amount = amount;
        }

        public string DiscountType { get; set; }

        public string PromotionCode { get; set; }

        public decimal? Amount { get; set; }
    }
}