using System;
using System.Windows;

namespace Redbox.Rental.Model.Browse
{
    public class BrowseFilter : DependencyObject
    {
        public static readonly DependencyProperty IsButtonEnabledProperty =
            DependencyProperty.Register(nameof(IsButtonEnabled), typeof(bool), typeof(BrowseFilter),
                (PropertyMetadata)new FrameworkPropertyMetadata((object)false));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text),
            typeof(string), typeof(BrowseFilter),
            (PropertyMetadata)new FrameworkPropertyMetadata((PropertyChangedCallback)null));

        public static readonly DependencyProperty ButtonVisibilityProperty =
            DependencyProperty.Register(nameof(ButtonVisibility), typeof(Visibility), typeof(BrowseFilter),
                (PropertyMetadata)new FrameworkPropertyMetadata((object)Visibility.Visible));

        private bool _isSelected;

        public string Name { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        public object Parameter { get; set; }

        public bool IsVisible
        {
            get => ButtonVisibility == Visibility.Visible;
            set => ButtonVisibility = value ? Visibility.Visible : Visibility.Collapsed;
        }

        public string Text
        {
            get { return Dispatcher.Invoke<string>((Func<string>)(() => (string)GetValue(TextProperty))); }
            set { Dispatcher.Invoke((Action)(() => SetValue(TextProperty, (object)value))); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                if (_isSelected != IsButtonEnabled)
                    return;
                IsButtonEnabled = !_isSelected;
            }
        }

        public bool IsDeselectable { get; set; }

        public bool IsButtonEnabled
        {
            get { return Dispatcher.Invoke<bool>((Func<bool>)(() => (bool)GetValue(IsButtonEnabledProperty))); }
            set { Dispatcher.Invoke((Action)(() => SetValue(IsButtonEnabledProperty, (object)value))); }
        }

        public Visibility ButtonVisibility
        {
            get
            {
                return Dispatcher.Invoke<Visibility>((Func<Visibility>)(() =>
                    (Visibility)GetValue(ButtonVisibilityProperty)));
            }
            set { Dispatcher.Invoke((Action)(() => SetValue(ButtonVisibilityProperty, (object)value))); }
        }

        public BrowseFilterType ButtonType { get; set; }
    }
}