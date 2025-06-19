using System.Windows;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models
{
    public class SortButtonModel : CheckMarkButtonModel
    {
        public SortButtonModel(Style unselectedStyle, Style selectedStyle)
            : base(unselectedStyle, selectedStyle)
        {
        }

        public BrowseSort Sort { get; set; }
    }
}