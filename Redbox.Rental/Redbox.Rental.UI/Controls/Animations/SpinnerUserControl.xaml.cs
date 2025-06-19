using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Redbox.Rental.UI.Controls.Animations
{
    public partial class SpinnerUserControl : UserControl
    {
        private bool _isAnimated;

        public SpinnerUserControl()
        {
            InitializeComponent();
        }

        public bool IsAnimated
        {
            get => _isAnimated;
            set
            {
                if (_isAnimated != value)
                {
                    _isAnimated = value;
                    if (_isAnimated)
                    {
                        SpinTheSpinner();
                        return;
                    }

                    Spinner.RenderTransform = null;
                }
            }
        }

        private void SpinTheSpinner()
        {
            var rotateTransform = new RotateTransform();
            var doubleAnimation = new DoubleAnimation(0.0, 360.0, new Duration(TimeSpan.FromSeconds(1.0)));
            doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Spinner.RenderTransform = rotateTransform;
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, doubleAnimation);
        }
    }
}