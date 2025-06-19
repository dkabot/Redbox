using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "TitleDetailsFullDetailsPopupView")]
    public partial class TitleDetailsFullDetailsPopupViewUserControl : TextToSpeechUserControl
    {
        public TitleDetailsFullDetailsPopupViewUserControl()
        {
            InitializeComponent();
            ApplyTheme();
        }

        private TitleDetailsFullDetailsPopupViewModel TitleDetailsFullDetailsPopupViewModel =>
            DataContext as TitleDetailsFullDetailsPopupViewModel;

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme == null) return;
            theme.SetStyle(CloseButton);
        }

        private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var titleDetailsFullDetailsPopupViewModel = TitleDetailsFullDetailsPopupViewModel;
            if (titleDetailsFullDetailsPopupViewModel == null) return;
            titleDetailsFullDetailsPopupViewModel.ProcessOnFullDetailsCloseButtonClicked();
        }
    }
}