using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "RedboxPlusOfferDetailView")]
    public partial class RedboxPlusOfferDetailViewUserControl : TextToSpeechUserControl, IWPFActor
    {
        public RedboxPlusOfferDetailViewUserControl()
        {
            InitializeComponent();
        }
    }
}