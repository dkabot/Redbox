using System;
using System.Windows;

namespace Redbox.Rental.UI.Models
{
    public class LoginModel : BaseModel<LoginModel>
    {
        public static readonly DependencyProperty TitleProperty = CreateDependencyProperty("Title", TYPES.STRING);

        public static readonly DependencyProperty StoreNumberProperty =
            CreateDependencyProperty("StoreNumber", TYPES.STRING);

        public static readonly DependencyProperty ErrorMessageProperty =
            CreateDependencyProperty("ErrorMessage", TYPES.STRING);

        public static readonly DependencyProperty EnableLoginProperty =
            CreateDependencyProperty("EnableLogin", TYPES.BOOL, true);

        public UserData UserData { get; set; }

        public Action CancelAction { get; set; }

        public Action ContinueAction { get; set; }

        public Action<UserData> AuthenticateAction { get; set; }

        public DynamicRoutedCommand ContinueButtonCommand { get; set; }

        public DynamicRoutedCommand CancelButtonCommand { get; set; }

        public string Title
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(TitleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(TitleProperty, value); }); }
        }

        public string ErrorMessage
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(ErrorMessageProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ErrorMessageProperty, value); }); }
        }

        public string StoreNumber
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(StoreNumberProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(StoreNumberProperty, value); }); }
        }

        public bool EnableLogin
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(EnableLoginProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(EnableLoginProperty, value); }); }
        }
    }
}