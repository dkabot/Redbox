using System.Windows.Controls;
using System.Windows.Input;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls
{
    public partial class BrowseMessagePopupUserControl : UserControl
    {
        public BrowseMessagePopupUserControl()
        {
            InitializeComponent();
        }

        private BrowseMessagePopupModel BrowseMessagePopupModel => DataContext as BrowseMessagePopupModel;

        private void BrowseMessagePopupInfoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var browseMessagePopupModel = BrowseMessagePopupModel;
            if (browseMessagePopupModel == null) return;
            browseMessagePopupModel.ProcessOnBrowseMessagePopupInfoClicked();
        }

        private void BrowseMessagePopupCloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var browseMessagePopupModel = BrowseMessagePopupModel;
            if (browseMessagePopupModel == null) return;
            browseMessagePopupModel.ProcessOnBrowseMessagePopupCloseClicked();
        }
    }
}