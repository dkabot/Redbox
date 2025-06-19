using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "PerksSignUpInfoPopupView")]
    public partial class PerksSignUpInfoPopupViewUserControl : TextToSpeechUserControl
    {
        public PerksSignUpInfoPopupViewUserControl()
        {
            InitializeComponent();
        }

        public override ISpeechControl GetSpeechControl()
        {
            var perksSignUpInfoPopupViewModel = (PerksSignUpInfoPopupViewModel)DataContext;
            if (perksSignUpInfoPopupViewModel == null) return null;
            var processGetSpeechControl = perksSignUpInfoPopupViewModel.ProcessGetSpeechControl;
            if (processGetSpeechControl == null) return null;
            return processGetSpeechControl();
        }
    }
}