using System;
using System.Windows;
using Redbox.Rental.UI.ControllersLogic;

namespace Redbox.Rental.UI.Models
{
    public class KeyboardModel : BaseModel<KeyboardModel>
    {
        public static readonly DependencyProperty KeyboardTextProperty =
            CreateDependencyProperty("KeyboardText", TYPES.STRING, string.Empty);

        public static readonly DependencyProperty KeyboardCodeProperty =
            CreateDependencyProperty("KeyboardCode", TYPES.STRING, string.Empty);

        public static readonly DependencyProperty KeyboardStarsProperty =
            CreateDependencyProperty("KeyboardStars", TYPES.STRING, string.Empty);

        public static readonly DependencyProperty KeyboardErrorProperty =
            CreateDependencyProperty("KeyboardError", TYPES.STRING);

        public static readonly DependencyProperty TitleTextProperty =
            CreateDependencyProperty("TitleText", TYPES.STRING);

        public static readonly DependencyProperty MessageTextProperty =
            CreateDependencyProperty("MessageText", TYPES.STRING);

        public static readonly DependencyProperty MarketingTextProperty =
            CreateDependencyProperty("MarketingText", TYPES.STRING);

        public static readonly DependencyProperty EmailPromptTextProperty =
            CreateDependencyProperty("EmailPromptText", TYPES.STRING);

        public static readonly DependencyProperty CodePromptTextProperty =
            CreateDependencyProperty("CodePromptText", TYPES.STRING);

        public static readonly DependencyProperty BackButtonTextProperty =
            CreateDependencyProperty("BackButtonText", TYPES.STRING);

        public static readonly DependencyProperty SpaceButtonTextProperty =
            CreateDependencyProperty("SpaceButtonText", TYPES.STRING);

        public static readonly DependencyProperty ClearButtonTextProperty =
            CreateDependencyProperty("ClearButtonText", TYPES.STRING);

        public static readonly DependencyProperty CancelButtonTextProperty =
            CreateDependencyProperty("CancelButtonText", TYPES.STRING);

        public static readonly DependencyProperty ContinueButtonTextProperty =
            CreateDependencyProperty("ContinueButtonText", TYPES.STRING);

        public static readonly DependencyProperty ContinueButtonEnabledProperty =
            CreateDependencyProperty("ContinueButtonEnabled", TYPES.BOOL, false);

        public static readonly DependencyProperty ForgotPasswordButtonTextProperty =
            CreateDependencyProperty("ForgotPasswordButtonText", TYPES.STRING);

        public static readonly DependencyProperty EmailButtonTextProperty =
            CreateDependencyProperty("EmailButtonText", TYPES.STRING);

        public static readonly DependencyProperty PasswordButtonTextProperty =
            CreateDependencyProperty("PasswordButtonText", TYPES.STRING);

        public static readonly DependencyProperty KeyboardCapLockProperty =
            CreateDependencyProperty("KeyboardCapLock", TYPES.BOOL, true);

        public static readonly DependencyProperty KeyEmailModeProperty =
            CreateDependencyProperty("KeyEmailMode", TYPES.BOOL, false);

        public static readonly DependencyProperty EmailChangeTextSizeProperty =
            CreateDependencyProperty("MessageTextSize", TYPES.INT, 24);

        public static readonly DependencyProperty SymbolsButtonTextProperty =
            CreateDependencyProperty("SymbolsButtonText", TYPES.STRING);

        public KeyboardLogic Logic { get; set; }

        public Action<bool> SetControlTextMode { get; set; }

        public DynamicRoutedCommand KeyButtonCommand { get; set; }

        public DynamicRoutedCommand CapButtonCommand { get; set; }

        public DynamicRoutedCommand ShiftButtonCommand { get; set; }

        public DynamicRoutedCommand BackButtonCommand { get; set; }

        public DynamicRoutedCommand ClearButtonCommand { get; set; }

        public DynamicRoutedCommand DomainButtonCommand { get; set; }

        public DynamicRoutedCommand DotComButtonCommand { get; set; }

        public DynamicRoutedCommand SpaceButtonCommand { get; set; }

        public DynamicRoutedCommand ForgotPasswordButtonCommand { get; set; }

        public DynamicRoutedCommand CancelButtonCommand { get; set; }

        public DynamicRoutedCommand ContinueButtonCommand { get; set; }

        public DynamicRoutedCommand TermsButtonCommand { get; set; }

        public DynamicRoutedCommand EmailSelectedCommand { get; set; }

        public DynamicRoutedCommand CodeSelectedCommand { get; set; }

        public string KeyboardText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(KeyboardTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(KeyboardTextProperty, value); }); }
        }

        public string KeyboardCode
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(KeyboardCodeProperty)); }
            set
            {
                var text = string.Empty;
                if (value != null)
                {
                    var num = value.Length - 1;
                    for (var i = 0; i < value.Length; i++)
                        if (i == num)
                            text += value[num].ToString();
                        else
                            text += "*";
                }

                KeyboardStars = text;
                Dispatcher.Invoke(delegate { SetValue(KeyboardCodeProperty, value); });
            }
        }

        public string KeyboardStars
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(KeyboardStarsProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(KeyboardStarsProperty, value); }); }
        }

        public string KeyboardError
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(KeyboardErrorProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(KeyboardErrorProperty, value); }); }
        }

        public string TitleText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(TitleTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(TitleTextProperty, value); }); }
        }

        public string MessageText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(MessageTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(MessageTextProperty, value); }); }
        }

        public string MarketingText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(MarketingTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(MarketingTextProperty, value); }); }
        }

        public string EmailPromptText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(EmailPromptTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(EmailPromptTextProperty, value); }); }
        }

        public string CodePromptText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(CodePromptTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CodePromptTextProperty, value); }); }
        }

        public string CancelButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(CancelButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CancelButtonTextProperty, value); }); }
        }

        public string ContinueButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(ContinueButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ContinueButtonTextProperty, value); }); }
        }

        public bool ContinueButtonEnabled
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(ContinueButtonEnabledProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ContinueButtonEnabledProperty, value); }); }
        }

        public string BackButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(BackButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BackButtonTextProperty, value); }); }
        }

        public string SpaceButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(SpaceButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(SpaceButtonTextProperty, value); }); }
        }

        public string ClearButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(ClearButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ClearButtonTextProperty, value); }); }
        }

        public string ForgotPasswordButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(ForgotPasswordButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ForgotPasswordButtonTextProperty, value); }); }
        }

        public string EmailButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(EmailButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(EmailButtonTextProperty, value); }); }
        }

        public string PasswordButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PasswordButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PasswordButtonTextProperty, value); }); }
        }

        public bool KeyboardCapLock
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(KeyboardCapLockProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(KeyboardCapLockProperty, value); }); }
        }

        public bool KeyEmailMode
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(KeyEmailModeProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(KeyEmailModeProperty, value); }); }
        }

        public int MessageTextSize
        {
            get { return Dispatcher.Invoke(() => (int)GetValue(EmailChangeTextSizeProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(EmailChangeTextSizeProperty, value); }); }
        }

        public string SymbolsButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(SymbolsButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(SymbolsButtonTextProperty, value); }); }
        }
    }
}