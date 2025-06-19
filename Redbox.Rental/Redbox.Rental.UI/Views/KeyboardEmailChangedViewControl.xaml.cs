using System.Windows;
using System.Windows.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "KeyboardEmailChangeView")]
    public partial class KeyboardEmailChangedViewControl : KeyboardControl
    {
        public KeyboardEmailChangedViewControl()
        {
            KeyboardMode = KeyboardMode.ACCOUNT_EMAIL_CHANGED;
            InitializeComponent();
            KeyboardElem.KeyboardMode = KeyboardMode;
            InitKeyboardUserControl(KeyboardElem, EmailElem);
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            foreach (var control in FindLogicalChildren<Control>(MainControl))
                if (theme != null)
                    theme.SetStyle(control);
        }

        private void ContinueTouched(object sender, RoutedEventArgs e)
        {
            ContinueTouched();
        }

        private void EmailSelected(object sender, RoutedEventArgs e)
        {
            KeyTouched();
        }
    }
}