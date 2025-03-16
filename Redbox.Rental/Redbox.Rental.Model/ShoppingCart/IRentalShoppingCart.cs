using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Promotion;
using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.ShoppingCart
{
    public interface IRentalShoppingCart
    {
        Guid ShoppingSessionId { get; }

        int Count { get; }

        void Abandon(string reason);

        bool AddItem(IBaseRentalShoppingCartItem rentalShoppingCartItem);

        bool AddTitleItem(
            IRentalShoppingCartTitleItem rentalShoppingCartItem,
            IPricingRecord pricingRecord);

        bool AddSubscriptionItem(
            IRentalShoppingCartSubscriptionItem rentalShoppingCartItem);

        void RemoveItem(IBaseRentalShoppingCartItem rentalShoppingCartItem, bool userRemoved);

        void Clear();

        IBaseRentalShoppingCartItem GetItem(int index);

        bool HasTitleItem(long productId);

        bool HasAnyTitleRollupTitle(ITitleRollupProduct titleRollupProduct);

        bool HasSubscriptionItem(ISubscriptionProduct subscriptionProduct);

        bool HasSubscriptionItem(SubscriptionType subscriptionType);

        bool HasItemAction(RentalShoppingCartItemAction action);

        bool HasItemWithAction(long productId, RentalShoppingCartItemAction action);

        bool HasMultiDiscItem();

        bool IsFull { get; }

        IList<IBaseRentalShoppingCartItem> Items { get; }

        IList<IRentalShoppingCartTitleItem> TitleItems { get; }

        IList<IRentalShoppingCartSubscriptionItem> SubscriptionItems { get; }

        event RentalShoppingCartChanged OnChange;

        void TriggerChangeNotification();

        int MaximumCapacity { get; }

        IUpsellInfo UpsellInfo { get; }

        IComingSoonOptinInfo ComingSoonOptInInfo { get; }

        bool InCartPromoOfferMade { get; set; }

        bool InCartPromoOfferCancel { get; set; }

        bool InCartDoNotShow { get; set; }

        bool InCartTitleMarketing { get; set; }

        void ClearPromoCode();

        IPromoCodeValidationInfo ValidateAndUpdatePromoCode();

        void ReValidatePromoCodeAfterVend();

        ITotals Totals { get; }

        IPromotion Promotion { get; }

        bool PickupRecommendationsShown { get; set; }

        bool PickupRecommendationsSelected { get; }

        OptInConfirmationStatus MarketingOptInConfirmationStatus { get; set; }

        void ClearLoyaltyPointsToBeEarned();

        void ClearAllLoyaltyPointsRecords();

        IList<IRentalShoppingCartTitleItem> RedeemableItems { get; }

        IList<IRentalShoppingCartTitleItem> ToBeRedeemedItems { get; }

        int TotalPointsToBeEarned { get; }

        bool HasItemsToBeRedeemed { get; }

        bool HasRedboxPlusFreeOneNightRentalSelected { get; }

        List<IRentalShoppingCartTitleItem> GetTitleItemsByCriteria(
            IRentalShoppingCartFilterCriteria filterCriteria);

        List<IRentalShoppingCartTitleItem> GetTitleItemsByCriteria(
            RentalShoppingCartItemAction? action,
            RentalShoppingCartItemSequence? sequence,
            TitleType titleType,
            TitleFamily titleFamily,
            List<long> includeProducts,
            List<long> excludeProducts,
            List<TitleType> productTypes,
            int? productTypeId,
            List<int> formatIds,
            bool includeMultiNightPricedItems,
            bool includeFlashDeals,
            bool includeLoyaltyRedeemedItems,
            bool? includeRedboxPlusFreeOneNightRentalItems,
            bool includeDiscountRestrictionItems);

        IServiceFee ServiceFee { get; set; }

        bool ServiceFeeEnabled { get; }

        decimal DefaultServiceFee { get; }

        void UpdateServiceFee();

        void SetDefaultServiceFee(decimal defaultServiceFee);

        IRentalShoppingCart ShallowClone();
    }
}