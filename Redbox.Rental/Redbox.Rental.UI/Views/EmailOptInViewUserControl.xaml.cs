using System.Windows.Input;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "EmailOptInView")]
    public partial class EmailOptInViewUserControl : TextToSpeechUserControl
    {
        public EmailOptInViewUserControl()
        {
            InitializeComponent();
        }

        private EmailOptInViewModel Model => DataContext as EmailOptInViewModel;

        private void Button1_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model == null) return;
            model.ProcessOnYesButtonClicked();
        }

        private void Button2_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model == null) return;
            model.ProcessOnNoThanksButtonClicked();
        }
    }
}