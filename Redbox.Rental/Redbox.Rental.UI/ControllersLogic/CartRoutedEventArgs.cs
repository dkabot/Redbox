using System.Windows;

namespace Redbox.Rental.UI.ControllersLogic
{
    public class CartRoutedEventArgs : RoutedEventArgs
    {
        public CartRoutedEventArgs(RoutedEvent routedEvent)
        {
            RoutedEvent = routedEvent;
        }

        public string ButtonName { get; set; }

        public dynamic Parameter { get; set; }
    }
}