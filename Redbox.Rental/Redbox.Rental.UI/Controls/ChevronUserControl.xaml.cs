using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Redbox.Rental.UI.Controls
{
    public partial class ChevronUserControl : UserControl
    {
        public enum Direction
        {
            Left,
            Right
        }

        private static readonly DependencyProperty ChevronDirectionProperty =
            DependencyProperty.Register("ChevronDirection", typeof(Direction), typeof(ChevronUserControl),
                new PropertyMetadata(Direction.Right, ChevronDirectionChanged));

        private static readonly DependencyProperty ChevronColorProperty = DependencyProperty.Register("ChevronColor",
            typeof(Brush), typeof(ChevronUserControl), new PropertyMetadata(Brushes.Black));

        public ChevronUserControl()
        {
            ChevronColor = Brushes.White;
            InitializeComponent();
            RightArrow.Visibility = Visibility.Visible;
            LeftArrow.Visibility = Visibility.Collapsed;
        }

        public Direction ChevronDirection
        {
            get { return Dispatcher.Invoke(() => (Direction)GetValue(ChevronDirectionProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ChevronDirectionProperty, value); }); }
        }

        public Brush ChevronColor
        {
            get { return Dispatcher.Invoke(() => (Brush)GetValue(ChevronColorProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ChevronColorProperty, value); }); }
        }

        private static void ChevronDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var chevronUserControl = d as ChevronUserControl;
            if (d != null)
            {
                if ((Direction)e.NewValue == Direction.Right)
                {
                    chevronUserControl.RightArrow.Visibility = Visibility.Visible;
                    chevronUserControl.LeftArrow.Visibility = Visibility.Collapsed;
                    return;
                }

                chevronUserControl.LeftArrow.Visibility = Visibility.Visible;
                chevronUserControl.RightArrow.Visibility = Visibility.Collapsed;
            }
        }
    }
}