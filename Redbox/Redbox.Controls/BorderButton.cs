using Redbox.Controls.Utilities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Redbox.Controls
{
    [Localizability(LocalizationCategory.Button)]
    public class BorderButton : Button
    {
        protected static List<BorderButton> borderButtons = new List<BorderButton>();

        public static readonly DependencyProperty ButtonStateProperty =
            Dependency<BorderButton>.CreateDependencyProperty(nameof(ButtonState), typeof(int), (object)0);

        public static readonly DependencyProperty IsSelectedProperty =
            Dependency<BorderButton>.CreateDependencyProperty(nameof(IsSelected), typeof(bool), (object)false);

        public static readonly DependencyProperty CornerRadiusProperty =
            Dependency<BorderButton>.CreateDependencyProperty(nameof(CornerRadius), typeof(CornerRadius),
                (object)new CornerRadius(0.0));

        public static readonly DependencyProperty AutoCornerRadiusProperty =
            Dependency<BorderButton>.CreateDependencyProperty(nameof(AutoCornerRadius), typeof(bool), (object)false);

        public static readonly DependencyProperty EffectColorProperty =
            Dependency<BorderButton>.CreateDependencyProperty(nameof(EffectColor), typeof(Color), (object)Colors.Black);

        public static readonly DependencyProperty SingleRadiusProperty =
            Dependency<BorderButton>.CreateDependencyProperty(nameof(SingleRadius), typeof(double), (object)0.0,
                new PropertyChangedCallback(OnSingleRadiusChanged));

        public static readonly DependencyProperty GroupNameProperty =
            Dependency<BorderButton>.CreateDependencyProperty(nameof(GroupName), typeof(string), (object)string.Empty,
                new PropertyChangedCallback(OnGroupNameChanged));

        public static readonly DependencyProperty BorderSizeProperty =
            Dependency<BorderButton>.CreateDependencyProperty(nameof(BorderSize), typeof(Size),
                (object)new Size(0.0, 0.0), new PropertyChangedCallback(OnBorderSizeChanged));

        public static readonly DependencyProperty BorderRectProperty =
            Dependency<BorderButton>.CreateDependencyProperty(nameof(BorderRect), typeof(Rect),
                (object)new Rect(0.0, 0.0, 0.0, 0.0));

        public static readonly DependencyProperty DisposeProperty =
            Dependency<BorderButton>.CreateDependencyProperty(nameof(Dispose), typeof(bool), (object)false,
                new PropertyChangedCallback(OnDisposeChanged));

        static BorderButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BorderButton),
                (PropertyMetadata)new FrameworkPropertyMetadata((object)typeof(BorderButton)));
        }

        public BorderButton()
        {
            Focusable = false;
            ClickMode = ClickMode.Release;
        }

        protected static void Register(BorderButton borderButton)
        {
            if (borderButtons.Contains(borderButton))
                return;
            borderButtons.Add(borderButton);
        }

        protected static void Unregister(BorderButton borderButton)
        {
            if (!borderButtons.Contains(borderButton))
                return;
            borderButtons.Remove(borderButton);
        }

        private static void OnSingleRadiusChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var borderButton = d as BorderButton;
            if (borderButton.AutoCornerRadius)
                return;
            borderButton.CornerRadius = new CornerRadius(borderButton.SingleRadius);
        }

        private static void OnBorderSizeChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var borderButton = d as BorderButton;
            borderButton.Width = borderButton.BorderSize.Width;
            borderButton.Height = borderButton.BorderSize.Height;
        }

        private static void OnDisposeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
                return;
            Unregister(d as BorderButton);
        }

        private static void OnGroupNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.NewValue as string))
                Unregister(d as BorderButton);
            else
                Register(d as BorderButton);
        }

        protected void UpdateSelection()
        {
            if (string.IsNullOrEmpty(GroupName))
                return;
            foreach (var borderButton in borderButtons)
                if (this != borderButton && GroupName.Equals(borderButton.GroupName))
                    borderButton.IsSelected = false;
        }

        [DefaultValue("")]
        [Localizability(LocalizationCategory.NeverLocalize)]
        public string GroupName
        {
            get => (string)GetValue(GroupNameProperty);
            set => SetValue(GroupNameProperty, (object)value);
        }

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set
            {
                if (value)
                    UpdateSelection();
                SetValue(IsSelectedProperty, (object)value);
            }
        }

        public int ButtonState
        {
            get => (int)GetValue(ButtonStateProperty);
            set => SetValue(ButtonStateProperty, (object)value);
        }

        public bool Dispose
        {
            get => (bool)GetValue(DisposeProperty);
            set => SetValue(DisposeProperty, (object)value);
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, (object)value);
        }

        public double SingleRadius
        {
            get => (double)GetValue(SingleRadiusProperty);
            set => SetValue(SingleRadiusProperty, (object)value);
        }

        public bool AutoCornerRadius
        {
            get => (bool)GetValue(AutoCornerRadiusProperty);
            set => SetValue(AutoCornerRadiusProperty, (object)value);
        }

        public Size BorderSize
        {
            get => (Size)GetValue(BorderSizeProperty);
            set => SetValue(BorderSizeProperty, (object)value);
        }

        public Rect BorderRect
        {
            get => (Rect)GetValue(BorderRectProperty);
            protected set => SetValue(BorderRectProperty, (object)value);
        }

        public Color EffectColor
        {
            get => (Color)GetValue(EffectColorProperty);
            set => SetValue(EffectColorProperty, (object)value);
        }

        protected override void OnClick()
        {
            IsSelected = true;
            base.OnClick();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            var newSize = sizeInfo.NewSize;
            var width = newSize.Width;
            newSize = sizeInfo.NewSize;
            var height = newSize.Height;
            BorderSize = new Size(width, height);
            BorderRect = new Rect(sizeInfo.NewSize);
            if (Width != BorderSize.Width)
                Width = BorderSize.Width;
            if (Height != BorderSize.Height)
                Height = BorderSize.Height;
            if (!AutoCornerRadius)
                return;
            SingleRadius = ActualHeight / 2.0;
            CornerRadius = new CornerRadius(SingleRadius);
        }
    }
}