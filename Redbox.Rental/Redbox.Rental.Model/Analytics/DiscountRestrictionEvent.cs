namespace Redbox.Rental.Model.Analytics
{
    public class DiscountRestrictionEvent : AnalyticsEvent
    {
        public DiscountRestrictionEvent(string restrictedPromotionName, string offeredPromotionName)
        {
            EventType = "DiscountRestriction";
            RestrictedPromotionName = restrictedPromotionName;
            OfferedPromotionName = offeredPromotionName;
        }

        public string RestrictedPromotionName { get; set; }

        public string OfferedPromotionName { get; set; }
    }
}