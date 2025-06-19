using System.Windows;
using System.Windows.Media;

namespace Redbox.Rental.UI.Models
{
    public class DisplayGroupSelectionItemModel : DependencyObject
    {
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string),
            typeof(DisplayGroupSelectionItemModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty TextStyleProperty = DependencyProperty.Register("TextStyleP",
            typeof(Style), typeof(DisplayGroupSelectionItemModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty BackgroundBrushProperty =
            DependencyProperty.Register("BackgroundBrush", typeof(Brush), typeof(DisplayGroupSelectionItemModel),
                new FrameworkPropertyMetadata(null));

        private bool _isEnabled;

        private bool _isSelected;

        public string Name
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(NameProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(NameProperty, value); }); }
        }

        public Style TextStyle
        {
            get { return Dispatcher.Invoke(() => (Style)GetValue(TextStyleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(TextStyleProperty, value); }); }
        }

        public Brush BackgroundBrush
        {
            get { return Dispatcher.Invoke(() => (Brush)GetValue(BackgroundBrushProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BackgroundBrushProperty, value); }); }
        }

        public Style EnableTextStyle { get; set; }

        public Style DisabledTextStyle { get; set; }

        public Brush SelectedBackgroundBrush { get; set; }

        public Brush UnselectedBackgroundBrusch { get; set; }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                TextStyle = _isEnabled ? EnableTextStyle : DisabledTextStyle;
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                BackgroundBrush = _isSelected ? SelectedBackgroundBrush : UnselectedBackgroundBrusch;
            }
        }
    }
}