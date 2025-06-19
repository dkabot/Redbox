using System.Windows;
using System.Windows.Media;

namespace Redbox.Rental.UI.Models
{
    public class DisplayCheckoutProductModel : DisplayProductModel
    {
        public static readonly DependencyProperty OfferActionVisibilityProperty = DependencyProperty.Register(
            "OfferActionVisibility", typeof(Visibility), typeof(DisplayCheckoutProductModel),
            new FrameworkPropertyMetadata(Visibility.Collapsed)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty TopCancelButtonVisibilityProperty = DependencyProperty.Register(
            "TopCancelButtonVisibility", typeof(Visibility), typeof(DisplayCheckoutProductModel),
            new FrameworkPropertyMetadata(Visibility.Collapsed)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty BottomCancelButtonVisibilityProperty = DependencyProperty.Register(
            "BottomCancelButtonVisibility", typeof(Visibility), typeof(DisplayCheckoutProductModel),
            new FrameworkPropertyMetadata(Visibility.Collapsed)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty ProductDescriptionLine2TextProperty = DependencyProperty.Register(
            "ProductDescriptionLine2Text", typeof(string), typeof(DisplayCheckoutProductModel),
            new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty ProductDescriptionLine3TextProperty = DependencyProperty.Register(
            "ProductDescriptionLine3Text", typeof(string), typeof(DisplayCheckoutProductModel),
            new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty ProductDescriptionLine3TextVisibilityProperty =
            DependencyProperty.Register("ProductDescriptionLine3TextVisibility", typeof(Visibility),
                typeof(DisplayCheckoutProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
                {
                    AffectsRender = true
                });

        public static readonly DependencyProperty DetailsButtonVisibilityProperty = DependencyProperty.Register(
            "DetailsButtonVisibility", typeof(Visibility), typeof(DisplayCheckoutProductModel),
            new FrameworkPropertyMetadata(Visibility.Collapsed)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty RedboxPlusMovieInfoVisibilityProperty = DependencyProperty.Register(
            "RedboxPlusMovieInfoVisibility", typeof(Visibility), typeof(DisplayCheckoutProductModel),
            new FrameworkPropertyMetadata(Visibility.Collapsed)
            {
                AffectsRender = true
            });

        public string ProductTypeText { get; set; }

        public string ProductTitleText { get; set; }

        public string ProductCartPriceText { get; set; }

        public string RedboxPlusMovieInfoButtonText { get; set; }

        public DynamicRoutedCommand RedboxPlusInfoCommand { get; set; }

        public DisplayProductCheckoutSpecialOfferActionModel OfferActionModel { get; set; }

        public Visibility TopCancelButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(TopCancelButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(TopCancelButtonVisibilityProperty, value); }); }
        }

        public Visibility BottomCancelButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(BottomCancelButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BottomCancelButtonVisibilityProperty, value); }); }
        }

        public string ProductDescriptionLine2Text
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(ProductDescriptionLine2TextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ProductDescriptionLine2TextProperty, value); }); }
        }

        public string ProductDescriptionLine3Text
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(ProductDescriptionLine3TextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ProductDescriptionLine3TextProperty, value); }); }
        }

        public Visibility ProductDescriptionLine3TextVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(ProductDescriptionLine3TextVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ProductDescriptionLine3TextVisibilityProperty, value); }); }
        }

        public Visibility OfferActionVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(OfferActionVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(OfferActionVisibilityProperty, value); }); }
        }

        public Visibility DetailsButtonVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(DetailsButtonVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(DetailsButtonVisibilityProperty, value); }); }
        }

        public Visibility RedboxPlusMovieInfoVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(RedboxPlusMovieInfoVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RedboxPlusMovieInfoVisibilityProperty, value); }); }
        }

        public void ConfigureForCheckout()
        {
            FlagVisibility = Visibility.Collapsed;
            ImageBorderColor = Color.FromRgb(60, 56, 57);
            ImageBorderThickness = new Thickness(1.0);
            ImageBorderCornerRadius = 0.0;
            ImageOpacity = 1.0;
            ImageBackgroundVisibility = Visibility.Collapsed;
            AddButtonOpacity = 1.0;
            CancelButtonVisibility = Visibility.Collapsed;
            CornerAddButtonVisibility = Visibility.Collapsed;
            AddButtonVisibility = Visibility.Collapsed;
            ADAMiniCartAddButtonVisibility = Visibility.Collapsed;
            OfferActionVisibility = Visibility.Collapsed;
            RedboxPlusMovieInfoVisibility = Visibility.Collapsed;
            OfferActionModel = new DisplayProductCheckoutSpecialOfferActionModel();
        }
    }
}