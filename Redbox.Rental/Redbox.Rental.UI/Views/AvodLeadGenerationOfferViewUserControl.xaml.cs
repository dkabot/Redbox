using System.Windows.Input;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "AvodLeadGenerationOfferView")]
    public partial class AvodLeadGenerationOfferViewUserControl : TextToSpeechUserControl, IWPFActor
    {
        public AvodLeadGenerationOfferViewUserControl()
        {
            InitializeComponent();
        }

        private AvodLeadGenerationOfferViewModel AvodLeadGenerationOfferViewModel =>
            DataContext as AvodLeadGenerationOfferViewModel;

        private void NoButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var avodLeadGenerationOfferViewModel = AvodLeadGenerationOfferViewModel;
            if (avodLeadGenerationOfferViewModel == null) return;
            avodLeadGenerationOfferViewModel.ProcessOnNoButtonClicked();
        }

        private void EmailButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var avodLeadGenerationOfferViewModel = AvodLeadGenerationOfferViewModel;
            if (avodLeadGenerationOfferViewModel == null) return;
            avodLeadGenerationOfferViewModel.ProcessOnEmailButtonClicked();
        }

        private void QRCodeButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var avodLeadGenerationOfferViewModel = AvodLeadGenerationOfferViewModel;
            if (avodLeadGenerationOfferViewModel == null) return;
            avodLeadGenerationOfferViewModel.ProcessOnQRCodeButtonClicked();
        }
    }
}