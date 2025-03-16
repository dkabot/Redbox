using Redbox.Rental.Model.ShoppingCart;
using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Promotion
{
    public interface IPromoCodeValidationInfo
    {
        bool IsCartValid { get; set; }

        bool IsCartInvalidBecauseOfMultiNightPricing { get; set; }

        bool IsCartInvalidBecauseOfFlashDeal { get; set; }

        bool IsCartInvalidBecauseOfDiscountRestriction { get; set; }

        decimal Price { get; set; }

        decimal Percent { get; set; }

        string Title { get; set; }

        string Message { get; set; }

        IList<IPromotionItem> PromotionItems { get; set; }

        IList<IRentalShoppingCartTitleItem> QualifyingItems { get; set; }
    }
}