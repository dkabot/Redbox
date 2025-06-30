using System.Windows;
using System.Windows.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "KeyboardEmailReceiptView")]
    public partial class KeyboardEmailReceiptViewControl : KeyboardControl
    {
        public KeyboardEmailReceiptViewControl()
        {
            KeyboardMode = KeyboardMode.EMAIL_RECEIPTS_PROMO;
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

        private void MainControl_Loaded(object sender, RoutedEventArgs e)
        {
            // The way this is called properly:
            //if (Model.ContinueButtonCommand.CanExecute(null)) Model.ContinueButtonCommand.Execute(null);
            if (Model.CancelButtonCommand.CanExecute(null)) { Model.CancelButtonCommand.Execute(null); };
        }
    }
}