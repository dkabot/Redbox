using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.UI.Views
{
    public class MaintenanceViewParameters : IViewAnalyticsName
    {
        public string ErrorCode { get; set; }
        public string ViewAnalyticsName { get; set; }
    }
}