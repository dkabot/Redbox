using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "VsmView")]
    public partial class VsmViewUserControl : TextToSpeechUserControl
    {
        public VsmViewUserControl()
        {
            InitializeComponent();
        }

        public override ISpeechControl GetSpeechControl()
        {
            var vsmViewModel = (VsmViewModel)DataContext;
            if (vsmViewModel == null) return null;
            var processGetSpeechControl = vsmViewModel.ProcessGetSpeechControl;
            if (processGetSpeechControl == null) return null;
            return processGetSpeechControl();
        }
    }
}