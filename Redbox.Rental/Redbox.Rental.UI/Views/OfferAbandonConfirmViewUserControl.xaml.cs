using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "OfferAbandonConfirmView")]
    public partial class OfferAbandonConfirmViewUserControl : ViewUserControl
    {
        public OfferAbandonConfirmViewUserControl()
        {
            InitializeComponent();
        }

        private OfferAbandonConfirmModel Model => DataContext as OfferAbandonConfirmModel;

        public override ISpeechControl GetSpeechControl()
        {
            return Model.ProcessGetSpeechControl();
        }
    }
}