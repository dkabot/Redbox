using Redbox.Rental.Model.ShoppingCart;
using System;

namespace Redbox.Rental.Model
{
    public interface IPromotionItem
    {
        IRentalShoppingCartTitleItem RentalShoppingCartItem { get; set; }

        decimal PromotionAmount { get; set; }
    }
}