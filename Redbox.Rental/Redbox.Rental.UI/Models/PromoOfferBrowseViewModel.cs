using System;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models
{
    public class PromoOfferBrowseViewModel : DependencyObject
    {
        public static readonly DependencyProperty BrowseProductControlModelProperty =
            DependencyProperty.Register("BrowseProductControlModel", typeof(BrowseControlModel),
                typeof(PromoOfferBrowseViewModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty MiniCartControlModelProperty =
            DependencyProperty.Register("MiniCartControlModel", typeof(BrowseControlModel),
                typeof(PromoOfferBrowseViewModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty AdaMiniCartControlModelProperty =
            DependencyProperty.Register("AdaMiniCartControlModel", typeof(BrowseControlModel),
                typeof(PromoOfferBrowseViewModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty MiniCartVisibilityProperty =
            DependencyProperty.Register("MiniCartVisibility", typeof(Visibility), typeof(PromoOfferBrowseViewModel),
                new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty PageNumberTextProperty = DependencyProperty.Register("PageNumberText",
            typeof(string), typeof(PromoOfferBrowseViewModel), new FrameworkPropertyMetadata(""));

        public static readonly DependencyProperty PageNumberProperty = DependencyProperty.Register("PageNumber",
            typeof(int), typeof(PromoOfferBrowseViewModel), new FrameworkPropertyMetadata(1));

        public static readonly DependencyProperty MiniCartTextProperty = DependencyProperty.Register("MiniCartText",
            typeof(string), typeof(PromoOfferBrowseViewModel), new FrameworkPropertyMetadata(""));

        public static readonly DependencyProperty AddToADACartLabelTextProperty =
            DependencyProperty.Register("AddToADACartLabelText", typeof(string), typeof(PromoOfferBrowseViewModel),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty AddToADACartLabelVisibilityProperty =
            DependencyProperty.Register("AddToADACartLabelVisibility", typeof(Visibility),
                typeof(PromoOfferBrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty EmptyCartSpacesVisibilityProperty =
            DependencyProperty.Register("EmptyCartSpacesVisibilityVisibility", typeof(Visibility),
                typeof(PromoOfferBrowseViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

        public Func<ISpeechControl> OnGetSpeechControl;
        public string HeaderText { get; set; }

        public string CancelButtonText { get; set; }

        public string ContinueButtonText { get; set; }

        public Visibility AdaMiniCartVisibility { get; set; }

        public BrowseControlModel BrowseProductControlModel
        {
            get { return Dispatcher.Invoke(() => (BrowseControlModel)GetValue(BrowseProductControlModelProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BrowseProductControlModelProperty, value); }); }
        }

        public DynamicRoutedCommand CancelButtonCommand { get; set; }

        public DynamicRoutedCommand ContinueButtonCommand { get; set; }

        public BrowseControlModel MiniCartControlModel
        {
            get { return Dispatcher.Invoke(() => (BrowseControlModel)GetValue(MiniCartControlModelProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(MiniCartControlModelProperty, value); }); }
        }

        public BrowseControlModel AdaMiniCartControlModel
        {
            get { return Dispatcher.Invoke(() => (BrowseControlModel)GetValue(AdaMiniCartControlModelProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(AdaMiniCartControlModelProperty, value); }); }
        }

        public Visibility MiniCartVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(MiniCartVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(MiniCartVisibilityProperty, value); }); }
        }

        public string PageNumberText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PageNumberTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PageNumberTextProperty, value); }); }
        }

        public int PageNumber
        {
            get { return Dispatcher.Invoke(() => (int)GetValue(PageNumberProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PageNumberProperty, value); }); }
        }

        public string MiniCartText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(MiniCartTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(MiniCartTextProperty, value); }); }
        }

        public string AddToADACartLabelText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(AddToADACartLabelTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(AddToADACartLabelTextProperty, value); }); }
        }

        public Visibility AddToADACartLabelVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(AddToADACartLabelVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(AddToADACartLabelVisibilityProperty, value); }); }
        }

        public Visibility EmptyCartSpacesVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(EmptyCartSpacesVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(EmptyCartSpacesVisibilityProperty, value); }); }
        }

        public ISpeechControl ProcessGetSpeechControl()
        {
            ISpeechControl speechControl = null;
            if (OnGetSpeechControl != null) speechControl = OnGetSpeechControl();
            return speechControl;
        }
    }
}