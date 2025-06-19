using System.Windows.Input;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls
{
    public partial class DisplayProductMoreLikeThisUserControl : TextToSpeechUserControl
    {
        public DisplayProductMoreLikeThisUserControl()
        {
            InitializeComponent();
        }

        private DisplayProductMoreLikeThisModel DisplayProductMoreLikeThisModel =>
            DataContext as DisplayProductMoreLikeThisModel;

        private void MoreLikeThis_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RunCommandOnce(delegate
            {
                try
                {
                    var displayProductMoreLikeThisUserControl =
                        e.OriginalSource as DisplayProductMoreLikeThisUserControl;
                    var browseItemModel =
                        (displayProductMoreLikeThisUserControl != null
                            ? displayProductMoreLikeThisUserControl.DataContext
                            : null) as IBrowseItemModel;
                    var displayProductMoreLikeThisModel = DisplayProductMoreLikeThisModel;
                    if (displayProductMoreLikeThisModel != null)
                        displayProductMoreLikeThisModel.ProcessMoreLikeThisSelected(browseItemModel);
                    HandleWPFHit();
                }
                finally
                {
                    CompleteRunOnce();
                }
            });
        }
    }
}