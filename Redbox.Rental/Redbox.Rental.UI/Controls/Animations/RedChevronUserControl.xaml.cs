using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Redbox.Rental.UI.Controls.Animations
{
    public partial class RedChevronUserControl : UserControl
    {
        private static readonly DependencyProperty ChevronColorProperty = DependencyProperty.Register("ChevronColor",
            typeof(Brush), typeof(RedChevronUserControl),
            new PropertyMetadata((SolidColorBrush)new BrushConverter().ConvertFrom("#D01D3E")));

        private static readonly DependencyProperty ChevronOpacityProperty =
            DependencyProperty.Register("ChevronOpacity", typeof(double), typeof(RedChevronUserControl),
                new PropertyMetadata(1.0));

        private bool m_isAnimated;

        public RedChevronUserControl()
        {
            InitializeComponent();
        }

        public bool IsAnimated
        {
            get => m_isAnimated;
            set
            {
                if (m_isAnimated != value)
                {
                    m_isAnimated = value;
                    if (value)
                    {
                        BeginAnimation();
                        return;
                    }

                    StopAnimation();
                }
            }
        }

        public Brush ChevronColor
        {
            get { return Dispatcher.Invoke(() => (Brush)GetValue(ChevronColorProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ChevronColorProperty, value); }); }
        }

        public double ChevronOpacity
        {
            get { return Dispatcher.Invoke(() => (double)GetValue(ChevronOpacityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ChevronOpacityProperty, value); }); }
        }

        public void BeginAnimation()
        {
            var doubleAnimation = new DoubleAnimation(0.0, 150.0, new Duration(TimeSpan.FromSeconds(3.0)));
            doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            var translateTransform = new TranslateTransform();
            ChevronContainer.RenderTransform = translateTransform;
            translateTransform.BeginAnimation(TranslateTransform.XProperty, doubleAnimation);
        }

        public void StopAnimation()
        {
            ChevronContainer.RenderTransform = null;
        }
    }
}