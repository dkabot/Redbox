using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.UI.Models
{
    public class PerksThanksViewModel : BaseModel<PerksThanksViewModel>
    {
        public static readonly DependencyProperty DisplayImageProperty =
            CreateDependencyProperty("DisplayImage", TYPES.BITMAPIMAGE);

        public static readonly DependencyProperty TitleTextProperty =
            CreateDependencyProperty("TitleText", TYPES.STRING, string.Empty);

        public static readonly DependencyProperty MessageTextProperty =
            CreateDependencyProperty("MessageText", TYPES.STRING, string.Empty);

        public static readonly DependencyProperty CommentTextProperty =
            CreateDependencyProperty("CommentText", TYPES.STRING, string.Empty);

        public static readonly DependencyProperty FooterTextProperty =
            CreateDependencyProperty("FooterText", TYPES.STRING, string.Empty);

        public static readonly DependencyProperty ContinueButtonTextProperty =
            CreateDependencyProperty("ContinueButtonText", TYPES.STRING, string.Empty);

        public static readonly DependencyProperty PerksIconsVisibilityProperty =
            CreateDependencyProperty("PerksIconsVisibility", TYPES.VISIBILITY, Visibility.Collapsed);

        public static readonly DependencyProperty PerksFreeRentalTextProperty =
            CreateDependencyProperty("PerksFreeRentalText", TYPES.STRING, string.Empty);

        public static readonly DependencyProperty PerksDealsTextProperty =
            CreateDependencyProperty("PerksDealsText", TYPES.STRING, string.Empty);

        public static readonly DependencyProperty PerksBdayGiftTextProperty =
            CreateDependencyProperty("PerksBdayGiftText", TYPES.STRING, string.Empty);

        public static readonly DependencyProperty TitleMarginProperty =
            CreateDependencyProperty("TitleMargin", typeof(Thickness), new Thickness(0.0, 36.0, 0.0, 36.0));

        public Action<AnalyticsTypes> AnalyticsAction { get; set; }

        public Action ContinueAction { get; set; }

        public DynamicRoutedCommand ContinueButtonCommand { get; set; }

        public BitmapImage DisplayImage
        {
            get { return Dispatcher.Invoke(() => (BitmapImage)GetValue(DisplayImageProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(DisplayImageProperty, value); }); }
        }

        public string TitleText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(TitleTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(TitleTextProperty, value); }); }
        }

        public string MessageText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(MessageTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(MessageTextProperty, value); }); }
        }

        public string CommentText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(CommentTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CommentTextProperty, value); }); }
        }

        public string FooterText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(FooterTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(FooterTextProperty, value); }); }
        }

        public string ContinueButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(ContinueButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ContinueButtonTextProperty, value); }); }
        }

        public Visibility PerksIconsVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(PerksIconsVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PerksIconsVisibilityProperty, value); }); }
        }

        public string PerksFreeRentalText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PerksFreeRentalTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PerksFreeRentalTextProperty, value); }); }
        }

        public string PerksDealsText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PerksDealsTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PerksDealsTextProperty, value); }); }
        }

        public string PerksBdayGiftText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PerksBdayGiftTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PerksBdayGiftTextProperty, value); }); }
        }

        public Thickness TitleMargin
        {
            get { return Dispatcher.Invoke(() => (Thickness)GetValue(TitleMarginProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(TitleMarginProperty, value); }); }
        }
    }
}