using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "MemberPerksPopupView")]
    public partial class MemberPerksPopupView : TextToSpeechUserControl
    {
        public MemberPerksPopupView()
        {
            InitializeComponent();
        }

        public override ISpeechControl GetSpeechControl()
        {
            var memberPerksPopupViewModel = (MemberPerksPopupViewModel)DataContext;
            if (memberPerksPopupViewModel == null) return null;
            var processGetSpeechControl = memberPerksPopupViewModel.ProcessGetSpeechControl;
            if (processGetSpeechControl == null) return null;
            return processGetSpeechControl();
        }
    }
}