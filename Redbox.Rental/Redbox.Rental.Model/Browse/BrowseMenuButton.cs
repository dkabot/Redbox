using System;
using System.Windows;

namespace Redbox.Rental.Model.Browse
{
    public class BrowseMenuButton : DependencyObject
    {
        public static readonly DependencyProperty IsButtonEnabledProperty =
            DependencyProperty.Register(nameof(IsButtonEnabled), typeof(bool), typeof(BrowseMenuButton),
                (PropertyMetadata)new FrameworkPropertyMetadata((object)false));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text),
            typeof(string), typeof(BrowseMenuButton),
            (PropertyMetadata)new FrameworkPropertyMetadata((PropertyChangedCallback)null));

        private string _textStyleName = "font_montserrat_extrabold_14_correct";
        private int _y;
        private int _height = 56;
        private bool _isSelected;

        public string Name { get; set; }

        public string Text
        {
            get { return Dispatcher.Invoke<string>((Func<string>)(() => (string)GetValue(TextProperty))); }
            set { Dispatcher.Invoke((Action)(() => SetValue(TextProperty, (object)value))); }
        }

        public int Width { get; set; }

        public int Height
        {
            get => _height;
            set => _height = value;
        }

        public int X { get; set; }

        public int Y
        {
            get => _y;
            set => _y = value;
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

        public bool IsButtonEnabled
        {
            get { return Dispatcher.Invoke<bool>((Func<bool>)(() => (bool)GetValue(IsButtonEnabledProperty))); }
            set { Dispatcher.Invoke((Action)(() => SetValue(IsButtonEnabledProperty, (object)value))); }
        }

        public bool FixedPosition { get; set; }

        public string TextStyleName
        {
            get => _textStyleName;
            set => _textStyleName = value;
        }
    }
}