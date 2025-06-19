using System;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Promotion;

namespace Redbox.Rental.UI.Views
{
    public class PresentOffersViewInfo : IViewAnalyticsName
    {
        public List<CustomerOffer> CustomerOffers { get; set; }

        public Action<CustomerOffer> ContinueAction { get; set; }

        public Action CancelAction { get; set; }

        public Action TimeoutAction { get; set; }
        public string ViewAnalyticsName { get; set; }
    }
}