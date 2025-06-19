using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "TitleDetailsWatchOptionsPopupView")]
    public partial class TitleDetailsWatchOptionsPopupViewUserControl : TextToSpeechUserControl
    {
        public TitleDetailsWatchOptionsPopupViewUserControl()
        {
            InitializeComponent();
            ApplyTheme();
        }

        private TitleDetailsWatchOptionsPopupViewModel TitleDetailsWatchOptionsPopupViewModel =>
            DataContext as TitleDetailsWatchOptionsPopupViewModel;

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme != null) theme.SetStyle(SeeFullDetailsButton);
            if (theme == null) return;
            theme.SetStyle(CloseButton);
        }

        private void SeeFullDetailsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var titleDetailsWatchOptionsPopupViewModel = TitleDetailsWatchOptionsPopupViewModel;
            if (titleDetailsWatchOptionsPopupViewModel == null) return;
            titleDetailsWatchOptionsPopupViewModel.ProcessOnWatchOptionsSeeFullDetailsButtonClicked();
        }

        private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var titleDetailsWatchOptionsPopupViewModel = TitleDetailsWatchOptionsPopupViewModel;
            if (titleDetailsWatchOptionsPopupViewModel == null) return;
            titleDetailsWatchOptionsPopupViewModel.ProcessOnWatchOptionsCloseButtonClicked();
        }
    }
}