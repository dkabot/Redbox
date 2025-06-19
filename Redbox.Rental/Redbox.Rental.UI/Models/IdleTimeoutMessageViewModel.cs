using System;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models
{
    public class IdleTimeoutMessageViewModel : DependencyObject
    {
        public static readonly DependencyProperty ButtonNoTextProperty = DependencyProperty.Register("ButtonNoText",
            typeof(string), typeof(IdleTimeoutMessageViewModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ButtonYesTextProperty = DependencyProperty.Register("ButtonYesText",
            typeof(string), typeof(IdleTimeoutMessageViewModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty MessageTextProperty = DependencyProperty.Register("MessageText",
            typeof(string), typeof(IdleTimeoutMessageViewModel), new FrameworkPropertyMetadata(null));

        public Action OnButtonNoClicked;

        public Action OnButtonYesClicked;

        public Func<ISpeechControl> OnGetSpeechControl;

        public int Timeout { get; set; }

        public string ButtonNoText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(ButtonNoTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ButtonNoTextProperty, value); }); }
        }

        public string ButtonYesText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(ButtonYesTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ButtonYesTextProperty, value); }); }
        }

        public string MessageText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(MessageTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(MessageTextProperty, value); }); }
        }

        public void ProcessOnButtonNoClicked()
        {
            var onButtonNoClicked = OnButtonNoClicked;
            if (onButtonNoClicked == null) return;
            onButtonNoClicked();
        }

        public void ProcessOnButtonYesClicked()
        {
            var onButtonYesClicked = OnButtonYesClicked;
            if (onButtonYesClicked == null) return;
            onButtonYesClicked();
        }

        public ISpeechControl ProcessGetSpeechControl()
        {
            ISpeechControl speechControl = null;
            if (OnGetSpeechControl != null) speechControl = OnGetSpeechControl();
            return speechControl;
        }
    }
}