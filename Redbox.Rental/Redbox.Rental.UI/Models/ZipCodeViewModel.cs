using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.Ads;

namespace Redbox.Rental.UI.Models
{
    public class ZipCodeViewModel : DependencyObject
    {
        public static readonly DependencyProperty KeypadModelProperty = DependencyProperty.Register("KeypadModel",
            typeof(KeypadModel), typeof(ZipCodeViewModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty AdImageSourceProperty = DependencyProperty.Register("AdImageSource",
            typeof(BitmapImage), typeof(ZipCodeViewModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty AdImageVisibilityProperty =
            DependencyProperty.Register("AdImageVisibility", typeof(Visibility), typeof(ZipCodeViewModel),
                new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty StackPanelMarginProperty =
            DependencyProperty.Register("StackPanelMargin", typeof(Thickness), typeof(ZipCodeViewModel),
                new FrameworkPropertyMetadata(new Thickness(0.0, 49.0, 0.0, 0.0)));

        public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register("DisplayText",
            typeof(string), typeof(ZipCodeViewModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty IsNextButtonEnabledProperty = DependencyProperty.Register(
            "IsNextButtonEnabled", typeof(bool), typeof(ZipCodeViewModel), new FrameworkPropertyMetadata(false)
            {
                AffectsRender = true
            });

        public Func<ISpeechControl> OnGetSpeechControl;

        public Action<string> OnNextButtonClicked;
        public int RequiredLength { get; set; }

        public string HeaderLabelText { get; set; }

        public string Header2LabelText { get; set; }

        public string Note1LabelText { get; set; }

        public string Note2LabelText { get; set; }

        public string NextButtonText { get; set; }

        public KeypadModel KeypadModel
        {
            get { return Dispatcher.Invoke(() => (KeypadModel)GetValue(KeypadModelProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(KeypadModelProperty, value); }); }
        }

        public BitmapImage AdImageSource
        {
            get { return Dispatcher.Invoke(() => (BitmapImage)GetValue(AdImageSourceProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(AdImageSourceProperty, value); }); }
        }

        public IAdImpression AdImpression { get; set; }

        public Visibility AdImageVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(AdImageVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(AdImageVisibilityProperty, value); }); }
        }

        public Thickness StackPanelMargin
        {
            get { return Dispatcher.Invoke(() => (Thickness)GetValue(StackPanelMarginProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(StackPanelMarginProperty, value); }); }
        }

        public string DisplayText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(DisplayTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(DisplayTextProperty, value); }); }
        }

        public string TTSDisplayText
        {
            get
            {
                var text = "";
                var displayText = DisplayText;
                for (var i = 0; i < displayText.Length; i++) text = text + displayText[i] + " ";
                return text;
            }
        }

        public bool IsNextButtonEnabled
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(IsNextButtonEnabledProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(IsNextButtonEnabledProperty, value); }); }
        }

        public void ProcessOnNextButtonClicked(string displayText)
        {
            if (OnNextButtonClicked != null) OnNextButtonClicked(displayText);
        }

        public ISpeechControl ProcessGetSpeechControl()
        {
            ISpeechControl speechControl = null;
            if (OnGetSpeechControl != null) speechControl = OnGetSpeechControl();
            return speechControl;
        }
    }
}