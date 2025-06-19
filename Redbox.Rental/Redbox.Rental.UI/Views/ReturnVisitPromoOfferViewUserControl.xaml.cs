using System.Windows.Controls;
using System.Windows.Input;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "ReturnVisitPromoOfferView")]
    public partial class ReturnVisitPromoOfferViewUserControl : ViewUserControl
    {
        public ReturnVisitPromoOfferViewUserControl()
        {
            InitializeComponent();
            ApplyTheme<Control>(MainControl);
        }

        private ReturnVisitPromoOfferViewModel ReturnVisitPromoOfferViewModel =>
            DataContext as ReturnVisitPromoOfferViewModel;

        private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var returnVisitPromoOfferViewModel = ReturnVisitPromoOfferViewModel;
            if (returnVisitPromoOfferViewModel == null) return;
            returnVisitPromoOfferViewModel.ProcessOnCloseButtonClicked();
        }

        private void UseNowCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var returnVisitPromoOfferViewModel = ReturnVisitPromoOfferViewModel;
            if (returnVisitPromoOfferViewModel == null) return;
            returnVisitPromoOfferViewModel.ProcessOnUseNowButtonClicked();
        }
    }
}