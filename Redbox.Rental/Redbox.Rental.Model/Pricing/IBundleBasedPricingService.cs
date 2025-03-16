using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model.Pricing
{
    public interface IBundleBasedPricingService
    {
        bool Reload();

        IPricingSet CurrentPriceSet { get; }

        IPricingRecord GetProductPricingRecord(long productId);

        IPricingRecord GetProductTypePricingRecord(
            TitleFamily titleFamily,
            TitleType titleType,
            bool useAdjustedPrices);
    }
}