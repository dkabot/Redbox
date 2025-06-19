using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "SignInView")]
    public partial class SignInViewUserControl : TextToSpeechUserControl, IWPFActor
    {
        public SignInViewUserControl()
        {
            InitializeComponent();
            ApplyTheme();
        }

        private SignInViewModel SignInViewModel => DataContext as SignInViewModel;

        public override ISpeechControl GetSpeechControl()
        {
            var signInViewModel = SignInViewModel;
            if (signInViewModel == null) return null;
            return signInViewModel.ProcessGetSpeechControl();
        }

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme != null) theme.SetStyle(EmailAndPasswordButton);
            if (theme != null) theme.SetStyle(PhoneAndPinButton);
            if (theme == null) return;
            theme.SetStyle(CancelButton);
        }
    }
}