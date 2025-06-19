using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.UI.Models
{
    public class RecommendationOnPickupViewModel : DependencyObject
    {
        public static readonly DependencyProperty ServiceFeeMessageProperty =
            DependencyProperty.Register("ServiceFeeMessage", typeof(string), typeof(RecommendationOnPickupViewModel),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty TitleTextProperty = DependencyProperty.Register("TitleText",
            typeof(string), typeof(RecommendationOnPickupViewModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty PayButtonTextProperty = DependencyProperty.Register("PayButtonText",
            typeof(string), typeof(RecommendationOnPickupViewModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ShoppingCartControlModelProperty =
            DependencyProperty.Register("ShoppingCartControlModel", typeof(BrowseControlModel),
                typeof(RecommendationOnPickupViewModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty PayButtonVisibilityProperty =
            DependencyProperty.Register("PayButtonVisibility", typeof(Visibility),
                typeof(RecommendationOnPickupViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty PickupButtonVisibilityProperty =
            DependencyProperty.Register("PickupButtonVisibility", typeof(Visibility),
                typeof(RecommendationOnPickupViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty BrowseProductControlModelProperty =
            DependencyProperty.Register("BrowseProductControlModel", typeof(BrowseControlModel),
                typeof(RecommendationOnPickupViewModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty PricingModelProperty = DependencyProperty.Register("PricingModel",
            typeof(PricingModel), typeof(RecommendationOnPickupViewModel), new FrameworkPropertyMetadata(null));

        public Func<ISpeechControl> OnGetSpeechControl;

        public Action OnPayButtonClicked;

        public Action OnPickupButtonClicked;

        public Action OnTermsAndPrivacyButtonClicked;

        public RecommendationOnPickupViewModel()
        {
            PricingModel = new PricingModel();
            IsViewUpdatingCart = false;
        }

        public bool HasPickupOrPayActionExecuted { get; set; }

        public bool IsViewUpdatingCart { get; set; }

        public string AddMoreText { get; set; }

        public string TermsAndPrivacyButtonText { get; set; }

        public string ChargedOnceText { get; set; }

        public string AgreeToTermsText { get; set; }

        public string PickupButtonText { get; set; }

        public List<IRentalShoppingCartTitleItem> SecondaryMdvDiscs { get; set; }

        public BrowseControlModel ShoppingCartControlModel
        {
            get { return Dispatcher.Invoke(() => (BrowseControlModel)GetValue(ShoppingCartControlModelProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ShoppingCartControlModelProperty, value); }); }
        }

        public BrowseControlModel BrowseProductControlModel
        {
            get { return Dispatcher.Invoke(() => (BrowseControlModel)GetValue(BrowseProductControlModelProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BrowseProductControlModelProperty, value); }); }
        }

        public PricingModel PricingModel
        {
            get { return Dispatcher.Invoke(() => (PricingModel)GetValue(PricingModelProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PricingModelProperty, value); }); }
        }

        public string TitleText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(TitleTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(TitleTextProperty, value); }); }
        }

        public string ServiceFeeMessage
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(ServiceFeeMessageProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ServiceFeeMessageProperty, value); }); }
        }

        public string PayButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PayButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PayButtonTextProperty, value); }); }
        }

        public Visibility PayButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(PayButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PayButtonVisibilityProperty, value); }); }
        }

        public Visibility PickupButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(PickupButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PickupButtonVisibilityProperty, value); }); }
        }

        public void ProcessOnTermsAndPrivacyButtonClicked()
        {
            var onTermsAndPrivacyButtonClicked = OnTermsAndPrivacyButtonClicked;
            if (onTermsAndPrivacyButtonClicked == null) return;
            onTermsAndPrivacyButtonClicked();
        }

        public void ProcessOnPickupButtonClicked()
        {
            var onPickupButtonClicked = OnPickupButtonClicked;
            if (onPickupButtonClicked == null) return;
            onPickupButtonClicked();
        }

        public void ProcessOnPayButtonClicked()
        {
            var onPayButtonClicked = OnPayButtonClicked;
            if (onPayButtonClicked == null) return;
            onPayButtonClicked();
        }

        public ISpeechControl ProcessGetSpeechControl()
        {
            ISpeechControl speechControl = null;
            if (OnGetSpeechControl != null) speechControl = OnGetSpeechControl();
            return speechControl;
        }
    }
}