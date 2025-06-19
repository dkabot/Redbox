using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "SubscriptionSignUpConfirmationView")]
    public partial class SubscriptionSignUpConfirmationViewUserControl : TextToSpeechUserControl, IWPFActor
    {
        public SubscriptionSignUpConfirmationViewUserControl()
        {
            InitializeComponent();
        }

        private SubscriptionSignUpConfirmationViewModel SubscriptionSignUpConfirmationViewModel =>
            DataContext as SubscriptionSignUpConfirmationViewModel;

        public override ISpeechControl GetSpeechControl()
        {
            var subscriptionSignUpConfirmationViewModel = SubscriptionSignUpConfirmationViewModel;
            if (subscriptionSignUpConfirmationViewModel == null) return null;
            return subscriptionSignUpConfirmationViewModel.ProcessGetSpeechControl();
        }
    }
}