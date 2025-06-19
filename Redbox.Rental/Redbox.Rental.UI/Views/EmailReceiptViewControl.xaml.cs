using System.Windows;
using System.Windows.Controls;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "EmailReceiptView")]
    public partial class EmailReceiptViewControl : ViewUserControl
    {
        public EmailReceiptViewControl()
        {
            InitializeComponent();
            ApplyTheme<Control>(MainControl);
            SpeechViewName = "email_receipt_view";
        }

        protected override void DataContextHasChanged()
        {
            var model = GetModel<SimpleModel>();
            var flag = (model != null ? model.BannerImage : null) != null;
            AdImage.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void InitSpeechPart(ISpeechPart speechPart)
        {
            var model = GetModel<SimpleModel>();
            speechPart.Texts.Add(new TextPart
            {
                Text = model != null ? model.TitleText : null
            });
            speechPart.Texts.Add(new TextPart
            {
                Text = model != null ? model.MessageText : null
            });
            speechPart.Texts.Add(new TextPart
            {
                Text = model.CommentText
            });
            speechPart.Texts.Add(new TextPart
            {
                Text = "To continue press ENTER"
            });
            speechPart.Events.Add(new MapKeyPressToAction
            {
                KeyCode = "ENTER",
                Function = "TTSRepeatSpeechPart",
                Action = delegate
                {
                    var model2 = GetModel<SimpleModel>();
                    InvokeRoutedCommand(model2 != null ? model2.ContinueButtonCommand : null);
                }
            });
            speechPart.Loop = true;
        }
    }
}