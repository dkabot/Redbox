using System.Windows;
using System.Windows.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls
{
    public partial class DisplayProductAToZUserControl : UserControl
    {
        private DisplayGroupModel _displayGroupModel;

        private DisplayProductModel _displayProductModel;

        public DisplayProductAToZUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _displayProductModel = DataContext as DisplayProductModel;
            _displayGroupModel = DataContext as DisplayGroupModel;
            MainGrid.Visibility = _displayProductModel != null || _displayGroupModel != null
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}