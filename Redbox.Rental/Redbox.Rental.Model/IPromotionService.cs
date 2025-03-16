using Redbox.Rental.Model.KioskClientService.Campaign;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.ShoppingCart;
using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model
{
    public interface IPromotionService
    {
        IPromotion GetPromotion();

        void SetPromotion(IPromotion promotion);

        void ClearPromotion();

        bool IsFreeInCartOfferRunning { get; }

        bool NeedToShowFreeInCartMessage { get; }

        bool IsRentTwoInCartOfferRunning { get; }

        bool NeedToShowRentTwoInCartMessage { get; }

        bool IsExcludeEmptyCaseInCartOfferRunning { get; }

        bool IsExclude4KInCartOfferRunning { get; }

        bool IsExcludeDiscountRestrictionInCartOfferRunning { get; }

        bool IsFirstNightFreeItemInCartOfferRunning { get; }

        bool NeedToShowReturnVisitIneligibleCartWarning { get; }

        void LoadInCartData(KioskInCartDetails inCart);

        IPromoCodeValidationInfo ValidateFreeInCartOffer();

        IPromoCodeValidationInfo ValidateRentTwoInCartPromo();

        IPromoCodeValidationInfo ValidateRegularInCartPromo(IRentalShoppingCart rentalShoppingCart);

        List<long> GetInCartIncludeTitleList();

        List<long> GetInCartExcludeTitleList();

        List<TitleType> GetInCartExcludeTitleTypeList();

        bool GetInCartExcludeDiscountRestriction();

        bool CartQualifiesForInCartOffer(IRentalShoppingCart rentalShoppingCart);

        InCartOfferMessageText GetInCartOfferMessageText();

        void SetFreeInCartPromo(IRentalShoppingCart rentalShoppingCart);

        void SetRentTwoInCartPromo(IRentalShoppingCart rentalShoppingCart);

        void SetPromoToFixedPriceInCart(IRentalShoppingCart rentalShoppingCart, decimal promoValue);

        void SetPromoToFirstNightFreeItemInCart(IRentalShoppingCart rentalShoppingCart);

        IPromotionRule GetInCartPromoActionRule();

        void SetPromoFromCustomerOffer(
            IRentalShoppingCart rentalShoppingCart,
            CustomerOffer offer,
            IRentalShoppingCartTitleItem applyToLineItem);

        void ValidatePromoCodeOnServer(
            string promoCode,
            Action<IPromoCodeValidationResult> promoCodeValidationResultCallback);

        string GetInCartPromoCode();

        decimal CalcInCartOfferPromoAmount(
            IRentalShoppingCartTitleItem newLineItem,
            CustomerOffer offer);
    }
}