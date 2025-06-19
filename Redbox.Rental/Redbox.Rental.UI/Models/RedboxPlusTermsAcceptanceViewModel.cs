using System;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models
{
    public class RedboxPlusTermsAcceptanceViewModel : DependencyObject
    {
        public static readonly DependencyProperty AcceptButtonIsEnabledProperty =
            DependencyProperty.Register("AcceptButtonIsEnabled", typeof(bool),
                typeof(RedboxPlusTermsAcceptanceViewModel), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty CheckMarkVisibilityProperty =
            DependencyProperty.Register("CheckMarkVisibility", typeof(Visibility),
                typeof(RedboxPlusTermsAcceptanceViewModel), new FrameworkPropertyMetadata(Visibility.Hidden));

        public static readonly DependencyProperty AcceptButtonTextProperty =
            DependencyProperty.Register("AcceptButtonText", typeof(string), typeof(RedboxPlusTermsAcceptanceViewModel),
                new FrameworkPropertyMetadata(null));

        public Func<ISpeechControl> OnGetSpeechControl;
        public DynamicRoutedCommand CheckMarkCommand { get; set; }

        public DynamicRoutedCommand AcceptButtonCommand { get; set; }

        public DynamicRoutedCommand BackButtonCommand { get; set; }

        public DynamicRoutedCommand TermsAndPrivacyButtonCommand { get; set; }

        public string TitleText { get; set; }

        public string SubtitleText { get; set; }

        public string EmailText { get; set; }

        public string TermsHeader { get; set; }

        public string TermsText { get; set; }

        public IRedboxPlusSubscriptionProduct RedboxPlusSubscriptionProduct { get; set; }

        public string AcceptButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(AcceptButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(AcceptButtonTextProperty, value); }); }
        }

        public bool AcceptButtonIsEnabled
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(AcceptButtonIsEnabledProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(AcceptButtonIsEnabledProperty, value); }); }
        }

        public Visibility CheckMarkVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(CheckMarkVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CheckMarkVisibilityProperty, value); }); }
        }

        public ISpeechControl ProcessGetSpeechControl()
        {
            var onGetSpeechControl = OnGetSpeechControl;
            if (onGetSpeechControl == null) return null;
            return onGetSpeechControl();
        }
    }
}