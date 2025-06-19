using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls
{
    public partial class DisplayProductCarouselUserControl : UserControl
    {
        public static readonly DependencyProperty ShowVacantItemBackgroundProperty = DependencyProperty.Register(
            "ShowVacantItemBackground", typeof(bool), typeof(DisplayProductCarouselUserControl),
            new FrameworkPropertyMetadata(false)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty ShaderOpacityProperty = DependencyProperty.Register("ShaderOpacity",
            typeof(double), typeof(DisplayProductCarouselUserControl), new FrameworkPropertyMetadata(0.25)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty ShadowDepthProperty = DependencyProperty.Register("ShadowDepth",
            typeof(double), typeof(DisplayProductCarouselUserControl), new FrameworkPropertyMetadata(0.0)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty ShadowOpacityProperty = DependencyProperty.Register("ShadowOpacity",
            typeof(double), typeof(DisplayProductCarouselUserControl), new FrameworkPropertyMetadata(0.0)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty ImageBorderThicknessProperty = DependencyProperty.Register(
            "ImageBorderThickness", typeof(Thickness), typeof(DisplayProductCarouselUserControl),
            new FrameworkPropertyMetadata(new Thickness(0.0))
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty ImageBorderColorProperty =
            DependencyProperty.Register("ImageBorderColor", typeof(Color), typeof(DisplayProductCarouselUserControl),
                new FrameworkPropertyMetadata(Colors.Gray));

        private DisplayProductModel _displayProductModel;

        public DisplayProductCarouselUserControl()
        {
            InitializeComponent();
        }

        public bool ShowVacantItemBackground
        {
            get => (bool)GetValue(ShowVacantItemBackgroundProperty);
            set => SetValue(ShowVacantItemBackgroundProperty, value);
        }

        public double ShaderOpacity
        {
            get => (double)GetValue(ShaderOpacityProperty);
            set => SetValue(ShaderOpacityProperty, value);
        }

        public double ShadowDepth
        {
            get => (double)GetValue(ShadowDepthProperty);
            set => SetValue(ShadowDepthProperty, value);
        }

        public double ShadowOpacity
        {
            get => (double)GetValue(ShadowOpacityProperty);
            set => SetValue(ShadowOpacityProperty, value);
        }

        public Thickness ImageBorderThickness
        {
            get => (Thickness)GetValue(ImageBorderThicknessProperty);
            set => SetValue(ImageBorderThicknessProperty, value);
        }

        public Color ImageBorderColor
        {
            get => (Color)GetValue(ImageBorderColorProperty);
            set => SetValue(ImageBorderColorProperty, value);
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _displayProductModel = DataContext as DisplayProductModel;
            MainGrid.Visibility = _displayProductModel != null || ShowVacantItemBackground
                ? Visibility.Visible
                : Visibility.Collapsed;
            if (_displayProductModel == null && ShowVacantItemBackground)
                MainGrid.Background = new SolidColorBrush(Colors.White);
        }
    }
}