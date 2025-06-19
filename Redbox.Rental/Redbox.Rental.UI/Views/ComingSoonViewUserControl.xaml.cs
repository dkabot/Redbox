using System.Windows;
using System.Windows.Controls;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.UI.Controls;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "ComingSoonView")]
    public partial class ComingSoonViewUserControl : UserControl
    {
        public ComingSoonViewUserControl()
        {
            InitializeComponent();
        }

        public string ComingSoonText
        {
            get => TextBlockComingSoon.Text;
            set => TextBlockComingSoon.Text = value;
        }

        public string RentalsAreOnTheirWayText
        {
            get => TextBlockRentalsComingSoon.Text;
            set => TextBlockRentalsComingSoon.Text = value;
        }

        public string ComingSoonTextStyle
        {
            set => SetStyle(TextBlockComingSoon, value);
        }

        public string RentalsAreOnTheirWayStyle
        {
            set => SetStyle(TextBlockRentalsComingSoon, value);
        }

        private void SetStyle(FrameworkElement frameworkElement, string styleName)
        {
            var obj = FindResource(styleName);
            if (obj != null && frameworkElement != null)
            {
                var style = obj as Style;
                if (style != null) frameworkElement.Style = style;
            }
        }

        public void AddComingSoonTitle(IComingSoonTitle comingSoonTitle)
        {
            var comingSoonVendUserControl = new ComingSoonVendUserControl
            {
                DataContext = comingSoonTitle,
                Margin = new Thickness(52.5, 0.0, 52.5, 0.0)
            };
            ComingSoonPanel.Children.Add(comingSoonVendUserControl);
        }
    }
}