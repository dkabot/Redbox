using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls
{
    public partial class DisplayProductUserControl : UserControl
    {
        public static readonly DependencyProperty ShowVacantItemBackgroundProperty = DependencyProperty.Register(
            "ShowVacantItemBackground", typeof(bool), typeof(DisplayProductUserControl),
            new FrameworkPropertyMetadata(false)
            {
                AffectsRender = true
            });

        private DisplayProductModel _displayProductModel;

        public DisplayProductUserControl()
        {
            InitializeComponent();
            ApplyStyle();
        }

        public bool ShowVacantItemBackground
        {
            get => (bool)GetValue(ShowVacantItemBackgroundProperty);
            set => SetValue(ShowVacantItemBackgroundProperty, value);
        }

        public void ApplyStyle()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme != null) theme.SetStyle(AddButton);
            AddButton.IsAnimated = false;
            AddButton.IsEnabled = true;
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _displayProductModel = DataContext as DisplayProductModel;
            MainGrid.Visibility = _displayProductModel != null || ShowVacantItemBackground
                ? Visibility.Visible
                : Visibility.Collapsed;
            if (_displayProductModel == null && ShowVacantItemBackground)
            {
                MainGrid.Background = new SolidColorBrush(Colors.White);
                AddButton.Visibility = Visibility.Collapsed;
                ComingSoonButton.Visibility = Visibility.Collapsed;
                Flag.Visibility = Visibility.Collapsed;
                FlagText.Visibility = Visibility.Collapsed;
            }
            else if (_displayProductModel != null)
            {
                if (_displayProductModel.Banner != null)
                    switch (_displayProductModel.Banner.BannerSize)
                    {
                        case BannerSize.Regular:
                            Grid.SetRow(Banner, 2);
                            Grid.SetRowSpan(Banner, 2);
                            Grid.SetRow(BannerTextViewBox, 1);
                            Grid.SetRowSpan(BannerTextViewBox, 1);
                            BannerText.VerticalAlignment = VerticalAlignment.Center;
                            break;
                        case BannerSize.Mini:
                            Grid.SetRow(Banner, 3);
                            Grid.SetRowSpan(Banner, 1);
                            Grid.SetRow(BannerTextViewBox, 0);
                            Grid.SetRowSpan(BannerTextViewBox, 3);
                            BannerText.VerticalAlignment = VerticalAlignment.Center;
                            break;
                        case BannerSize.Large:
                            Grid.SetRow(Banner, 1);
                            Grid.SetRowSpan(Banner, 3);
                            Grid.SetRow(BannerTextViewBox, 0);
                            Grid.SetRowSpan(BannerTextViewBox, 1);
                            Banner.RowDefinitions[0].Height = new GridLength(0.7, GridUnitType.Star);
                            Banner.RowDefinitions[1].Height = new GridLength(1.0, GridUnitType.Star);
                            Banner.RowDefinitions[1].Height = new GridLength(1.0, GridUnitType.Star);
                            BannerText.VerticalAlignment = VerticalAlignment.Top;
                            break;
                    }

                if (_displayProductModel.ADAMiniCartAddButtonVisibility == Visibility.Visible)
                    MainGrid.Background = new SolidColorBrush(Colors.Transparent);
            }

            UserControl_SizeChanged(null, null);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_displayProductModel != null && _displayProductModel.IsTitleDetailsFlag)
            {
                Flag.Height = ImageBorder.ActualHeight * 0.11;
                Flag.Width = ImageBorder.ActualWidth * 0.65;
            }
            else
            {
                Flag.Height = ImageBorder.ActualHeight * 0.25;
                Flag.Width = ImageBorder.ActualWidth;
            }

            var num = -Flag.Height / 2.0;
            Flag.Margin = new Thickness(num * 2.0 / 3.0, num, 0.0, 0.0);
            AddButton.IsEnabled = true;
        }
    }
}