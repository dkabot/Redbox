using Redbox.Controls.BaseControls;
using Redbox.Controls.Utilities;
using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Redbox.Controls
{
    [Localizability(LocalizationCategory.Ignore)]
    [ContentProperty("Child")]
    public class SpinControl : BaseContainer
    {
        private DoubleAnimation RotateAnimation = new DoubleAnimation()
        {
            From = new double?(0.0),
            To = new double?(360.0)
        };

        public static readonly DependencyProperty RadiusProperty =
            Dependency<SpinControl>.CreateDependencyProperty(nameof(Radius), typeof(double), (object)0.0,
                new PropertyChangedCallback(OnRadiusChanged));

        public static readonly DependencyProperty PeriodProperty =
            Dependency<SpinControl>.CreateDependencyProperty(nameof(Period), typeof(double), (object)0.0);

        public static readonly DependencyProperty BorderRectProperty =
            Dependency<SpinControl>.CreateDependencyProperty(nameof(BorderRect), typeof(Rect),
                (object)new Rect(0.0, 0.0, 0.0, 0.0));

        public static readonly DependencyProperty IsAnimatedProperty =
            Dependency<SpinControl>.CreateDependencyProperty(nameof(IsAnimated), typeof(bool), (object)false,
                new PropertyChangedCallback(OnIsAnimatedChanged));

        public static readonly DependencyProperty RepeatForeverProperty =
            Dependency<SpinControl>.CreateDependencyProperty(nameof(RepeatForever), typeof(bool), (object)false);

        public SpinControl()
        {
            Focusable = false;
            IsVisibleChanged += new DependencyPropertyChangedEventHandler(OnVisiblePropertyChanged);
        }

        static SpinControl()
        {
            Dependency<SpinControl>.DefaultOverrideMetadata(
                new Action<Type, PropertyMetadata>(DefaultStyleKeyProperty.OverrideMetadata));
            WidthProperty.OverrideMetadata(typeof(SpinControl), Dependency<SectionalBar>.CreatePropertyMetadata());
            HeightProperty.OverrideMetadata(typeof(SpinControl), Dependency<SectionalBar>.CreatePropertyMetadata());
            RenderTransformProperty.OverrideMetadata(typeof(SpinControl),
                Dependency<SectionalBar>.CreatePropertyMetadata(
                    new PropertyChangedCallback(OnRenderTransformPropertyChanged)));
        }

        public new double Width
        {
            get => (double)GetValue(WidthProperty);
            protected set => SetValue(WidthProperty, (object)value);
        }

        public new double Height
        {
            get => (double)GetValue(HeightProperty);
            protected set => SetValue(HeightProperty, (object)value);
        }

        public double Radius
        {
            get => (double)GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, (object)value);
        }

        public Rect BorderRect
        {
            get => (Rect)GetValue(BorderRectProperty);
            protected set => SetValue(BorderRectProperty, (object)value);
        }

        public bool IsAnimated
        {
            get => (bool)GetValue(IsAnimatedProperty);
            set => SetValue(IsAnimatedProperty, (object)value);
        }

        public double Period
        {
            get => (double)GetValue(PeriodProperty);
            set => SetValue(PeriodProperty, (object)value);
        }

        public bool RepeatForever
        {
            get => (bool)GetValue(RepeatForeverProperty);
            set => SetValue(RepeatForeverProperty, (object)value);
        }

        private static void OnRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SpinControl).RadiusChanged();
        }

        private static void OnIsAnimatedChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            (d as SpinControl).IsAnimatedChanged();
        }

        private static void OnRenderTransformPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
        }

        private void OnVisiblePropertyChanged(object s, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && IsAnimated)
                AnimationBegin();
            else
                AnimationEnd();
        }

        protected virtual void IsAnimatedChanged()
        {
            var renderTransformOrigin = RenderTransformOrigin;
            if (renderTransformOrigin.X != 0.5 || renderTransformOrigin.Y != 0.5)
                RenderTransformOrigin = new Point(0.5, 0.5);
            if (RenderTransform == null || !(RenderTransform is RotateTransform))
                RenderTransform = (Transform)new RotateTransform();
            (RenderTransform as RotateTransform).Angle = 0.0;
        }

        protected virtual void RadiusChanged()
        {
            Width = Height = Radius * 2.0;
        }

        protected virtual void AnimationEnd()
        {
            if (RenderTransform == null)
                return;
            RotateAnimation.BeginTime = new TimeSpan?();
            RenderTransform = (Transform)null;
        }

        protected virtual void AnimationBegin()
        {
            SetChildSpinSize();
            RotateAnimation.BeginTime = new TimeSpan?(TimeSpan.FromMilliseconds(0.0));
            RotateAnimation.Duration = new Duration(TimeSpan.FromSeconds(Period));
            if (RepeatForever)
                RotateAnimation.RepeatBehavior = RepeatBehavior.Forever;
            if (RenderTransform == null)
                IsAnimatedChanged();
            if (!(RenderTransform is RotateTransform renderTransform))
                return;
            renderTransform.BeginAnimation(RotateTransform.AngleProperty, (AnimationTimeline)RotateAnimation);
        }

        protected void SetChildSpinSize(Size? size = null)
        {
            size = !size.HasValue ? new Size?(new Size(Width, Height)) : size;
            if (!(Child is FrameworkElement))
                return;
            var child = Child as FrameworkElement;
            child.Width = size.Value.Width;
            child.Height = size.Value.Height;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            BorderRect = new Rect(sizeInfo.NewSize);
            SetChildSpinSize(new Size?(sizeInfo.NewSize));
            base.OnRenderSizeChanged(sizeInfo);
        }
    }
}