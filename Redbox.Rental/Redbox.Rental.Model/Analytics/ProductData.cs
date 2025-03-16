using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model.Analytics
{
    public class ProductData : AnalyticsData
    {
        public ProductData()
        {
            DataType = "Product";
        }

        public string ViewName { get; set; }

        public long? ProductId { get; set; }

        public long? TitleRollupProductGroupId { get; set; }

        public int? ProductTypeId { get; set; }

        public string TivoQueryId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TitleFamily? TitleFamily { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TitleType? TitleType { get; set; }

        public string SubscriptionId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public SubscriptionType? SubscriptionType { get; set; }

        public static ProductData ToProductData(long productGroupId)
        {
            return new ProductData()
            {
                TitleRollupProductGroupId = new long?(productGroupId)
            };
        }

        public static ProductData ToProductData(ITitleRollupProduct titleRollupProduct)
        {
            if (titleRollupProduct == null)
                return (ProductData)null;
            return new ProductData()
            {
                ProductId = new long?(titleRollupProduct.ProductId),
                TitleRollupProductGroupId = new long?(titleRollupProduct.TitleRollupProductGroupId),
                ProductTypeId = new int?(titleRollupProduct.ProductTypeId),
                TitleFamily = new TitleFamily?(titleRollupProduct.TitleFamily)
            };
        }

        public static ProductData ToProductData(ITitleProduct titleProduct)
        {
            if (titleProduct == null)
                return (ProductData)null;
            return new ProductData()
            {
                ProductId = new long?(titleProduct.ProductId),
                TitleRollupProductGroupId = new long?(titleProduct.TitleRollupProductGroupId),
                ProductTypeId = new int?(titleProduct.ProductTypeId),
                TitleFamily = new TitleFamily?(titleProduct.TitleFamily),
                TitleType = new TitleType?(titleProduct.TitleType),
                TivoQueryId = titleProduct.TivoQueryId
            };
        }

        public static ProductData ToProductData(ISubscriptionProduct subscriptionProduct)
        {
            var productData = (ProductData)null;
            if (subscriptionProduct != null)
                productData = new ProductData()
                {
                    SubscriptionId = subscriptionProduct.SubscriptionId,
                    SubscriptionType = new SubscriptionType?(subscriptionProduct.SubscriptionType)
                };
            return productData;
        }

        public static ProductData ToProductData(IBaseRentalShoppingCartItem rentalShoppingCartItem)
        {
            var shoppingCartTitleItem = rentalShoppingCartItem as IRentalShoppingCartTitleItem;
            var subscriptionItem = rentalShoppingCartItem as IRentalShoppingCartSubscriptionItem;
            if (shoppingCartTitleItem != null)
                return new ProductData()
                {
                    ProductId = new long?(shoppingCartTitleItem.ProductId),
                    TitleFamily = new TitleFamily?(shoppingCartTitleItem.TitleFamily),
                    TitleType = new TitleType?(shoppingCartTitleItem.TitleType)
                };
            if (subscriptionItem == null)
                return (ProductData)null;
            return new ProductData()
            {
                SubscriptionId = subscriptionItem.SubscriptionProduct.SubscriptionId,
                SubscriptionType = new SubscriptionType?(subscriptionItem.SubscriptionProduct.SubscriptionType)
            };
        }
    }
}