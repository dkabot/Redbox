using System.Windows;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models
{
    public class RatingButtonModel : CheckMarkButtonModel
    {
        public RatingButtonModel(Style unselectedStyle, Style selectedStyle)
            : base(unselectedStyle, selectedStyle)
        {
        }

        public Ratings Rating { get; set; }
    }
}