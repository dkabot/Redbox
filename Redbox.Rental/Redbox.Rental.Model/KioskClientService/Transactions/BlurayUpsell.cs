namespace Redbox.Rental.Model.KioskClientService.Transactions
{
    public class BlurayUpsell
    {
        public int ProductGroupId { get; set; }

        public string Offer { get; set; }

        public long TitleId { get; set; }

        public int? TypeId { get; set; }

        public override string ToString()
        {
            return string.Format("ProductGroupId {0}, Offer {1}, TitleId {2}, TypeId {3}", (object)ProductGroupId,
                (object)Offer, (object)TitleId, (object)TypeId);
        }
    }
}