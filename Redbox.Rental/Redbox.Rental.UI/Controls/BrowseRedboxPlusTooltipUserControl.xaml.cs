using System.Windows.Controls;
using System.Windows.Input;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls
{
    public partial class BrowseRedboxPlusTooltipUserControl : UserControl
    {
        public BrowseRedboxPlusTooltipUserControl()
        {
            InitializeComponent();
        }

        private BrowseRedboxPlusTooltipModel BrowseRedboxPlusTooltipModel =>
            DataContext as BrowseRedboxPlusTooltipModel;

        private void BrowseRedboxPlusTooltipCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var browseRedboxPlusTooltipModel = BrowseRedboxPlusTooltipModel;
            if (browseRedboxPlusTooltipModel == null) return;
            browseRedboxPlusTooltipModel.ProcessOnBrowseRedboxPlusTooltipUserControlClicked(
                BrowseRedboxPlusTooltipModel);
        }
    }
}