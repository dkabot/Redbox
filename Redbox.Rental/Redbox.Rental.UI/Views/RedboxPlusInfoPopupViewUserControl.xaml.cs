using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "RedboxPlusInfoPopupView")]
    public partial class RedboxPlusInfoPopupViewUserControl : TextToSpeechUserControl
    {
        public RedboxPlusInfoPopupViewUserControl()
        {
            InitializeComponent();
            ApplyTheme();
        }

        private RedboxPlusInfoPopupViewModel RedboxPlusInfoPopupViewModel =>
            DataContext as RedboxPlusInfoPopupViewModel;

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme == null) return;
            theme.SetStyle(CloseButton);
        }

        private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var redboxPlusInfoPopupViewModel = RedboxPlusInfoPopupViewModel;
            if (redboxPlusInfoPopupViewModel == null) return;
            redboxPlusInfoPopupViewModel.ProcessOnCloseButtonClicked();
        }
    }
}