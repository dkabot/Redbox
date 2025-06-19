using System.Windows.Controls;

namespace Redbox.Rental.UI.Controls
{
    public partial class OfferInfoUserControl : UserControl
    {
        public OfferInfoUserControl()
        {
            InitializeComponent();
            TransparentContinueButton.IsAnimated = false;
        }
    }
}