using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "PromoConfirmationView")]
    public partial class PromoConfirmationViewUserControl : TextToSpeechUserControl
    {
        public PromoConfirmationViewUserControl()
        {
            InitializeComponent();
        }

        private PromoConfirmationViewModel Model => DataContext as PromoConfirmationViewModel;

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme != null) theme.SetStyle(CancelButton);
            if (theme == null) return;
            theme.SetStyle(ApplyButton);
        }

        public override ISpeechControl GetSpeechControl()
        {
            var model = Model;
            if (model == null) return null;
            return model.OnGetSpeechControl();
        }
    }
}