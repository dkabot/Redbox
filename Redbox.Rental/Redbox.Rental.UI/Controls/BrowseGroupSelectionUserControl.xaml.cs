using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls
{
    public partial class BrowseGroupSelectionUserControl : UserControl
    {
        private DisplayGroupSelectionModel _displayGroupSelectionModel;

        public BrowseGroupSelectionUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _displayGroupSelectionModel = DataContext as DisplayGroupSelectionModel;
            if (_displayGroupSelectionModel != null && _displayGroupSelectionModel.GroupSelectionItems != null)
            {
                GroupSelectionContainer.Children.Clear();
                GroupSelectionContainer.ColumnDefinitions.Clear();
                GroupSelectionContainer.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(3.0, GridUnitType.Star)
                });
                var num = 0;
                foreach (var displayGroupSelectionItemModel in _displayGroupSelectionModel.GroupSelectionItems)
                {
                    GroupSelectionContainer.ColumnDefinitions.Add(new ColumnDefinition
                    {
                        Width = new GridLength(10.0, GridUnitType.Star)
                    });
                    num++;
                    var userControl = new UserControl
                    {
                        DataContext = displayGroupSelectionItemModel
                    };
                    GroupSelectionContainer.Children.Add(userControl);
                    userControl.Style = GetStyle(this, "group_selection_item");
                    userControl.InputBindings.Add(new InputBinding(Commands.BrowseGroupSelectedCommand,
                        new MouseGesture(MouseAction.LeftClick)));
                    Grid.SetColumn(userControl, num);
                }

                GroupSelectionContainer.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(3.0, GridUnitType.Star)
                });
            }
        }

        private static Style GetStyle(FrameworkElement frameworkElement, string styleName)
        {
            Style style = null;
            if (!string.IsNullOrEmpty(styleName))
            {
                var obj = frameworkElement != null ? frameworkElement.FindResource(styleName) : null;
                if (obj != null) style = obj as Style;
            }

            return style;
        }

        private void BrowseGroupSelectedCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var displayGroupSelectionItemModel =
                (e.OriginalSource as FrameworkElement).DataContext as DisplayGroupSelectionItemModel;
            if (displayGroupSelectionItemModel != null)
            {
                var displayGroupSelectionModel = DataContext as DisplayGroupSelectionModel;
                if (displayGroupSelectionModel != null)
                {
                    displayGroupSelectionModel.ProcessOnGroupSelectionItemClicked(displayGroupSelectionItemModel);
                    Commands.ResetIdleTimerCommand.Execute(null, this);
                }
            }
        }
    }
}