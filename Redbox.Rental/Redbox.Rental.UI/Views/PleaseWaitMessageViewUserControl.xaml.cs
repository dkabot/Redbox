using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "PleaseWaitMessageView")]
    public partial class PleaseWaitMessageViewUserControl : TextToSpeechUserControl
    {
        public PleaseWaitMessageViewUserControl()
        {
            InitializeComponent();
        }

        private PleaseWaitMessageViewModel ViewModel => DataContext as PleaseWaitMessageViewModel;

        public override ISpeechControl GetSpeechControl()
        {
            var viewModel = ViewModel;
            if (viewModel == null) return null;
            return viewModel.ProcessGetSpeechControl();
        }
    }
}