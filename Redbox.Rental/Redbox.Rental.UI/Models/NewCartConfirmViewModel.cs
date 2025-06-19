using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models
{
    public class NewCartConfirmViewModel : DependencyObject
    {
        public static readonly DependencyProperty PayButtonTextProperty = DependencyProperty.Register("PayButtonText",
            typeof(string), typeof(NewCartConfirmViewModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty PricingModelProperty = DependencyProperty.Register("PricingModel",
            typeof(PricingModel), typeof(NewCartConfirmViewModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty PayButtonEnabledProperty =
            DependencyProperty.Register("PayButtonEnabled", typeof(bool), typeof(NewCartConfirmViewModel),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty DisplayProductModelsProperty =
            DependencyProperty.Register("DisplayProductModels", typeof(List<DisplayCheckoutProductModel>),
                typeof(NewCartConfirmViewModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ServiceFeeTextVisibilityProperty =
            DependencyProperty.Register("ServiceFeeTextVisibility", typeof(Visibility), typeof(NewCartConfirmViewModel),
                new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty MDVFootnoteVisibilityProperty =
            DependencyProperty.Register("MDVFootnoteVisibility", typeof(Visibility), typeof(NewCartConfirmViewModel),
                new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty SkipOfferButtonVisibilityProperty =
            DependencyProperty.Register("SkipOfferButtonVisibility", typeof(Visibility),
                typeof(NewCartConfirmViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty AdaSkipOfferButtonVisibilityProperty =
            DependencyProperty.Register("AdaSkipOfferButtonVisibility", typeof(Visibility),
                typeof(NewCartConfirmViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

        public Func<ISpeechControl> OnGetSpeechControl;

        public NewCartConfirmViewModel()
        {
            PricingModel = new PricingModel
            {
                SubtotalLineVisibility = Visibility.Visible,
                AddedDiscsLineVisibility = Visibility.Collapsed,
                ReservedDiscsLineVisibility = Visibility.Collapsed,
                PricingLineMargin = new Thickness(0.0, 7.0, 0.0, 7.0)
            };
        }

        public DynamicRoutedCommand CancelButtonCommand { get; set; }

        public DynamicRoutedCommand SubmitButtonCommand { get; set; }

        public string LabelTop { get; set; }

        public string PriceHeaderText { get; set; }

        public string BackButtonText { get; set; }

        public string PassiveSaleNote { get; set; }

        public string MDVFootnote { get; set; }

        public string ServiceFeeText { get; set; }

        public string SubmitButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PayButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PayButtonTextProperty, value); }); }
        }

        public PricingModel PricingModel
        {
            get { return Dispatcher.Invoke(() => (PricingModel)GetValue(PricingModelProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PricingModelProperty, value); }); }
        }

        public List<DisplayCheckoutProductModel> DisplayProductModels
        {
            get
            {
                return Dispatcher.Invoke(() =>
                    (List<DisplayCheckoutProductModel>)GetValue(DisplayProductModelsProperty));
            }
            set { Dispatcher.Invoke(delegate { SetValue(DisplayProductModelsProperty, value); }); }
        }

        public Visibility ServiceFeeTextVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(ServiceFeeTextVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ServiceFeeTextVisibilityProperty, value); }); }
        }

        public Visibility MDVFootnoteVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(MDVFootnoteVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(MDVFootnoteVisibilityProperty, value); }); }
        }

        public Visibility SkipOfferButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(SkipOfferButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(SkipOfferButtonVisibilityProperty, value); }); }
        }

        public Visibility AdaSkipOfferButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(AdaSkipOfferButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(AdaSkipOfferButtonVisibilityProperty, value); }); }
        }

        public event Action OnLoaded;

        public event Action<DisplayProductModel, object> OnCancelBrowseItemModel;

        public void ProcessOnCancelBrowseItemModel(DisplayProductModel displayProductModel, object parameter)
        {
            if (OnCancelBrowseItemModel != null) OnCancelBrowseItemModel(displayProductModel, parameter);
        }

        public ISpeechControl ProcessGetSpeechControl()
        {
            ISpeechControl speechControl = null;
            if (OnGetSpeechControl != null) speechControl = OnGetSpeechControl();
            return speechControl;
        }

        public void ProcessOnLoaded()
        {
            var onLoaded = OnLoaded;
            if (onLoaded == null) return;
            onLoaded();
        }
    }
}