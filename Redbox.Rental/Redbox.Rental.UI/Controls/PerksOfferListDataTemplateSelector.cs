using System.Windows;
using System.Windows.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls
{
    public class PerksOfferListDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var frameworkElement = container as FrameworkElement;
            if (frameworkElement != null && item != null && item is StoredPromoCodeModel)
                return frameworkElement.FindResource("StoredPromoCodeDataTemplate") as DataTemplate;
            if (frameworkElement == null || item == null || !(item is PerksOfferListItemModel)) return null;
            if (((PerksOfferListItemModel)item).OfferStatus == "Completed")
                return frameworkElement.FindResource("CompletedOfferDataTemplate") as DataTemplate;
            return frameworkElement.FindResource("ActiveOfferDataTemplate") as DataTemplate;
        }
    }
}