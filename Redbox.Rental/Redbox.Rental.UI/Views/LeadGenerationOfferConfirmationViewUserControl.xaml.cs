using System.Windows.Input;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "LeadGenerationOfferConfirmationView")]
    public partial class LeadGenerationOfferConfirmationViewUserControl : TextToSpeechUserControl, IWPFActor
    {
        public LeadGenerationOfferConfirmationViewUserControl()
        {
            InitializeComponent();
        }

        private LeadGenerationOfferConfirmationViewModel LeadGenerationOfferConfirmationViewModel =>
            DataContext as LeadGenerationOfferConfirmationViewModel;

        private void GotItButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var leadGenerationOfferConfirmationViewModel = LeadGenerationOfferConfirmationViewModel;
            if (leadGenerationOfferConfirmationViewModel == null) return;
            leadGenerationOfferConfirmationViewModel.ProcessOnGotItButtonClicked();
        }
    }
}