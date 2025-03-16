using Redbox.Core;
using Redbox.Rental.Model.KioskProduct;
using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Pricing
{
    public class PricingRecord : IPricingRecord
    {
        public TitleFamily TitleFamily { get; set; }

        public TitleType TitleType { get; set; }

        public decimal InitialNight { get; set; }

        public decimal ExtraNight { get; set; }

        public decimal ExpirationPrice { get; set; }

        public decimal NonReturn { get; set; }

        public decimal NonReturnDays { get; set; }

        public decimal PromoValue { get; set; }

        public long? ProductPricingId { get; set; }

        public int? InitialDays { get; set; }

        public decimal? DefaultInitialNightPrice { get; set; }

        public decimal? DefaultExtraNightPrice { get; set; }

        public int? PriceSetId { get; set; }

        public decimal Purchase { get; set; }

        public bool IsMultiNightPrice
        {
            get
            {
                if (!InitialDays.HasValue)
                    return false;
                var initialDays = InitialDays;
                var num = 1;
                return (initialDays.GetValueOrDefault() > num) & initialDays.HasValue;
            }
        }

        public bool IsDeal
        {
            get
            {
                if (IsMultiNightPrice || !DefaultInitialNightPrice.HasValue)
                    return false;
                var initialNightPrice = DefaultInitialNightPrice;
                var initialNight = InitialNight;
                var nullable = initialNightPrice.HasValue
                    ? new decimal?(initialNightPrice.GetValueOrDefault() - initialNight)
                    : new decimal?();
                var num = 0M;
                return (nullable.GetValueOrDefault() > num) & nullable.HasValue;
            }
        }

        public bool IsEqual(IPricingRecord pricingRecord)
        {
            return CompareTo(pricingRecord) == 0;
        }

        public int CompareTo(IPricingRecord pricingRecord)
        {
            var num1 = TitleFamily.CompareTo((object)pricingRecord.TitleFamily);
            if (num1 == 0)
                num1 = TitleType.CompareTo((object)pricingRecord.TitleType);
            decimal num2;
            if (num1 == 0)
            {
                num2 = InitialNight;
                num1 = num2.CompareTo(pricingRecord.InitialNight);
            }

            if (num1 == 0)
            {
                num2 = ExtraNight;
                num1 = num2.CompareTo(pricingRecord.ExtraNight);
            }

            if (num1 == 0)
            {
                num2 = ExpirationPrice;
                num1 = num2.CompareTo(pricingRecord.ExpirationPrice);
            }

            if (num1 == 0)
            {
                num2 = NonReturn;
                num1 = num2.CompareTo(pricingRecord.NonReturn);
            }

            if (num1 == 0)
            {
                num2 = NonReturnDays;
                num1 = num2.CompareTo(pricingRecord.NonReturnDays);
            }

            if (num1 == 0)
            {
                num2 = PromoValue;
                num1 = num2.CompareTo(pricingRecord.PromoValue);
            }

            if (num1 == 0)
            {
                num2 = Purchase;
                num1 = num2.CompareTo(pricingRecord.Purchase);
            }

            if (num1 == 0)
                num1 = ProductPricingId.GetValueOrDefault(-999L)
                    .CompareTo(pricingRecord.ProductPricingId.GetValueOrDefault(-999L));
            int? nullable;
            if (num1 == 0)
            {
                var valueOrDefault1 = InitialDays.GetValueOrDefault(-999);
                ref var local = ref valueOrDefault1;
                nullable = pricingRecord.InitialDays;
                var valueOrDefault2 = nullable.GetValueOrDefault(-999);
                num1 = local.CompareTo(valueOrDefault2);
            }

            if (num1 == 0)
            {
                num2 = DefaultInitialNightPrice.GetValueOrDefault(-999M);
                num1 = num2.CompareTo(pricingRecord.DefaultInitialNightPrice.GetValueOrDefault(-999M));
            }

            if (num1 == 0)
            {
                num2 = DefaultExtraNightPrice.GetValueOrDefault(-999M);
                num1 = num2.CompareTo(pricingRecord.DefaultExtraNightPrice.GetValueOrDefault(-999M));
            }

            if (num1 == 0)
            {
                nullable = PriceSetId;
                var valueOrDefault3 = nullable.GetValueOrDefault(-999);
                ref var local = ref valueOrDefault3;
                nullable = pricingRecord.PriceSetId;
                var valueOrDefault4 = nullable.GetValueOrDefault(-999);
                num1 = local.CompareTo(valueOrDefault4);
            }

            return num1;
        }

        public IPricingRecord Clone()
        {
            return (IPricingRecord)new PricingRecord()
            {
                TitleFamily = TitleFamily,
                TitleType = TitleType,
                InitialNight = InitialNight,
                ExtraNight = ExtraNight,
                ExpirationPrice = ExpirationPrice,
                NonReturn = NonReturn,
                NonReturnDays = NonReturnDays,
                PromoValue = PromoValue,
                Purchase = Purchase,
                ProductPricingId = ProductPricingId,
                InitialDays = InitialDays,
                DefaultInitialNightPrice = DefaultInitialNightPrice,
                DefaultExtraNightPrice = DefaultExtraNightPrice,
                PriceSetId = PriceSetId
            };
        }

        public Dictionary<string, object> ToDictionary()
        {
            var dictionary = new Dictionary<string, object>();
            dictionary.Add("family", (object)TitleFamily.ToString().ToLower());
            dictionary.Add("type",
                (object)TitleType.ToString().ToLower().Replace("bluray", "blu-ray").Replace("all", "*"));
            dictionary.Add("initial_night", (object)InitialNight);
            dictionary.Add("extra_night", (object)ExtraNight);
            dictionary.Add("expiration_price", (object)ExpirationPrice);
            dictionary.Add("non_return", (object)NonReturn);
            dictionary.Add("non_return_days", (object)NonReturnDays);
            dictionary.Add("promo_value", (object)PromoValue);
            dictionary.Add("purchase", (object)Purchase);
            dictionary.Add("product_price_id", (object)ProductPricingId);
            dictionary.Add("initial_days", (object)InitialDays);
            dictionary.Add("default_initial_night", (object)DefaultInitialNightPrice);
            dictionary.Add("default_extra_night", (object)DefaultExtraNightPrice);
            ServiceLocator.Instance.GetService<IPricingService>();
            dictionary.Add("price_set_id", (object)"1");
            return dictionary;
        }

        public class Constants
        {
            public const string PriceSetId = "price_set_id";
            public const string Family = "family";
            public const string Type = "type";
            public const string InitialNight = "initial_night";
            public const string ExtraNight = "extra_night";
            public const string ExpirationlPrice = "expiration_price";
            public const string NonReturn = "non_return";
            public const string NonReturnDays = "non_return_days";
            public const string PromoValue = "promo_value";
            public const string Purchase = "purchase";
            public const string ProductPricingId = "product_price_id";
            public const string InitialDays = "initial_days";
            public const string DefaultInitialNightPrice = "default_initial_night";
            public const string DefaultExtraNightPrice = "default_extra_night";
        }
    }
}