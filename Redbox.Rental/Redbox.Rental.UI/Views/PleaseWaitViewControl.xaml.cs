using System.Windows.Controls;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "PleaseWaitView")]
    public partial class PleaseWaitViewControl : ViewUserControl
    {
        public PleaseWaitViewControl()
        {
            InitializeComponent();
            ApplyTheme<Control>(MainControl);
            SpeechViewName = "please_wait_view";
        }

        private PleaseWaitViewModel Model => DataContext as PleaseWaitViewModel;

        protected override void InitSpeechPart(ISpeechPart speechPart)
        {
            var texts = speechPart.Texts;
            var textPart = new TextPart();
            var model = Model;
            textPart.Text = model != null ? model.TitleText : null;
            texts.Add(textPart);
            var texts2 = speechPart.Texts;
            var textPart2 = new TextPart();
            var model2 = Model;
            textPart2.Text = model2 != null ? model2.MessageText : null;
            texts2.Add(textPart2);
        }
    }
}