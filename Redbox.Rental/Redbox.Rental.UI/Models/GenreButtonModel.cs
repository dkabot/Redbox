using System.Windows;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models
{
    public class GenreButtonModel : CheckMarkButtonModel
    {
        public GenreButtonModel(Style unselectedStyle, Style selectedStyle)
            : base(unselectedStyle, selectedStyle)
        {
        }

        public Genres Genre { get; set; }
    }
}