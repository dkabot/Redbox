using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "PromoOfferBrowseView")]
    public partial class PromoOfferBrowseViewUserControl : TextToSpeechUserControl, IWPFActor
    {
        public PromoOfferBrowseViewUserControl()
        {
            InitializeComponent();
        }

        public override ISpeechControl GetSpeechControl()
        {
            var promoOfferBrowseViewModel = DataContext as PromoOfferBrowseViewModel;
            if (promoOfferBrowseViewModel == null) return null;
            return promoOfferBrowseViewModel.ProcessGetSpeechControl();
        }
    }
}