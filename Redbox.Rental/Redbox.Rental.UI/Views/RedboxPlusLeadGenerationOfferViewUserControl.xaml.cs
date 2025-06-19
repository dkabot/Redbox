using System.Windows.Input;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "RedboxPlusLeadGenerationOfferView")]
    public partial class RedboxPlusLeadGenerationOfferViewUserControl : TextToSpeechUserControl, IWPFActor
    {
        public RedboxPlusLeadGenerationOfferViewUserControl()
        {
            InitializeComponent();
        }

        private RedboxPlusLeadGenerationOfferViewModel RedboxPlusLeadGenerationOfferViewModel =>
            DataContext as RedboxPlusLeadGenerationOfferViewModel;

        private void NoButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var redboxPlusLeadGenerationOfferViewModel = RedboxPlusLeadGenerationOfferViewModel;
            if (redboxPlusLeadGenerationOfferViewModel == null) return;
            redboxPlusLeadGenerationOfferViewModel.ProcessOnNoButtonClicked();
        }

        private void EmailButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var redboxPlusLeadGenerationOfferViewModel = RedboxPlusLeadGenerationOfferViewModel;
            if (redboxPlusLeadGenerationOfferViewModel == null) return;
            redboxPlusLeadGenerationOfferViewModel.ProcessOnEmailButtonClicked();
        }

        private void QRCodeButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var redboxPlusLeadGenerationOfferViewModel = RedboxPlusLeadGenerationOfferViewModel;
            if (redboxPlusLeadGenerationOfferViewModel == null) return;
            redboxPlusLeadGenerationOfferViewModel.ProcessOnQRCodeButtonClicked();
        }
    }
}