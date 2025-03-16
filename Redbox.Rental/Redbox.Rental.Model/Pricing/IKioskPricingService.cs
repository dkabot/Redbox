using Redbox.Rental.Model.KioskProduct;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Pricing
{
    public interface IKioskPricingService
    {
        void UpdatePricingInstance(bool force = false);

        IPricingRecord GetProductTypePricingRecord(TitleFamily titleFamily, TitleType titleType);

        IPricingRecord GetActiveProductTypePricingRecord(TitleFamily titleFamily, TitleType titleType);

        IPricingRecord GetOneDayProductPricingRecord(long productId);

        List<IPricingRecord> GetProductPricingRecords(long productId);

        IPricingRecord GetPurchasePricing(ITitleProduct titleProduct);

        bool IsPricingEngineDataAvailable { get; }
    }
}