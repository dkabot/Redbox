using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "RedboxPlusOfferSelectionView")]
    public partial class RedboxPlusOfferSelectionUserControl : TextToSpeechUserControl, IWPFActor
    {
        public RedboxPlusOfferSelectionUserControl()
        {
            InitializeComponent();
        }
    }
}