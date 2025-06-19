using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "PhoneAndPinView")]
    public partial class PhoneAndPinViewUserControl : TextToSpeechUserControl, IWPFActor
    {
        public PhoneAndPinViewUserControl()
        {
            InitializeComponent();
            ApplyTheme();
        }

        private PhoneAndPinViewModel PhoneAndPinViewModel => DataContext as PhoneAndPinViewModel;

        public override ISpeechControl GetSpeechControl()
        {
            var phoneAndPinViewModel = PhoneAndPinViewModel;
            if (phoneAndPinViewModel == null) return null;
            return phoneAndPinViewModel.ProcessGetSpeechControl();
        }

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme != null) theme.SetStyle(StartButton);
            if (theme != null) theme.SetStyle(SignInButton);
            if (theme == null) return;
            theme.SetStyle(TermsButton);
        }

        private void PhoneDisplayCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var phoneAndPinViewModel = PhoneAndPinViewModel;
            if (phoneAndPinViewModel == null) return;
            phoneAndPinViewModel.ProcessOnDisplayClicked(PhoneAndPinViewModel.DisplayType.Phone);
        }

        private void PinDisplayCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var phoneAndPinViewModel = PhoneAndPinViewModel;
            if (phoneAndPinViewModel == null) return;
            phoneAndPinViewModel.ProcessOnDisplayClicked(PhoneAndPinViewModel.DisplayType.Pin);
        }

        private void PinConfirmDisplayCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var phoneAndPinViewModel = PhoneAndPinViewModel;
            if (phoneAndPinViewModel == null) return;
            phoneAndPinViewModel.ProcessOnDisplayClicked(PhoneAndPinViewModel.DisplayType.PinConfirm);
        }

        private void StartCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RunCommandOnce(delegate
            {
                try
                {
                    var phoneAndPinViewModel = PhoneAndPinViewModel;
                    if (phoneAndPinViewModel != null) phoneAndPinViewModel.ProcessOnStartButtonClicked();
                }
                finally
                {
                    CompleteRunOnce();
                }
            });
        }

        private void SignInCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RunCommandOnce(delegate
            {
                try
                {
                    var phoneAndPinViewModel = PhoneAndPinViewModel;
                    if (phoneAndPinViewModel != null) phoneAndPinViewModel.ProcessOnSignInButtonClicked();
                }
                finally
                {
                    CompleteRunOnce();
                }
            });
        }

        private void TermsButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RunCommandOnce(delegate
            {
                try
                {
                    var phoneAndPinViewModel = PhoneAndPinViewModel;
                    if (phoneAndPinViewModel != null) phoneAndPinViewModel.ProcessOnTermsButtonClicked();
                }
                finally
                {
                    CompleteRunOnce();
                }
            });
        }
    }
}