using Redbox.KioskEngine.ComponentModel;
using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Transactions
{
    public class LineItem
    {
        public long ProductId { get; set; }

        public string SubscriptionId { get; set; }

        public string Barcode { get; set; }

        public VendStatus VendStatus { get; set; }

        public string ProductFamily { get; set; }

        public string ProductType { get; set; }

        public string ProductName { get; set; }

        public Pricing Pricing { get; set; }

        public BlurayUpsell BlurayUpsell { get; set; }

        public ItemSourceType? SourceType { get; set; }

        public string OfferCode { get; set; }

        public string TivoQueryId { get; set; }

        public List<string> PersonalizationTitleTags { get; set; }

        public string TempPassword { get; set; }

        public bool MultiDisc { get; set; }

        public int? DiscNumber { get; set; }

        public override string ToString()
        {
            return string.Format(
                "ProductId: {0}, SubscriptionId: {1}, Barcode: {2} VendStatus: {3}, ProductFamily: {4}, ProductType: {5}, Pricing: ({6}), BlurayUpsell: ({7}), SourceType: {8}, OfferCode: {9}",
                (object)ProductId, (object)SubscriptionId, (object)Barcode, (object)VendStatus, (object)ProductFamily,
                (object)ProductType, (object)Pricing, (object)BlurayUpsell, (object)SourceType, (object)OfferCode);
        }
    }
}