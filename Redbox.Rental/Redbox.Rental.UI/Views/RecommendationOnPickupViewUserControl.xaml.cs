using System.Windows.Input;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "RecommendationOnPickupView")]
    public partial class RecommendationOnPickupViewUserControl : TextToSpeechUserControl, IWPFActor
    {
        public RecommendationOnPickupViewUserControl()
        {
            InitializeComponent();
        }

        private RecommendationOnPickupViewModel Model => DataContext as RecommendationOnPickupViewModel;

        public override ISpeechControl GetSpeechControl()
        {
            var model = Model;
            if (model == null) return null;
            return model.ProcessGetSpeechControl();
        }

        private void PickupCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model == null) return;
            model.ProcessOnPickupButtonClicked();
        }

        private void PayCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model == null) return;
            model.ProcessOnPayButtonClicked();
        }

        private void TermsAndPrivacyCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model == null) return;
            model.ProcessOnTermsAndPrivacyButtonClicked();
        }
    }
}