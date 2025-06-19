using System.Windows;
using System.Windows.Controls;

namespace Redbox.Rental.UI.Controls
{
    public partial class PerksIconBanner : UserControl
    {
        public static readonly DependencyProperty PerksFreeRentalTextProperty =
            DependencyProperty.Register("PerksFreeRentalText", typeof(string), typeof(PerksIconBanner),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty PerksDealsTextProperty = DependencyProperty.Register("PerksDealsText",
            typeof(string), typeof(PerksIconBanner), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty PerksBdayGiftTextProperty =
            DependencyProperty.Register("PerksBdayGiftText", typeof(string), typeof(PerksIconBanner),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty GridMarginProperty = DependencyProperty.Register("GridMargin",
            typeof(Thickness), typeof(PerksIconBanner), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty FreeRentalIconWidthProperty =
            DependencyProperty.Register("FreeRentalIconWidth", typeof(double), typeof(PerksIconBanner),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SpecialDealsIconWidthProperty =
            DependencyProperty.Register("SpecialDealsIconWidth", typeof(double), typeof(PerksIconBanner),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty BDayIconWidthProperty = DependencyProperty.Register("BDayIconWidth",
            typeof(double), typeof(PerksIconBanner), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty BackgroundOpacityProperty =
            DependencyProperty.Register("BackgroundOpacity", typeof(double), typeof(PerksIconBanner),
                new FrameworkPropertyMetadata(0.2));

        public static readonly DependencyProperty TextStyleProperty = DependencyProperty.Register("TextStyle",
            typeof(Style), typeof(PerksIconBanner), new FrameworkPropertyMetadata(null));

        public PerksIconBanner()
        {
            InitializeComponent();
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

        public Thickness GridMargin
        {
            get { return Dispatcher.Invoke(() => (Thickness)GetValue(GridMarginProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(GridMarginProperty, value); }); }
        }

        public double FreeRentalIconWidth
        {
            get { return Dispatcher.Invoke(() => (double)GetValue(FreeRentalIconWidthProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(FreeRentalIconWidthProperty, value); }); }
        }

        public double SpecialDealsIconWidth
        {
            get { return Dispatcher.Invoke(() => (double)GetValue(SpecialDealsIconWidthProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(SpecialDealsIconWidthProperty, value); }); }
        }

        public double BDayIconWidth
        {
            get { return Dispatcher.Invoke(() => (double)GetValue(BDayIconWidthProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BDayIconWidthProperty, value); }); }
        }

        public double BackgroundOpacity
        {
            get { return Dispatcher.Invoke(() => (double)GetValue(BackgroundOpacityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BackgroundOpacityProperty, value); }); }
        }

        public Style TextStyle
        {
            get { return Dispatcher.Invoke(() => (Style)GetValue(TextStyleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(TextStyleProperty, value); }); }
        }
    }
}