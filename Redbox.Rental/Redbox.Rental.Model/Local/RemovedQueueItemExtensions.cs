using Redbox.Core;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Local
{
    public static class RemovedQueueItemExtensions
    {
        public static IEnumerable<IInventoryItem> ToInventoryItems(
            this IEnumerable<RemovedQueueItem> source)
        {
            List<IInventoryItem> inventoryItems;
            if (source == null)
            {
                inventoryItems = (List<IInventoryItem>)null;
            }
            else
            {
                inventoryItems = new List<IInventoryItem>();
                var service = ServiceLocator.Instance.GetService<IInventoryService>();
                foreach (var removedQueueItem in source)
                    inventoryItems.Add(service.CreateInventoryItem(removedQueueItem.Barcode, new int?(), new int?(),
                        InventoryItemStatus.Removed));
            }

            return (IEnumerable<IInventoryItem>)inventoryItems;
        }
    }
}