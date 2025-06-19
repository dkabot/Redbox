using System.Windows;
using System.Windows.Controls;

namespace Redbox.Rental.UI.Views
{
    public partial class MarketingScreenSaverUserControl : UserControl
    {
        public static readonly DependencyProperty IsAnimationOnProperty = DependencyProperty.Register("IsAnimationOn",
            typeof(bool), typeof(MarketingScreenSaverUserControl), new PropertyMetadata(false));

        public MarketingScreenSaverUserControl()
        {
            InitializeComponent();
        }

        public bool IsAnimationOn
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(IsAnimationOnProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(IsAnimationOnProperty, value); }); }
        }
    }
}