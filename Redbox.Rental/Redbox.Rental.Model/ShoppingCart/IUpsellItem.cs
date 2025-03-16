namespace Redbox.Rental.Model.ShoppingCart
{
    public interface IUpsellItem
    {
        bool MatchesOrginalOrUpsellProductId(long productId);

        long OriginalProductId { get; set; }

        long UpsellProductId { get; set; }

        UpsellItemOfferResponse OfferResponse { get; set; }

        long ProductGroupId { get; set; }

        UpsellItemType TypeId { get; set; }
    }
}