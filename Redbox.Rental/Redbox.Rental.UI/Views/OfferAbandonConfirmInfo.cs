using System;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.UI.Views
{
    public class OfferAbandonConfirmInfo : IViewAnalyticsName
    {
        public bool IsInCart { get; set; }

        public Action ContinueAction { get; set; }
        public string ViewAnalyticsName { get; set; }
    }
}