using System.Collections.Generic;

namespace Redbox.Rental.Model.Personalization
{
    public class SubscriptionData : ISubscriptionData
    {
        public SubscriptionData()
        {
            EligibleSubscriptionPlanIds = new List<string>();
            ExistingSubscriptions = new List<IExistingSubscription>();
        }

        public bool IsEligibleForSubscription(string subscriptionPlanId)
        {
            return EligibleSubscriptionPlanIds != null && EligibleSubscriptionPlanIds.Contains(subscriptionPlanId);
        }

        public bool SubscriptionServiceIsDown { get; set; }

        public List<string> EligibleSubscriptionPlanIds { get; set; }

        public List<IExistingSubscription> ExistingSubscriptions { get; set; }

        public string TempPassword { get; set; }
    }
}