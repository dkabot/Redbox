using System.Windows;
using System.Windows.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.UI.Controls
{
    public partial class TitleDetailDisplayProductWithMoreLikeThisUserControl : UserControl
    {
        public TitleDetailDisplayProductWithMoreLikeThisUserControl()
        {
            InitializeComponent();
            Loaded += TitleDetailDisplayProductWithMoreLikeThisUserControl_Loaded;
        }

        private void TitleDetailDisplayProductWithMoreLikeThisUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme != null) theme.SetStyle(go_back_button);
            if (theme != null) theme.SetStyle(see_full_details_button);
            if (theme != null) theme.SetStyle(buy_button);
            for (var i = 0; i < FormatButtonsItemsControl.Items.Count; i++)
            {
                var frameworkElement =
                    (FrameworkElement)FormatButtonsItemsControl.ItemContainerGenerator.ContainerFromIndex(i);
                if (frameworkElement != null && theme != null) theme.SetStyle(frameworkElement);
            }
        }
    }
}