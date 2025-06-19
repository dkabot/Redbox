using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "NewCartConfirmADAActionUpdateView")]
    public partial class NewCartConfirmADAActionUpdate : TextToSpeechUserControl
    {
        public NewCartConfirmADAActionUpdate()
        {
            InitializeComponent();
        }

        private NewCartConfirmADAActionUpdateViewModel Model => DataContext as NewCartConfirmADAActionUpdateViewModel;

        public override ISpeechControl GetSpeechControl()
        {
            var model = Model;
            if (model == null) return null;
            return model.ProcessGetSpeechControl();
        }
    }
}