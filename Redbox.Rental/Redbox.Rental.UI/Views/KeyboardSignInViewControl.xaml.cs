using System.Windows;
using System.Windows.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "KeyboardSignInView")]
    public partial class KeyboardSignInViewControl : KeyboardControl
    {
        public KeyboardSignInViewControl()
        {
            KeyboardMode = KeyboardMode.ACCOUNT_SIGN_IN;
            InitializeComponent();
            InitKeyboardUserControl(KeyboardElem, EmailElem);
            ApplyTheme();
        }

        protected override void DataContextHasChanged()
        {
            var visibility = IsABEOn ? Visibility.Visible : Visibility.Collapsed;
            EmailButtonElem.Visibility = visibility;
            CodeButtonElem.Visibility = visibility;
            if (Model != null)
            {
                Model.SetControlTextMode = SetControlTextMode;
                SetControlTextMode(Model.KeyEmailMode);
            }

            UpdateKeyboardControl();
        }

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            foreach (var control in FindLogicalChildren<Control>(MainControl))
                if (theme != null)
                    theme.SetStyle(control);
        }

        public void UpdateKeyStates()
        {
            KeyboardElem.CapKeyState = Model.KeyboardCapLock;
            UpdateKeyboardControl();
        }

        private void UpdateTextInputs(bool mode)
        {
            if (mode)
            {
                KeyboardElem.KeyboardMode = KeyboardMode.EMAIL_COMING_SOON;
                return;
            }

            KeyboardElem.KeyboardMode = KeyboardMode.ACCOUNT_SIGN_IN;
        }

        private void UpdateKeyboardControl()
        {
            var flag = Model == null || Model.KeyEmailMode;
            UpdateTextInputs(flag);
        }

        protected override void KeyTouched()
        {
            if (EmailElem.ButtonState != 0) EmailElem.ButtonState = CodeElem.ButtonState = 0;
            Model.ContinueButtonEnabled = Model.ContinueButtonCommand.CanExecute(null);
        }

        protected override void KeyPressed(SymbolMode mode)
        {
            var lastKeyMode = LastKeyMode;
            LastKeyMode = KeyboardElem.KeyboardMode;
            if (mode <= SymbolMode.CODE__LETTER_KEY_PRESSED)
            {
                KeyboardElem.KeyboardMode = lastKeyMode;
                return;
            }

            if (mode - SymbolMode.EMAIL_SYMBOL_KEY_PRESSED > 1) return;
            KeyboardElem.KeyboardMode = KeyboardMode.EMAIL_SYMBOLS;
        }

        protected override void ContinueTouched()
        {
            Model.ContinueButtonEnabled = Model.ContinueButtonCommand.CanExecute(null);
            if (Model.ContinueButtonEnabled) Model.ContinueButtonCommand.Execute(null);
            var model = Model;
            if (!string.IsNullOrEmpty(model != null ? model.KeyboardError : null))
                EmailElem.ButtonState = CodeElem.ButtonState = 1;
        }

        private void NotifyUpdateTextInputs(bool state)
        {
            if (Model != null && Model.EmailSelectedCommand.CanExecute(null)) Model.EmailSelectedCommand.Execute(state);
            UpdateKeyboardControl();
        }

        private void SetControlTextMode(bool mode)
        {
            if (mode != Model.KeyEmailMode) NotifyUpdateTextInputs(mode);
            (mode ? EmailElem : CodeElem).IsSelected = true;
            KeyTouched();
        }

        private void ForgotPasswordClicked(object sender, RoutedEventArgs e)
        {
            var model = Model;
            if (model == null) return;
            var forgotPasswordButtonCommand = model.ForgotPasswordButtonCommand;
            if (forgotPasswordButtonCommand == null) return;
            forgotPasswordButtonCommand.Execute(null);
        }

        private void EmailSelected(object sender, RoutedEventArgs e)
        {
            SetControlTextMode(true);
        }

        private void CodeSelected(object sender, RoutedEventArgs e)
        {
            SetControlTextMode(false);
        }

        private void ContinueTouched(object sender, RoutedEventArgs e)
        {
            ContinueTouched();
        }
    }
}