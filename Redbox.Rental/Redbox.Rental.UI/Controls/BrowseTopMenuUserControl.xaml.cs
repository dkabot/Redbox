using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Redbox.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls
{
    public partial class BrowseTopMenuUserControl : RentalUserControl, IWPFActor
    {
        public BrowseTopMenuUserControl()
        {
            InitializeComponent();
        }

        private TopMenuModel BrowseTopMenuModel => DataContext as TopMenuModel;

        private static Style GetStyle(FrameworkElement frameworkElement, string styleName)
        {
            Style style = null;
            var obj = frameworkElement.FindResource(styleName);
            if (obj != null) style = obj as Style;
            return style;
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var topMenuModel = e.NewValue as TopMenuModel;
            if (topMenuModel != null)
            {
                NonFixedButtonContainer.Children.Clear();
                NonFixedButtonContainer.ColumnDefinitions.Clear();
                FixedButtonContainer.Children.Clear();
                if (topMenuModel.MenuButtons != null)
                {
                    var service = ServiceLocator.Instance.GetService<IThemeService>();
                    var theme = service != null ? service.CurrentTheme : null;
                    foreach (var browseMenuButton in topMenuModel.MenuButtons)
                    {
                        var browseProductFamilyMenuButton = browseMenuButton as BrowseProductFamilyMenuButton;
                        var roundedButton = new RoundedButton
                        {
                            DataContext = browseMenuButton,
                            Style = GetStyle(this, "classic_menu_button_style"),
                            CornerRadius = 28.0,
                            Width = browseMenuButton.Width,
                            Height = browseMenuButton.Height,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(5.0, 0.0, 5.0, 0.0)
                        };
                        roundedButton.Click += MenuButton_Click;
                        roundedButton.SetBinding(IsEnabledProperty, new Binding
                        {
                            Path = new PropertyPath("IsButtonEnabled"),
                            Source = browseMenuButton
                        });
                        var textBlock = new TextBlock
                        {
                            Text = browseMenuButton.Text,
                            Style = GetStyle(this, browseMenuButton.TextStyleName),
                            TextWrapping = TextWrapping.Wrap,
                            TextAlignment = TextAlignment.Center
                        };
                        textBlock.SetBinding(TextBlock.TextProperty, new Binding
                        {
                            Path = new PropertyPath("Text"),
                            Source = browseMenuButton
                        });
                        StackPanel stackPanel = null;
                        if (browseProductFamilyMenuButton != null)
                        {
                            var flag = browseProductFamilyMenuButton.ArrowDirection == ArrowDirection.Right;
                            stackPanel = new StackPanel
                            {
                                Orientation = Orientation.Horizontal
                            };
                            var userControl = new UserControl
                            {
                                Style = GetStyle(this, flag ? "right_arrow" : "left_arrow"),
                                Margin = flag ? new Thickness(10.0, 0.0, 0.0, 0.0) : new Thickness(0.0, 0.0, 10.0, 0.0),
                                VerticalAlignment = VerticalAlignment.Center
                            };
                            var uielement = flag ? textBlock : (UIElement)userControl;
                            var uielement2 = flag ? userControl : (UIElement)textBlock;
                            stackPanel.Children.Add(uielement);
                            stackPanel.Children.Add(uielement2);
                        }

                        if (browseMenuButton is SignOutMenuButton)
                        {
                            roundedButton.Style = GetStyle(this, "transparent_button_style");
                            textBlock.Foreground = new SolidColorBrush
                            {
                                Color = (Color)FindResource("RubineRed")
                            };
                            textBlock.TextDecorations = TextDecorations.Underline;
                        }

                        if (browseMenuButton.FixedPosition)
                            roundedButton.Margin = new Thickness(browseMenuButton.X, browseMenuButton.Y, 0.0, 0.0);
                        roundedButton.Content = stackPanel != null ? stackPanel : (object)textBlock;
                        if (browseMenuButton.FixedPosition)
                        {
                            FixedButtonContainer.Children.Add(roundedButton);
                        }
                        else
                        {
                            NonFixedButtonContainer.ColumnDefinitions.Add(new ColumnDefinition
                            {
                                Width = new GridLength(browseMenuButton.Width + 15)
                            });
                            var stackPanel2 = new StackPanel
                            {
                                Orientation = Orientation.Horizontal
                            };
                            stackPanel2.Children.Add(roundedButton);
                            if (browseMenuButton.IsSelected)
                            {
                                roundedButton.Style = GetStyle(this, "classic_menu_button_style_disabled");
                                stackPanel2.Children.Add(new Image
                                {
                                    Source = new BitmapImage(new Uri(
                                        "pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/purple-white-checkmark-lg.png")),
                                    Width = 22.0,
                                    Height = 22.0,
                                    Margin = new Thickness(-19.0, -35.0, 0.0, 0.0)
                                });
                            }

                            var num = topMenuModel.MenuButtons.IndexOf(browseMenuButton);
                            Grid.SetColumn(stackPanel2, num);
                            Grid.SetRow(stackPanel2, 0);
                            NonFixedButtonContainer.Children.Add(stackPanel2);
                        }

                        if (theme != null) theme.SetStyle(roundedButton);
                    }
                }
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            RunCommandOnce(delegate
            {
                try
                {
                    var roundedButton = e.OriginalSource as RoundedButton;
                    if (roundedButton != null)
                    {
                        var browseMenuButton = roundedButton.DataContext as BrowseMenuButton;
                        if (browseMenuButton != null)
                        {
                            var browseTopMenuModel = BrowseTopMenuModel;
                            if (browseTopMenuModel != null)
                                browseTopMenuModel.ProcessOnBrowseMenuButtonClicked(browseMenuButton);
                            Commands.ResetIdleTimerCommand.Execute(null, this);
                        }
                    }
                }
                finally
                {
                    CompleteRunOnce();
                }
            });
        }

        private void AZButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RunCommandOnce(delegate
            {
                try
                {
                    var browseTopMenuModel = BrowseTopMenuModel;
                    if (browseTopMenuModel != null) browseTopMenuModel.ProcessOnAZButtonClicked();
                }
                finally
                {
                    CompleteRunOnce();
                }
            });
        }
    }
}