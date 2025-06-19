using System.Windows;
using System.Windows.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls
{
    public partial class DisplayProductWithCommandsUserControl : UserControl
    {
        private DisplayProductModel _displayProductModel;

        public DisplayProductWithCommandsUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _displayProductModel = DataContext as DisplayProductModel;
            MainGrid.Visibility = _displayProductModel != null ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}