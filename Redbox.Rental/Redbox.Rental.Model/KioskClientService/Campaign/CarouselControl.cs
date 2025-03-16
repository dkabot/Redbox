namespace Redbox.Rental.Model.KioskClientService.Campaign
{
    internal class CarouselControl : Control, ICarouselControl, IControl
    {
        public CarouselControl()
        {
            ControlType = ControlType.Carousel;
        }

        public int MaxTitles { get; internal set; }
    }
}