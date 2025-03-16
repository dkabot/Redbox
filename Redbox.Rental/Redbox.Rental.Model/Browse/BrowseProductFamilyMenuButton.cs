using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model.Browse
{
    public class BrowseProductFamilyMenuButton : BrowseMenuButton
    {
        public BrowseProductFamilyMenuButton()
        {
            Height = 56;
            Width = 184;
            TextStyleName = "font_montserrat_extrabold_14_correct";
        }

        public TitleFamily FilterValue { get; set; }

        public ArrowDirection ArrowDirection { get; set; }
    }
}