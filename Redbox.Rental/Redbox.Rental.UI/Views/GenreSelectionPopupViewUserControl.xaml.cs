using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "GenreSelectionPopupView")]
    public partial class GenreSelectionPopupViewUserControl : TextToSpeechUserControl
    {
        public GenreSelectionPopupViewUserControl()
        {
            InitializeComponent();
            ApplyTheme();
        }

        private GenreSelectionPopupViewModel GenreSelectionPopupViewModel =>
            DataContext as GenreSelectionPopupViewModel;

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
            var genreSelectionPopupViewModel = GenreSelectionPopupViewModel;
            if (genreSelectionPopupViewModel == null) return;
            genreSelectionPopupViewModel.ProcessOnCancelButtonClicked();
        }

        private void ApplyCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var genreSelectionPopupViewModel = GenreSelectionPopupViewModel;
            if (genreSelectionPopupViewModel == null) return;
            genreSelectionPopupViewModel.ProcessOnApplyButtonClicked();
        }
    }
}