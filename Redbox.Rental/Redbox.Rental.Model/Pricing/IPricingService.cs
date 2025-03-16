using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;
using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Pricing
{
    public interface IPricingService
    {
        void Start();

        void Stop();

        event PricingSetChange OnPricingSetChange;

        void Reload();

        void UpdatePricingInstance();

        decimal GetAdjustedPrice(
            decimal price,
            RentalShoppingCartItemAction action,
            TitleType titleType);

        decimal? GetPriceSetting(
            TitleFamily titleFamily,
            TitleType titleType,
            string settingName,
            RentalShoppingCartItemAction action);

        IPricingRecord GetProductTypePricingRecord(
            TitleFamily titleFamily,
            TitleType titleType,
            bool useAdjustedPrices);

        IPricingRecord GetActiveProductTypePricingRecord(TitleFamily titleFamily, TitleType titleType);

        IPricingRecord GetOneDayProductPricingRecord(long productId);

        IList<IPricingRecord> GetProductPricingRecords(long productId);

        IPricingRecord GetPurchasePricing(ITitleProduct titleProduct);

        decimal GetRentalTaxRate();

        decimal GetPurchaseTaxRate();

        decimal GetDigitalPurchaseTaxRate();

        decimal GetEmptyCasePrice();

        void ClearOnPricingSetChange();

        bool IsPricingEngineEnabled { get; }
    }
}