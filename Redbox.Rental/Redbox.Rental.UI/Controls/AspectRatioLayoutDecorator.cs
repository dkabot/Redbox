using System;
using System.Windows;
using System.Windows.Controls;

namespace Redbox.Rental.UI.Controls
{
    public class AspectRatioLayoutDecorator : Decorator
    {
        public static readonly DependencyProperty AspectRatioProperty = DependencyProperty.Register("AspectRatio",
            typeof(double), typeof(AspectRatioLayoutDecorator),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsMeasure), ValidateAspectRatio);

        public static readonly DependencyProperty LockAspectRatioProperty = DependencyProperty.Register(
            "LockAspectRatio", typeof(bool), typeof(AspectRatioLayoutDecorator), new FrameworkPropertyMetadata(false)
            {
                AffectsArrange = true
            });

        public bool LockAspectRatio
        {
            get => (bool)GetValue(LockAspectRatioProperty);
            set => SetValue(LockAspectRatioProperty, value);
        }

        public double AspectRatio
        {
            get => (double)GetValue(AspectRatioProperty);
            set => SetValue(AspectRatioProperty, value);
        }

        private static bool ValidateAspectRatio(object value)
        {
            if (!(value is double)) return false;
            var num = (double)value;
            return num > 0.0 && !double.IsInfinity(num) && !double.IsNaN(num);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (Child == null) return new Size(0.0, 0.0);
            constraint = SizeToRatio(constraint, false);
            Child.Measure(constraint);
            if (double.IsInfinity(constraint.Width) || double.IsInfinity(constraint.Height))
                return SizeToRatio(Child.DesiredSize, true);
            return constraint;
        }

        public Size SizeToRatio(Size size, bool expand)
        {
            var aspectRatio = AspectRatio;
            var num = size.Width / aspectRatio;
            var num2 = size.Height * aspectRatio;
            if (expand)
            {
                num2 = Math.Max(num2, size.Width);
                num = Math.Max(num, size.Height);
            }
            else
            {
                num2 = Math.Min(num2, size.Width);
                num = Math.Min(num, size.Height);
            }

            return new Size(num2, num);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (Child != null)
            {
                var size = arrangeSize;
                if (LockAspectRatio) size = SizeToRatio(arrangeSize, false);
                var num = arrangeSize.Width - size.Width;
                var num2 = arrangeSize.Height - size.Height;
                var num3 = 0.0;
                var num4 = 0.0;
                if (!double.IsNaN(num) && !double.IsInfinity(num)) num4 = num / 2.0;
                if (!double.IsNaN(num2) && !double.IsInfinity(num2)) num3 = num2 / 2.0;
                var rect = new Rect(new Point(num4, num3), size);
                Child.Arrange(rect);
            }

            return arrangeSize;
        }
    }
}