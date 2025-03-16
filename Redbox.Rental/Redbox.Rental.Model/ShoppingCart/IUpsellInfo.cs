using Redbox.Rental.Model.KioskProduct;
using System.Collections.Generic;

namespace Redbox.Rental.Model.ShoppingCart
{
    public interface IUpsellInfo
    {
        bool GetDoNotShowAgain(TitleType titleType);

        void SetDoNotShowAgain(TitleType titleType, bool value);

        IList<IUpsellItem> Items { get; }

        bool GetDialogNeedsToBeShown(TitleType titleType);

        void AddMissingUpsellItems(TitleType titleType);

        void RejectUndecidedUpsellItems();

        void RemoveUndecidedUpsellItems();

        void SetUpsellOffer(long originalProductId, UpsellItemOfferResponse offerResponse);

        IUpsellItem GetUpsellItemByRentalShoppingCartItem(
            IRentalShoppingCartTitleItem rentalShoppingCartItem);
    }
}