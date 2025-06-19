using System;
using System.Security;
using System.Windows.Input;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.UI.ControllersLogic
{
    public class KeyboardLogic : BaseLogic
    {
        public KeyboardLogic(KeyboardModel model)
        {
            Model = model;
            Model.KeyboardError = "";
            Model.KeyButtonCommand = RegisterRoutedCommand(Commands.KeyButtonCommand,
                new Action<string>(KeyButtonCommand_Execute), null);
            Model.BackButtonCommand = RegisterRoutedCommand(Commands.BackButtonCommand, BackButtonCommand_Execute);
            Model.ClearButtonCommand = RegisterRoutedCommand(Commands.ClearButtonCommand, ClearButtonCommand_Execute);
            Model.DomainButtonCommand = RegisterRoutedCommand(Commands.DomainButtonCommand,
                new Action<string>(DomainButtonCommand_Execute), null);
            Model.DotComButtonCommand = RegisterRoutedCommand(Commands.DotComButtonCommand,
                new Action<string>(DotComButtonCommand_Execute), null);
            Model.SpaceButtonCommand = RegisterRoutedCommand(Commands.SpaceButtonCommand, SpaceButtonCommand_Execute);
            Model.ForgotPasswordButtonCommand = RegisterRoutedCommand(Commands.ForgotPasswordButtonCommand,
                ForgotPasswordButtonCommand_Execute);
            Model.CancelButtonCommand =
                RegisterRoutedCommand(Commands.CancelButtonCommand, CancelButtonCommand_Execute);
            Model.ContinueButtonCommand = RegisterRoutedCommand(Commands.ContinueButtonCommand,
                ContinueButtonCommand_Execute, ContinueButtonCommand_canExecute);
            Model.EmailSelectedCommand =
                RegisterRoutedCommand<bool>(Commands.EmailSelectedCommand, EmailSelectedCommand_Execute);
        }

        public KeyboardModel Model { get; set; }

        public KeyboardInfo KeyboardInfo { get; set; }

        public bool Instructions { get; set; }

        public int KeyboardTextMaxLength { get; set; }

        public Action ForgotPasswordAction { get; set; }

        public Action CancelAction { get; set; }

        public Action<string> ContinueAction { get; set; }

        public Action<string, SecureString> ContinueCodeAction { get; set; }

        public Predicate<string> IsValidEntry { get; set; }

        public Func<string, string, bool> IsValidEntries { get; set; }

        public static Action StopIdleTimerAction { get; set; }

        public KeyboardMode KeyboardMode { get; set; }

        public bool InitControlTextMode()
        {
            var keyEmailMode = Model.KeyEmailMode;
            var setControlTextMode = Model.SetControlTextMode;
            if (setControlTextMode != null) setControlTextMode(keyEmailMode);
            return keyEmailMode;
        }

        public void SetControlTextMode(bool mode)
        {
            var setControlTextMode = Model.SetControlTextMode;
            if (setControlTextMode == null) return;
            setControlTextMode(mode);
        }

        public bool GetControlTextMode()
        {
            return Model.KeyEmailMode;
        }

        private string GetErrorMessage()
        {
            SetCurrentUICulture();
            switch (KeyboardInfo.KeyboardMode)
            {
                case KeyboardMode.EMAIL_COMING_SOON:
                case KeyboardMode.ACCOUNT_SIGN_IN:
                case KeyboardMode.PERKS_SIGN_UP:
                case KeyboardMode.REDBOX_PLUS_SIGN_UP:
                case KeyboardMode.LEAD_GENERATION_SIGN_UP:
                    return Resources.email_error_message;
                case KeyboardMode.EMAIL_RECEIPTS_PROMO:
                    return Resources.receipt_email_error;
                case KeyboardMode.PROMOTIONS_CODE:
                    return Resources.promo_code_error_message;
            }

            return "";
        }

        public void CancelButton_Execute()
        {
            if (CancelAction != null)
            {
                CancelAction();
                Model.KeyboardText = "";
            }
        }

        public bool ValidateKeyboardText()
        {
            Model.KeyboardText = Model.KeyboardText ?? "";
            Model.KeyboardCode = Model.KeyboardCode ?? "";
            Model.KeyboardError = "";
            var keyboardMode = KeyboardMode;
            bool flag;
            if (keyboardMode != KeyboardMode.ACCOUNT_SIGN_IN)
            {
                if (keyboardMode != KeyboardMode.ACCOUNT_EMAIL_CHANGED)
                {
                    flag = IsValidEntry != null && IsValidEntry(Model.KeyboardText);
                    if (!flag) Model.KeyboardError = GetErrorMessage();
                }
                else
                {
                    flag = IsValidEntries != null && IsValidEntries(Model.KeyboardText, Model.KeyboardCode);
                    if (!flag) Model.KeyboardError = GetErrorMessage();
                }
            }
            else
            {
                flag = IsValidEntries != null && IsValidEntries(Model.KeyboardText, Model.KeyboardCode);
                if (!flag) Model.KeyboardError = GetErrorMessage();
            }

            return flag;
        }

        public string ContinueButtonExecute()
        {
            if (ValidateKeyboardText())
            {
                if (KeyboardMode == KeyboardMode.ACCOUNT_SIGN_IN)
                {
                    if (ContinueCodeAction != null)
                    {
                        var stopIdleTimerAction = StopIdleTimerAction;
                        if (stopIdleTimerAction != null) stopIdleTimerAction();
                        ContinueCodeAction(Model.KeyboardText, CreateSecureString(Model.KeyboardCode));
                        Model.KeyboardCode = string.Empty;
                        Model.KeyEmailMode = false;
                    }
                }
                else if (ContinueAction != null)
                {
                    var stopIdleTimerAction2 = StopIdleTimerAction;
                    if (stopIdleTimerAction2 != null) stopIdleTimerAction2();
                    ContinueAction(Model.KeyboardText);
                }
            }

            return Model.KeyboardError;
        }

        private void UpdateContinue(bool clearError = true)
        {
            if (Model.KeyEmailMode)
            {
                var keyboardText = Model.KeyboardText;
                if ((keyboardText != null ? keyboardText.Length : 0) > KeyboardTextMaxLength)
                    Model.KeyboardText = Model.KeyboardText.Remove(KeyboardTextMaxLength);
            }
            else
            {
                var keyboardCode = Model.KeyboardCode;
                if ((keyboardCode != null ? keyboardCode.Length : 0) > KeyboardTextMaxLength)
                    Model.KeyboardCode = Model.KeyboardCode.Remove(KeyboardTextMaxLength);
            }

            if (clearError) Model.KeyboardError = string.Empty;
            HandleWPFHit();
            CommandManager.InvalidateRequerySuggested();
            Model.ContinueButtonEnabled = Model.ContinueButtonCommand.CanExecute(null);
        }

        public void KeyButtonCommand_Execute(string text)
        {
            if (Model.KeyEmailMode)
            {
                var model = Model;
                model.KeyboardText += text;
            }
            else
            {
                var model2 = Model;
                model2.KeyboardCode += text;
            }

            UpdateContinue();
        }

        public void BackButtonCommand_Execute()
        {
            if (Model.KeyboardText != null)
            {
                if (Model.KeyEmailMode)
                {
                    var num = Model.KeyboardText.Length - 1;
                    if (num > -1) Model.KeyboardText = Model.KeyboardText.Remove(num);
                }
                else
                {
                    var num = Model.KeyboardCode.Length - 1;
                    if (num > -1) Model.KeyboardCode = Model.KeyboardCode.Remove(num);
                }

                UpdateContinue();
            }
        }

        public void ClearButtonCommand_Execute()
        {
            if (Model.KeyEmailMode)
                Model.KeyboardText = string.Empty;
            else
                Model.KeyboardCode = string.Empty;
            UpdateContinue();
        }

        public void DomainButtonCommand_Execute(string text)
        {
            if (Model.KeyboardText != null)
            {
                var num = Model.KeyboardText.IndexOf('@');
                if (num > -1) Model.KeyboardText = Model.KeyboardText.Remove(num);
                if (Model.KeyboardText.Length > 0)
                {
                    var model = Model;
                    model.KeyboardText += text;
                    SetControlTextMode(false);
                }

                UpdateContinue();
            }
        }

        public void DotComButtonCommand_Execute(string text)
        {
            if (Model.KeyboardText != null)
            {
                SetCurrentUICulture();
                var num = Model.KeyboardText.IndexOf('@');
                if (num == -1) Model.KeyboardError = Resources.email_error_message;
                var text2 = Model.KeyboardText.Substring(num + 1);
                if (text2.Length == 0)
                {
                    Model.KeyboardError = Resources.email_error_message;
                }
                else
                {
                    var num2 = text2.IndexOf('.') + 1;
                    if (num2 > 0) Model.KeyboardText = Model.KeyboardText.Remove(num + num2);
                    var model = Model;
                    model.KeyboardText += text;
                    SetControlTextMode(false);
                }

                UpdateContinue();
            }
        }

        public void SpaceButtonCommand_Execute()
        {
            if (!Model.KeyEmailMode)
            {
                var model = Model;
                model.KeyboardCode += " ";
            }

            UpdateContinue();
        }

        public void ForgotPasswordButtonCommand_Execute()
        {
            var analyticsService = AnalyticsService;
            if (analyticsService != null) analyticsService.AddButtonPressEvent("KeyboardLogicSignInForgotPassword");
            var forgotPasswordAction = ForgotPasswordAction;
            if (forgotPasswordAction != null) forgotPasswordAction();
            HandleWPFHit();
        }

        public void CancelButtonCommand_Execute()
        {
            var analyticsService = AnalyticsService;
            if (analyticsService != null) analyticsService.AddButtonPressEvent("KeyboardLogicStartBrowsing");
            CancelButton_Execute();
            HandleWPFHit();
        }

        public bool ContinueButtonCommand_canExecute()
        {
            return KeyboardMode != KeyboardMode.ACCOUNT_SIGN_IN || (!string.IsNullOrWhiteSpace(Model.KeyboardText) &&
                                                                    !string.IsNullOrEmpty(Model.KeyboardCode));
        }

        public void ContinueButtonCommand_Execute()
        {
            var analyticsService = AnalyticsService;
            if (analyticsService != null) analyticsService.AddButtonPressEvent("KeyboardLogicStartBrowsing");
            Model.KeyboardError = ContinueButtonExecute();
            if (!string.IsNullOrEmpty(Model.KeyboardError))
            {
                UpdateContinue(false);
                return;
            }

            if (KeyboardMode != KeyboardMode.ACCOUNT_SIGN_IN && KeyboardMode != KeyboardMode.PROMOTIONS_CODE)
                Model.KeyboardText = "";
        }

        public void EmailSelectedCommand_Execute(bool emailMode)
        {
            Model.KeyEmailMode = emailMode;
        }
    }
}