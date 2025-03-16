using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Products
{
    public class ProductGroupSortOrder
    {
        public ProductGroupSortOrder()
        {
            SortOrders = new List<SortOrder>();
        }

        public int SortOrderType { get; set; }

        public List<SortOrder> SortOrders { get; set; }
    }
}