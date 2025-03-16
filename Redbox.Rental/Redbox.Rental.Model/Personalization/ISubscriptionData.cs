using System.Collections.Generic;

namespace Redbox.Rental.Model.Personalization
{
    public interface ISubscriptionData
    {
        bool IsEligibleForSubscription(string subscriptionPlanId);

        bool SubscriptionServiceIsDown { get; set; }

        List<IExistingSubscription> ExistingSubscriptions { get; set; }

        List<string> EligibleSubscriptionPlanIds { get; set; }

        string TempPassword { get; set; }
    }
}