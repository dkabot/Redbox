using System;
using System.Windows;

namespace Redbox.Rental.UI.Models
{
    public class ConfirmEmailModel : AbstractViewModel
    {
        public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register("HeaderText",
            typeof(string), typeof(ConfirmEmailModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty BodyTextProperty = DependencyProperty.Register("BodyText",
            typeof(string), typeof(ConfirmEmailModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty OptionalBodyTextProperty =
            DependencyProperty.Register("OptionalBodyText", typeof(string), typeof(ConfirmEmailModel),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty OptionalBodyTextVisibilityProperty =
            DependencyProperty.Register("OptionalBodyTextVisibility", typeof(Visibility), typeof(ConfirmEmailModel),
                new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty Button1VisibilityProperty =
            DependencyProperty.Register("Button1Visibility", typeof(Visibility), typeof(ConfirmEmailModel),
                new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty Button2VisibilityProperty =
            DependencyProperty.Register("Button2Visibility", typeof(Visibility), typeof(ConfirmEmailModel),
                new FrameworkPropertyMetadata(Visibility.Collapsed));

        public bool HasActionExecuted { get; set; }

        public string HeaderText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(HeaderTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(HeaderTextProperty, value); }); }
        }

        public string BodyText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(BodyTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BodyTextProperty, value); }); }
        }

        public string OptionalBodyText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(OptionalBodyTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(OptionalBodyTextProperty, value); }); }
        }

        public Visibility OptionalBodyTextVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(OptionalBodyTextVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(OptionalBodyTextVisibilityProperty, value); }); }
        }

        public Visibility Button1Visibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(Button1VisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(Button1VisibilityProperty, value); }); }
        }

        public Visibility Button2Visibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(Button2VisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(Button2VisibilityProperty, value); }); }
        }

        public event Action Button1Action;

        public event Action Button2Action;

        public event Action TimeoutAction;

        public void ProcessButton1Action()
        {
            var button1Action = Button1Action;
            if (button1Action == null) return;
            button1Action();
        }

        public void ProcessButton2Action()
        {
            var button2Action = Button2Action;
            if (button2Action == null) return;
            button2Action();
        }

        public void ProcessTimeoutAction()
        {
            var timeoutAction = TimeoutAction;
            if (timeoutAction == null) return;
            timeoutAction();
        }
    }
}