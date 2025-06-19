using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "FormatSelectionPopupView")]
    public partial class FormatSelectionPopupUserControl : TextToSpeechUserControl
    {
        public FormatSelectionPopupUserControl()
        {
            InitializeComponent();
            Loaded += FormatSelectionPopupUserControl_Loaded;
        }

        private FormatSelectionPopupViewModel FormatSelectionPopupViewModel =>
            DataContext as FormatSelectionPopupViewModel;

        private void FormatSelectionPopupUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            for (var i = 0; i < FormatButtonsItemsControl.Items.Count; i++)
            {
                var frameworkElement =
                    (FrameworkElement)FormatButtonsItemsControl.ItemContainerGenerator.ContainerFromIndex(i);
                if (frameworkElement != null && theme != null) theme.SetStyle(frameworkElement);
            }

            if (theme != null) theme.SetStyle(PurchaseButton);
            if (theme != null) theme.SetStyle(CancelButton);
            if (theme != null) theme.SetStyle(DigitalCodeInfoButton);
        }

        private void FormatCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var formatSelectionPopupViewModel = FormatSelectionPopupViewModel;
            if (formatSelectionPopupViewModel == null) return;
            formatSelectionPopupViewModel.ProcessFormatButtonClicked((TitleType)((Control)e.OriginalSource).Tag);
        }

        private void PurchaseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var formatSelectionPopupViewModel = FormatSelectionPopupViewModel;
            if (formatSelectionPopupViewModel == null) return;
            formatSelectionPopupViewModel.ProcessPurchaseButtonClicked();
        }

        private void CancelCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var formatSelectionPopupViewModel = FormatSelectionPopupViewModel;
            if (formatSelectionPopupViewModel == null) return;
            formatSelectionPopupViewModel.ProcessCancelButtonClicked();
        }

        private void DigitalCodeInfoCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var formatSelectionPopupViewModel = FormatSelectionPopupViewModel;
            if (formatSelectionPopupViewModel == null) return;
            formatSelectionPopupViewModel.ProcessDigitalCodeInfoButtonClicked();
        }

        public override ISpeechControl GetSpeechControl()
        {
            var formatSelectionPopupViewModel = FormatSelectionPopupViewModel;
            if (formatSelectionPopupViewModel == null) return null;
            var onGetSpeechControl = formatSelectionPopupViewModel.OnGetSpeechControl;
            if (onGetSpeechControl == null) return null;
            return onGetSpeechControl();
        }
    }
}