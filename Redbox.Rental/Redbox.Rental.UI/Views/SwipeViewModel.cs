using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.Rental.Model.Ads;

namespace Redbox.Rental.UI.Views
{
    public class SwipeViewModel : DependencyObject
    {
        public static readonly DependencyProperty BackButtonVisibilityProperty =
            DependencyProperty.Register("BackButtonVisibility", typeof(Visibility), typeof(SwipeViewModel),
                new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty ImageStackPanelVisibilityProperty =
            DependencyProperty.Register("ImageStackPanelVisibility", typeof(Visibility), typeof(SwipeViewModel),
                new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty ApplePayImageVisibilityProperty =
            DependencyProperty.Register("ApplePayImageVisibility", typeof(Visibility), typeof(SwipeViewModel),
                new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty GooglePayImageVisibilityProperty =
            DependencyProperty.Register("GooglePayImageVisibility", typeof(Visibility), typeof(SwipeViewModel),
                new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty TimerTextBorderVisibilityProperty =
            DependencyProperty.Register("TimerTextBorderVisibility", typeof(Visibility), typeof(SwipeViewModel),
                new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty TitleTextVisibilityProperty =
            DependencyProperty.Register("TitleTextVisibility", typeof(Visibility), typeof(SwipeViewModel),
                new FrameworkPropertyMetadata(Visibility.Visible));

        public Action OnBackButtonClicked;

        public Func<ISpeechControl> OnGetSpeechControl;

        public Action OnMessageButtonClicked;

        public Action OnTimeout;
        public Brush GridBackground { get; set; }

        public string TitleText { get; set; }

        public string TitleNoteText { get; set; }

        public string MessageText { get; set; }

        public string MessageButtonText { get; set; }

        public string TimerTitleText { get; set; }

        public string TimerMessageText { get; set; }

        public string BackButtonText { get; set; }

        public string ChipNotEnabledText { get; set; }

        public ICardReadAttempt CardReadAttempt { get; set; }

        public BitmapImage AdImage { get; set; }

        public IAdImpression AdImpression { get; set; }

        public Visibility BackButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(BackButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BackButtonVisibilityProperty, value); }); }
        }

        public Visibility ImageStackPanelVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(ImageStackPanelVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ImageStackPanelVisibilityProperty, value); }); }
        }

        public Visibility ApplePayImageVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(ApplePayImageVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ApplePayImageVisibilityProperty, value); }); }
        }

        public Visibility GooglePayImageVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(GooglePayImageVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(GooglePayImageVisibilityProperty, value); }); }
        }

        public Visibility TimerTextBorderVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(TimerTextBorderVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(TimerTextBorderVisibilityProperty, value); }); }
        }

        public Visibility TitleTextVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(TitleTextVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(TitleTextVisibilityProperty, value); }); }
        }

        public void ProcessOnMessageButtonClicked()
        {
            var onMessageButtonClicked = OnMessageButtonClicked;
            if (onMessageButtonClicked == null) return;
            onMessageButtonClicked();
        }

        public void ProcessOnBackButtonClicked()
        {
            var onBackButtonClicked = OnBackButtonClicked;
            if (onBackButtonClicked == null) return;
            onBackButtonClicked();
        }

        public void ProcessOnTimeout()
        {
            var onTimeout = OnTimeout;
            if (onTimeout == null) return;
            onTimeout();
        }
    }
}