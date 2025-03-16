using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Promotion;
using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.ShoppingCart
{
    public interface IRentalShoppingCartService
    {
        IRentalShoppingCart GetRentalShoppingCart(Guid id);

        IRentalShoppingCart CreateNew(Guid id);

        bool RemoveRentalShoppingCart(Guid id);

        void RemoveItemFromRentalShoppingCartByProductId(
            Guid shoppingCartSessionId,
            long productId,
            bool userRemoved);

        IRentalShoppingCart CurrentRentalShoppingCart { get; }

        bool CurrentRentalShoppingCartIsFull { get; }

        event RentalShoppingCartAdded OnRentalShoppingCartAdded;

        void AddMultiDiscDiscsToRentalShoppingCart(Guid shoppingCartSessionId);

        void RemoveMultiDiscDiscsFromRentalShoppingCart(Guid shoppingCartSessionId);

        void RemoveNonReservedProductsFromRentalShoppingCart(Guid shoppingCartSessionId);

        IRentalShoppingCartTitleItem CreateRentalShoppingCartItemFromTitleProduct(
            ITitleProduct titleProduct);

        IRentalShoppingCartTitleItem CreateRentalShoppingCartItemFromProductId(long productId);

        IRentalShoppingCartSubscriptionItem CreateRentalShoppingCartItemFromSubscriptionProduct(
            ISubscriptionProduct subscriptionProduct);

        void SendRentalShoppingCartToAnalytics(IRentalShoppingCart rentalShoppingCart);

        List<string> AssignBarcodesToVend(IRentalShoppingCart rentalShoppingCart);

        void RemovePromoOfferFromCart(Guid shoppingSessionId, bool force = false);

        void AddMultiNightOfferToItem(
            IRentalShoppingCartTitleItem rentalShoppingCartItem,
            CustomerOffer selectedOffer);

        void RemoveMultiNightOfferFromItem(
            IRentalShoppingCartTitleItem rentalShoppingCartItem);

        void SetPurchasePriceForSelectedItem(
            CustomerOffer selectedOffer,
            IRentalShoppingCartTitleItem newLineItem,
            IPromotionService promotionService);
    }
}