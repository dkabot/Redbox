using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Redbox.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.UI.Controls
{
    public partial class KeyboardUserControl : BaseUserControl, IValueConverter
    {
        public static readonly DependencyProperty KeyboardModeProperty =
            CreateDependencyProperty<KeyboardUserControl>("KeyboardMode", typeof(KeyboardMode),
                KeyboardMode.NONE_OR_UNKNOWN);

        public KeyboardUserControl()
        {
            Resources.Add("KeyboardUserControl", this);
            InitializeComponent();
            AddClickHandlers();
        }

        public bool CapKeyState
        {
            get => CapButton.State;
            set => CapButton.State = value;
        }

        public KeyboardMode KeyboardMode
        {
            get => (KeyboardMode)GetValue(KeyboardModeProperty);
            set
            {
                SetValue(KeyboardModeProperty, value);
                InitVisibility(KeyboardMode);
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var flag = false;
            var keyboardMode = KeyboardMode;
            var text = (string)value;
            if (!(text == "SymbolKeysElem"))
            {
                if (!(text == "CaseKeysElem"))
                {
                    if (!(text == "EmailKeysElem"))
                    {
                        if (!(text == "CodeKeysElem"))
                        {
                            if (text == "PromoCodeElem") flag = keyboardMode == KeyboardMode.PROMOTIONS_CODE;
                        }
                        else
                        {
                            flag = keyboardMode == KeyboardMode.ACCOUNT_SIGN_IN ||
                                   keyboardMode == KeyboardMode.EMAIL_RECEIPTS_PROMO ||
                                   keyboardMode == KeyboardMode.ACCOUNT_EMAIL_CHANGED ||
                                   keyboardMode == KeyboardMode.EMAIL_COMING_SOON;
                        }
                    }
                    else
                    {
                        flag = keyboardMode == KeyboardMode.ACCOUNT_SIGN_IN ||
                               keyboardMode == KeyboardMode.EMAIL_RECEIPTS_PROMO ||
                               keyboardMode == KeyboardMode.ACCOUNT_EMAIL_CHANGED ||
                               keyboardMode == KeyboardMode.EMAIL_COMING_SOON;
                    }
                }
                else
                {
                    flag = keyboardMode == KeyboardMode.ACCOUNT_SIGN_IN ||
                           keyboardMode == KeyboardMode.EMAIL_RECEIPTS_PROMO ||
                           keyboardMode == KeyboardMode.ACCOUNT_EMAIL_CHANGED ||
                           keyboardMode == KeyboardMode.EMAIL_COMING_SOON;
                }
            }
            else
            {
                flag = false;
            }

            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        public event Action KeyTouched;

        public event Action<SymbolMode> KeyPressed;

        protected override void DataContextHasChanged()
        {
            if (DataContext is KeyboardModel)
            {
                var keyboardModel = DataContext as KeyboardModel;
                KeyState keyState;
                if (KeyboardMode == KeyboardMode.PROMOTIONS_CODE)
                {
                    keyState = KeyState.UPPER;
                }
                else
                {
                    CapKeyState = keyboardModel.KeyboardCapLock;
                    keyState = CapButton.State ? KeyState.UPPER : KeyState.LOWER;
                }

                ConvertKeyboardKeys(keyState);
            }
        }

        private void RoundedButton_Clicked(object sender, RoutedEventArgs e)
        {
            var keyboardModel = DataContext as KeyboardModel;
            var roundedButton = sender as RoundedButton;
            var content = roundedButton.Content;
            if (CapKeyState)
            {
                CapKeyState = false;
                ConvertKeyboardKeys(KeyState.LOWER);
            }

            if (keyboardModel != null && content != null)
            {
                var text = roundedButton.Tag as string;
                if (!(text == "key"))
                {
                    if (!(text == "back"))
                    {
                        if (!(text == "space"))
                        {
                            if (!(text == "clear"))
                            {
                                if (!(text == "domain"))
                                {
                                    if (text == "dotcom")
                                        if (keyboardModel.DotComButtonCommand.CanExecute(null))
                                            keyboardModel.DotComButtonCommand.Execute((content as TextBlock).Text);
                                }
                                else if (keyboardModel.DomainButtonCommand.CanExecute(null))
                                {
                                    keyboardModel.DomainButtonCommand.Execute((content as TextBlock).Text);
                                }
                            }
                            else if (keyboardModel.ClearButtonCommand.CanExecute(null))
                            {
                                keyboardModel.ClearButtonCommand.Execute(null);
                            }
                        }
                        else if (keyboardModel.SpaceButtonCommand.CanExecute(null))
                        {
                            keyboardModel.SpaceButtonCommand.Execute(null);
                        }
                    }
                    else if (keyboardModel.BackButtonCommand.CanExecute(null))
                    {
                        keyboardModel.BackButtonCommand.Execute(null);
                    }
                }
                else if (keyboardModel.KeyButtonCommand.CanExecute(null))
                {
                    keyboardModel.KeyButtonCommand.Execute(content as string);
                }

                var keyTouched = KeyTouched;
                if (keyTouched == null) return;
                keyTouched();
            }
        }

        private void CapButtonClicked(object sender, RoutedEventArgs e)
        {
            if ("ABC".Equals(CapButtonText.Text))
                CapKeyState = true;
            else
                CapKeyState = false;
            ConvertKeyboardKeys(CapButton.State ? KeyState.UPPER : KeyState.LOWER);
        }

        protected void AddClickHandlers()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            foreach (var roundedButton in FindLogicalChildren<RoundedButton>(MainGridElem))
            {
                if (!"cap_shift".Equals(roundedButton.Tag as string)) roundedButton.Click += RoundedButton_Clicked;
                if (theme != null) theme.SetStyle(roundedButton);
            }
        }

        protected void InitVisibility(KeyboardMode KeyboardMode)
        {
            switch (KeyboardMode)
            {
                case KeyboardMode.EMAIL_COMING_SOON:
                case KeyboardMode.EMAIL_RECEIPTS_PROMO:
                case KeyboardMode.ACCOUNT_EMAIL_CHANGED:
                case KeyboardMode.PERKS_SIGN_UP:
                case KeyboardMode.REDBOX_PLUS_SIGN_UP:
                case KeyboardMode.LEAD_GENERATION_SIGN_UP:
                    SymbolKeysElem.Visibility = Visibility.Collapsed;
                    CodeKeysElem.Visibility = Visibility.Collapsed;
                    EmailKeysElem.Visibility = Visibility.Visible;
                    PromoCodeElem.Visibility = Visibility.Collapsed;
                    CaseKeysElem.Visibility = Visibility.Visible;
                    CaseKeysElem.Margin = new Thickness(0.0, 0.0, 730.0, 0.0);
                    break;
                case KeyboardMode.EMAIL_SYMBOLS:
                    SymbolButtonTextElem.Text =
                        CapKeyState ? CapButtonText.Text.ToUpper() : CapButtonText.Text.ToLower();
                    SymbolKeysElem.Visibility = Visibility.Visible;
                    CodeKeysElem.Visibility = Visibility.Collapsed;
                    EmailKeysElem.Visibility = Visibility.Collapsed;
                    PromoCodeElem.Visibility = Visibility.Collapsed;
                    CaseKeysElem.Visibility = Visibility.Collapsed;
                    return;
                case KeyboardMode.PROMOTIONS_CODE:
                    SymbolKeysElem.Visibility = Visibility.Collapsed;
                    CodeKeysElem.Visibility = Visibility.Collapsed;
                    EmailKeysElem.Visibility = Visibility.Collapsed;
                    PromoCodeElem.Visibility = Visibility.Visible;
                    CaseKeysElem.Visibility = Visibility.Collapsed;
                    return;
                case KeyboardMode.ACCOUNT_SIGN_IN:
                    SymbolKeysElem.Visibility = Visibility.Collapsed;
                    CodeKeysElem.Visibility = Visibility.Visible;
                    EmailKeysElem.Visibility = Visibility.Collapsed;
                    PromoCodeElem.Visibility = Visibility.Collapsed;
                    CaseKeysElem.Visibility = Visibility.Visible;
                    CaseKeysElem.Margin = new Thickness(75.0, 0.0, 650.0, 0.0);
                    return;
                case KeyboardMode.STARZ_SIGN_UP:
                    break;
                default:
                    return;
            }
        }

        protected void ConvertKeyboardKeys(KeyState keyState)
        {
            ConvertKeyboardKeyGroup(QCodeElements, keyState);
            ConvertKeyboardKeyGroup(ACodeElements, keyState);
            ConvertKeyboardKeyGroup(ZCodeElements, keyState);
            ConvertKeyboardKeyGroup(QEmailElements, keyState);
            ConvertKeyboardKeyGroup(AEmailElements, keyState);
            ConvertKeyboardKeyGroup(AEmailElements2, keyState);
            ConvertKeyboardKeyGroup(ZEmailElements, keyState);
            ConvertKeyboardKeyGroup(ZEmailElements2, keyState);
            var text = CapButtonText.Text;
            if (keyState == KeyState.LOWER)
                CapButtonText.Text = text.ToUpper();
            else
                CapButtonText.Text = text.ToLower();
            HandleWPFHit();
        }

        protected void ConvertKeyboardKeyGroup(Grid grid, KeyState keyState)
        {
            foreach (var obj in grid.Children)
            {
                var button = obj as Button;
                var text = (button != null ? button.Content : null) as string;
                if (text != null && text.Length == 1)
                {
                    var num = (short)text[0];
                    if (keyState == KeyState.LOWER)
                    {
                        if (num >= 65 && num <= 90) (obj as Button).Content = text.ToLower();
                    }
                    else if (num >= 97 && num <= 122)
                    {
                        (obj as Button).Content = text.ToUpper();
                    }
                }
            }
        }

        private void SwitchToLettersPressed(object sender, RoutedEventArgs e)
        {
            var keyPressed = KeyPressed;
            if (keyPressed == null) return;
            keyPressed(SymbolMode.EMAIL_LETTER_KEY_PRESSED);
        }

        private void CodeSwitchToSymbolsPressed(object sender, RoutedEventArgs e)
        {
            var keyPressed = KeyPressed;
            if (keyPressed == null) return;
            keyPressed(SymbolMode.CODE__SYMBOL_KEY_PRESSED);
        }

        private void EmailSwitchToSymbolsPressed(object sender, RoutedEventArgs e)
        {
            var keyPressed = KeyPressed;
            if (keyPressed == null) return;
            keyPressed(SymbolMode.EMAIL_SYMBOL_KEY_PRESSED);
        }

        private void RoundedButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}