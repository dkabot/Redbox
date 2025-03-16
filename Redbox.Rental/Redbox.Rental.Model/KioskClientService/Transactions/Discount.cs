using System;
using System.Text;

namespace Redbox.Rental.Model.KioskClientService.Transactions
{
    public class Discount
    {
        public long ProductId { get; set; }

        public DiscountType DiscountType { get; set; }

        public string PromotionCode { get; set; }

        public decimal? PromotionCodeValue { get; set; }

        public KioskEngine.ComponentModel.KioskServices.PromotionIntentCode? PromotionIntentCode { get; set; }

        public int? RedemptionPoints { get; set; }

        public decimal Amount { get; set; }

        public short Status { get; set; }

        public bool? ApplyOnlyToProduct { get; set; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(string.Format("ProductId: {0}, DiscountType: {1}, ", (object)ProductId,
                (object)DiscountType));
            if (DiscountType == DiscountType.PromotionCode)
                stringBuilder.Append(string.Format(
                    "PromotionCode: {0}, PromotionIntentCode: {1}, PromotionCodeValue: {2:C2}, ", (object)PromotionCode,
                    (object)PromotionIntentCode, (object)PromotionCodeValue));
            else if (DiscountType == DiscountType.Loyalty)
                stringBuilder.Append(string.Format("RedemptionPoints: {0}, ", (object)RedemptionPoints));
            stringBuilder.Append(string.Format("Amount: {0:C2}, Status:{1}", (object)Amount, (object)Status));
            return stringBuilder.ToString();
        }
    }
}