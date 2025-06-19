namespace Redbox.Rental.UI.Views
{
    public static class SwipeViewTypeExtensions
    {
        public static bool IsReservation(this SwipeViewType source)
        {
            return source == SwipeViewType.ReservationPickup ||
                   source == SwipeViewType.EncryptedSwipeReservationPickup ||
                   source == SwipeViewType.EMVInsertTapReservationPickup ||
                   source == SwipeViewType.EMVInsertReservationPickup ||
                   source == SwipeViewType.EMVTapReservationPickup ||
                   source == SwipeViewType.ZeroTouchReservationPickup;
        }
    }
}