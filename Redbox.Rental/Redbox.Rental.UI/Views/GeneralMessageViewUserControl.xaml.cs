using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "GeneralMessageView")]
    public partial class GeneralMessageViewUserControl : TextToSpeechUserControl, IWPFActor
    {
        public GeneralMessageViewUserControl()
        {
            InitializeComponent();
        }

        private GeneralMessageViewModel Model => DataContext as GeneralMessageViewModel;

        public override ISpeechControl GetSpeechControl()
        {
            ISpeechControl speechControl = new SpeechControl();
            speechControl.Name = "tts_message_view";
            AddSpeechPartCheckForCartEmpty(speechControl);
            return speechControl;
        }

        private void AddSpeechPartCheckForCartEmpty(ISpeechControl speechControl)
        {
            ISpeechPart speechPart = new SpeechPart
            {
                Sequence = 0,
                Name = "Main",
                StartPause = 250
            };
            speechPart.Refresh = delegate
            {
                speechPart.Clear();
                var texts = speechPart.Texts;
                var textPart = new TextPart();
                var model = Model;
                textPart.Text = model != null ? model.MessageText : null;
                texts.Add(textPart);
            };
            speechControl.SpeechParts.Add(speechPart);
        }
    }
}