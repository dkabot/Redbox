using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "SortSelectionPopupView")]
    public partial class SortSelectionPopupViewUserControl : TextToSpeechUserControl
    {
        public SortSelectionPopupViewUserControl()
        {
            InitializeComponent();
            ApplyTheme();
        }

        private SortSelectionPopupViewModel SortSelectionPopupViewModel => DataContext as SortSelectionPopupViewModel;

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme != null) theme.SetStyle(CancelButton);
            if (theme == null) return;
            theme.SetStyle(ApplyButton);
        }

        private void CancelCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var sortSelectionPopupViewModel = SortSelectionPopupViewModel;
            if (sortSelectionPopupViewModel == null) return;
            sortSelectionPopupViewModel.ProcessOnCancelButtonClicked();
        }

        private void ApplyCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var sortSelectionPopupViewModel = SortSelectionPopupViewModel;
            if (sortSelectionPopupViewModel == null) return;
            sortSelectionPopupViewModel.ProcessOnApplyButtonClicked();
        }
    }
}