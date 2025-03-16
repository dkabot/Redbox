using Redbox.Core;
using Redbox.Rental.Model.DataService;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Personalization
{
    public class StoredPromoCodeCollection : List<IStoredPromoCode>
    {
        public List<IStoredPromoCode> GetRedboxPlusPromos()
        {
            var redboxPlusPromos = new List<IStoredPromoCode>();
            var service = ServiceLocator.Instance.GetService<IDataService>();
            foreach (var storedPromoCode in (List<IStoredPromoCode>)this)
                if (service?.GetRedboxPlusPromoCampaign(storedPromoCode.CampaignId) != null)
                    redboxPlusPromos.Add(storedPromoCode);
            return redboxPlusPromos;
        }
    }
}