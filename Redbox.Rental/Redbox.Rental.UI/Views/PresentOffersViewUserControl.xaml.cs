using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "PresentOffersView")]
    public partial class PresentOffersViewUserControl : TextToSpeechUserControl
    {
        public PresentOffersViewUserControl()
        {
            InitializeComponent();
        }

        private PresentOffersViewModel ViewModel => DataContext as PresentOffersViewModel;

        public override ISpeechControl GetSpeechControl()
        {
            var viewModel = ViewModel;
            if (viewModel == null) return null;
            return viewModel.ProcessGetSpeechControl();
        }
    }
}