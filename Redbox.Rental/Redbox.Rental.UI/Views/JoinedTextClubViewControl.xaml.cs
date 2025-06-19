using System.Windows.Controls;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "JoinTextClubPromoView")]
    public partial class JoinedTextClubViewControl : ViewUserControl
    {
        public JoinedTextClubViewControl()
        {
            InitializeComponent();
            ApplyTheme<Control>(MainControl);
            SpeechViewName = "join_text_club_promo_view";
        }

        private SimpleModel Model { get; set; }

        protected override void DataContextHasChanged()
        {
            if (DataContext == null || !(DataContext is SimpleModel)) return;
            Model = DataContext as SimpleModel;
        }

        protected override void InitSpeechPart(ISpeechPart speechPart)
        {
            speechPart.Texts.Add(new TextPart
            {
                Text = Model.TitleText
            });
            speechPart.Texts.Add(new TextPart
            {
                Text = Model.MessageText
            });
            speechPart.Texts.Add(new TextPart
            {
                Text = "To continue press ENTER"
            });
            speechPart.Events.Add(new MapKeyPressToAction
            {
                KeyCode = "ENTER",
                Function = "TTSRepeatSpeechPart",
                Action = delegate { InvokeRoutedCommand(Model.ContinueButtonCommand); }
            });
            speechPart.Loop = true;
        }
    }
}