using System.Windows.Controls;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "SignInThanksView")]
    public partial class SignInThanksViewUserControl : ViewUserControl
    {
        public SignInThanksViewUserControl()
        {
            InitializeComponent();
            ApplyTheme<Control>(MainControl);
            SpeechViewName = "please_wait_view";
        }

        private SignInThanksViewModel Model => DataContext as SignInThanksViewModel;

        protected override void InitSpeechPart(ISpeechPart speechPart)
        {
            var texts = speechPart.Texts;
            var textPart = new TextPart();
            var model = Model;
            textPart.Text = model != null ? model.MessageText : null;
            texts.Add(textPart);
            var texts2 = speechPart.Texts;
            var textPart2 = new TextPart();
            var model2 = Model;
            textPart2.Text = model2 != null ? model2.ThanksText : null;
            texts2.Add(textPart2);
        }
    }
}