using Redbox.Controls.Themes;
using Redbox.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Redbox.Controls
{
    public class RoundedButton : Button, IThemedControl
    {
        private int _executingButtonClick;
        private Storyboard _storyboard;

        public static readonly DependencyProperty StateProperty = DependencyProperty.Register(nameof(State),
            typeof(bool), typeof(RoundedButton), (PropertyMetadata)new FrameworkPropertyMetadata((object)false));

        public static readonly DependencyProperty UsedDefaultCornerRadiusProperty =
            DependencyProperty.Register(nameof(UsedDefaultCornerRadius), typeof(bool), typeof(RoundedButton),
                (PropertyMetadata)new FrameworkPropertyMetadata((object)false));

        public static readonly DependencyProperty IsClickAnimatedProperty =
            DependencyProperty.Register("IsClickAnimated", typeof(bool), typeof(RoundedButton),
                (PropertyMetadata)new FrameworkPropertyMetadata((object)true));

        public static readonly DependencyProperty IsClickBeforeAnimationProperty =
            DependencyProperty.Register(nameof(IsClickBeforeAnimation), typeof(bool), typeof(RoundedButton),
                (PropertyMetadata)new FrameworkPropertyMetadata((object)false));

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(double), typeof(RoundedButton),
            (PropertyMetadata)new FrameworkPropertyMetadata((object)0.0,
                new PropertyChangedCallback(OnCornerRadiusPropertyChanged))
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty CornerRadiusInnerProperty = DependencyProperty.Register(
            nameof(CornerRadiusInner), typeof(double), typeof(RoundedButton),
            (PropertyMetadata)new FrameworkPropertyMetadata((object)0.0)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty DropShadowDepthProperty = DependencyProperty.Register(
            nameof(DropShadowDepth), typeof(double), typeof(RoundedButton),
            (PropertyMetadata)new FrameworkPropertyMetadata((object)5.0)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty GlassHighlightVisibilityProperty =
            DependencyProperty.Register(nameof(GlassHighlightVisibility), typeof(Visibility), typeof(RoundedButton),
                (PropertyMetadata)new FrameworkPropertyMetadata((object)Visibility.Visible));

        public static readonly DependencyProperty GlassHighlightWidthProperty =
            DependencyProperty.Register(nameof(GlassHighlightWidth), typeof(double), typeof(RoundedButton),
                (PropertyMetadata)new FrameworkPropertyMetadata((object)0.0));

        public static readonly DependencyProperty GlassHighlightHeightProperty =
            DependencyProperty.Register(nameof(GlassHighlightHeight), typeof(double), typeof(RoundedButton),
                (PropertyMetadata)new FrameworkPropertyMetadata((object)0.0));

        public static readonly DependencyProperty DropShadowOpacityProperty =
            DependencyProperty.Register(nameof(DropShadowOpacity), typeof(double), typeof(RoundedButton),
                (PropertyMetadata)new FrameworkPropertyMetadata((object)100.0));

        public static readonly DependencyProperty GlassHighlightCornerRadiusProperty =
            DependencyProperty.Register(nameof(GlassHighlightCornerRadius), typeof(double), typeof(RoundedButton),
                (PropertyMetadata)new FrameworkPropertyMetadata((object)0.0));

        public static readonly DependencyProperty BorderColorProperty = DependencyProperty.Register(nameof(BorderColor),
            typeof(Color), typeof(RoundedButton),
            (PropertyMetadata)new FrameworkPropertyMetadata((object)Colors.Black));

        public static readonly DependencyProperty BorderGradientColorProperty =
            DependencyProperty.Register(nameof(BorderGradientColor), typeof(Color), typeof(RoundedButton),
                (PropertyMetadata)new FrameworkPropertyMetadata((object)Colors.Black));

        public static readonly DependencyProperty BorderGradientStartPointProperty =
            DependencyProperty.Register(nameof(BorderGradientStartPoint), typeof(Point), typeof(RoundedButton),
                (PropertyMetadata)new FrameworkPropertyMetadata((object)new Point(0.5, 0.0)));

        public static readonly DependencyProperty BorderGradientEndPointProperty =
            DependencyProperty.Register(nameof(BorderGradientEndPoint), typeof(Point), typeof(RoundedButton),
                (PropertyMetadata)new FrameworkPropertyMetadata((object)new Point(0.5, 1.0)));

        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register(nameof(BackgroundColor), typeof(Color), typeof(RoundedButton),
                (PropertyMetadata)new FrameworkPropertyMetadata((object)Colors.Black));

        public static readonly DependencyProperty BackgroundGradientColorProperty =
            DependencyProperty.Register(nameof(BackgroundGradientColor), typeof(Color), typeof(RoundedButton),
                (PropertyMetadata)new FrameworkPropertyMetadata((object)Colors.Gray));

        public static readonly DependencyProperty BackgroundGradientStartPointProperty =
            DependencyProperty.Register(nameof(BackgroundGradientStartPoint), typeof(Point), typeof(RoundedButton),
                (PropertyMetadata)new FrameworkPropertyMetadata((object)new Point(0.5, 0.0)));

        public static readonly DependencyProperty BackgroundGradientEndPointProperty =
            DependencyProperty.Register(nameof(BackgroundGradientEndPoint), typeof(Point), typeof(RoundedButton),
                (PropertyMetadata)new FrameworkPropertyMetadata((object)new Point(0.5, 1.0)));

        public RoundedButton()
        {
            Focusable = false;
            IsAnimated = DefaultIsAnimated;
            if (!IsAnimated)
                return;
            CreateAnimationStoryboard();
        }

        private void CreateAnimationStoryboard()
        {
            var num1 = 0.05;
            var num2 = num1 * 2.0;
            var storyboard = new Storyboard();
            storyboard.Name = "ButtonAnimationStoryboard";
            storyboard.Duration = new Duration(TimeSpan.FromSeconds(num2));
            _storyboard = storyboard;
            RenderTransformOrigin = new Point(0.5, 0.5);
            var scaleTransform = new ScaleTransform();
            RenderTransform = (Transform)new TransformGroup()
            {
                Children =
                {
                    (Transform)scaleTransform
                }
            };
            var doubleAnimation1 = new DoubleAnimation();
            doubleAnimation1.From = new double?(1.0);
            doubleAnimation1.To = new double?(0.95);
            doubleAnimation1.Duration = new Duration(TimeSpan.FromSeconds(num1));
            doubleAnimation1.AutoReverse = true;
            var element1 = doubleAnimation1;
            _storyboard.Children.Add((Timeline)element1);
            Storyboard.SetTarget((DependencyObject)element1, (DependencyObject)this);
            Storyboard.SetTargetProperty((DependencyObject)element1,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleX)",
                    Array.Empty<object>()));
            var doubleAnimation2 = new DoubleAnimation();
            doubleAnimation2.From = new double?(1.0);
            doubleAnimation2.To = new double?(0.95);
            doubleAnimation2.Duration = new Duration(TimeSpan.FromSeconds(num1));
            doubleAnimation2.AutoReverse = true;
            var element2 = doubleAnimation2;
            _storyboard.Children.Add((Timeline)element2);
            Storyboard.SetTarget((DependencyObject)element2, (DependencyObject)this);
            Storyboard.SetTargetProperty((DependencyObject)element2,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleY)",
                    Array.Empty<object>()));
            Triggers.Add((TriggerBase)new EventTrigger(PreviewMouseDownEvent)
            {
                Actions =
                {
                    (TriggerAction)new BeginStoryboard()
                    {
                        Storyboard = _storyboard
                    }
                }
            });
        }

        public static bool DefaultIsAnimated { get; set; }

        public static TaskScheduler UITaskScheduler { get; set; }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
                e.Handled = true;
            base.OnPreviewMouseDown(e);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (UsedDefaultCornerRadius)
                CornerRadius = ActualHeight / 2.0;
            AdjustGlassHighlight(this);
            AdjustCornRadiusInner(this);
        }

        private static void AdjustGlassHighlight(RoundedButton roundedButton)
        {
            roundedButton.GlassHighlightWidth = Math.Max(roundedButton.ActualWidth - roundedButton.CornerRadius, 0.0);
            roundedButton.GlassHighlightHeight = roundedButton.CornerRadius * 0.66;
            roundedButton.GlassHighlightCornerRadius = roundedButton.CornerRadius * 0.66;
        }

        protected override void OnClick()
        {
            if (IsClickBeforeAnimation || !IsAnimated)
            {
                ButtonClick();
            }
            else if (Interlocked.CompareExchange(ref _executingButtonClick, 1, 0) == 1)
            {
                LogHelper.Instance.Log("RoundedButton click is already being executed.  Ignoring this call.");
            }
            else
            {
                var timer = new System.Timers.Timer();
                timer.AutoReset = false;
                var storyboard = _storyboard;
                timer.Interval = (double)((storyboard != null ? storyboard.Duration.TimeSpan.Milliseconds : 0) + 10);
                var buttonTimer = timer;
                buttonTimer.Elapsed += (ElapsedEventHandler)((sender, args) =>
                {
                    try
                    {
                        buttonTimer.Stop();
                        buttonTimer.Dispose();
                        ButtonClick();
                    }
                    finally
                    {
                        _executingButtonClick = 0;
                    }
                });
                buttonTimer.Start();
            }
        }

        private void ButtonClick()
        {
            Task.Factory.StartNew((Action)(() =>
                {
                    State = !State;
                    LogButtonClick();
                    base.OnClick();
                }), CancellationToken.None, TaskCreationOptions.None,
                UITaskScheduler != null ? UITaskScheduler : TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void LogButtonClick()
        {
            string str1 = null;
            var parent = VisualTreeHelper.GetParent(this);

            while (parent != null && !(parent is Window) && !(parent is UserControl))
                parent = VisualTreeHelper.GetParent(parent);

            if (parent != null)
            {
                if (parent is UserControl userControl)
                {
                    str1 = userControl.Name;
                    if (string.IsNullOrEmpty(str1))
                        str1 = userControl?.GetType()?.ToString();
                }
                else if (parent is Window window)
                {
                    str1 = window.Name;
                    if (string.IsNullOrEmpty(str1))
                        str1 = window?.GetType()?.ToString();
                }
            }

            var str2 = "";
            if (!string.IsNullOrEmpty(Name))
            {
                str2 = "Button Name: " + Name;
            }
            else
            {
                var str3 = Tag?.ToString();
                if (!string.IsNullOrEmpty(str3))
                    str2 = "Tag: " + str3.Substring(0, Math.Min(str3.Length, 50));
            }

            LogHelper.Instance.Log("RoundedButton click: Parent: " + str1 + ", " + str2);
        }

        private static void OnCornerRadiusPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var roundedButton = d as RoundedButton;
            AdjustGlassHighlight(roundedButton);
            AdjustCornRadiusInner(roundedButton);
            roundedButton.InvalidateProperty(WidthProperty);
        }

        private static void AdjustCornRadiusInner(RoundedButton roundedButton)
        {
            double num;
            if ((num = roundedButton.CornerRadius - 1.0) <= 0.0)
                num = 0.0;
            roundedButton.CornerRadiusInner = num;
        }

        public bool State
        {
            get => (bool)GetValue(StateProperty);
            set => SetValue(StateProperty, (object)value);
        }

        public double CornerRadiusInner
        {
            get => (double)GetValue(CornerRadiusInnerProperty);
            set => SetValue(CornerRadiusInnerProperty, (object)value);
        }

        public double CornerRadius
        {
            get => (double)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, (object)value);
        }

        public double GlassHighlightWidth
        {
            get => (double)GetValue(GlassHighlightWidthProperty);
            set => SetValue(GlassHighlightWidthProperty, (object)value);
        }

        public double GlassHighlightHeight
        {
            get => (double)GetValue(GlassHighlightHeightProperty);
            set => SetValue(GlassHighlightHeightProperty, (object)value);
        }

        public double GlassHighlightCornerRadius
        {
            get => (double)GetValue(GlassHighlightCornerRadiusProperty);
            set => SetValue(GlassHighlightCornerRadiusProperty, (object)value);
        }

        public double DropShadowOpacity
        {
            get => (double)GetValue(DropShadowOpacityProperty);
            set => SetValue(DropShadowOpacityProperty, (object)value);
        }

        public double DropShadowDepth
        {
            get => (double)GetValue(DropShadowDepthProperty);
            set => SetValue(DropShadowDepthProperty, (object)value);
        }

        public Color BorderColor
        {
            get => (Color)GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, (object)value);
        }

        public Color BorderGradientColor
        {
            get => (Color)GetValue(BorderGradientColorProperty);
            set => SetValue(BorderGradientColorProperty, (object)value);
        }

        public Point BorderGradientStartPoint
        {
            get => (Point)GetValue(BorderGradientStartPointProperty);
            set => SetValue(BorderGradientStartPointProperty, (object)value);
        }

        public Point BorderGradientEndPoint
        {
            get => (Point)GetValue(BorderGradientEndPointProperty);
            set => SetValue(BorderGradientEndPointProperty, (object)value);
        }

        public Color BackgroundColor
        {
            get => (Color)GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, (object)value);
        }

        public Color BackgroundGradientColor
        {
            get => (Color)GetValue(BackgroundGradientColorProperty);
            set => SetValue(BackgroundGradientColorProperty, (object)value);
        }

        public Point BackgroundGradientStartPoint
        {
            get => (Point)GetValue(BackgroundGradientStartPointProperty);
            set => SetValue(BackgroundGradientStartPointProperty, (object)value);
        }

        public Point BackgroundGradientEndPoint
        {
            get => (Point)GetValue(BackgroundGradientEndPointProperty);
            set => SetValue(BackgroundGradientEndPointProperty, (object)value);
        }

        public Visibility GlassHighlightVisibility
        {
            get => (Visibility)GetValue(GlassHighlightVisibilityProperty);
            set => SetValue(GlassHighlightVisibilityProperty, (object)value);
        }

        public bool IsAnimated
        {
            get => (bool)GetValue(IsClickAnimatedProperty);
            set => SetValue(IsClickAnimatedProperty, (object)value);
        }

        public bool UsedDefaultCornerRadius
        {
            get => (bool)GetValue(UsedDefaultCornerRadiusProperty);
            set => SetValue(UsedDefaultCornerRadiusProperty, (object)value);
        }

        public bool IsClickBeforeAnimation
        {
            get => (bool)GetValue(IsClickBeforeAnimationProperty);
            set => SetValue(IsClickBeforeAnimationProperty, (object)value);
        }
    }
}