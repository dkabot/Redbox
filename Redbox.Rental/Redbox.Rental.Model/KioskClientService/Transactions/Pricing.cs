using System;
using System.Text;

namespace Redbox.Rental.Model.KioskClientService.Transactions
{
    public class Pricing
    {
        public decimal InitialNight { get; set; }

        public decimal ExtraNight { get; set; }

        public decimal NonReturn { get; set; }

        public decimal Purchase { get; set; }

        public decimal Expiration { get; set; }

        public int NonReturnDays { get; set; }

        public int PriceSetId { get; set; }

        public int? ProductPriceId { get; set; }

        public int? InitialDays { get; set; }

        public decimal? DefaultInitialNight { get; set; }

        public decimal? DefaultExtraNight { get; set; }

        public override string ToString()
        {
            var stringBuilder1 = new StringBuilder();
            stringBuilder1.Append(string.Format(
                "PriceSetId: {0}, InitialNight: {1:C2}, ExtraNight: {2:C2}, NonReturn: {3:C2}, NonReturnDays: {4}, Purchase: {5:C2}, Expiration: {6:C2}",
                (object)PriceSetId, (object)InitialNight, (object)ExtraNight, (object)NonReturn, (object)NonReturnDays,
                (object)Purchase, (object)Expiration));
            int? nullable1;
            if (ProductPriceId.HasValue)
            {
                var stringBuilder2 = stringBuilder1;
                nullable1 = ProductPriceId;
                var str = string.Format(", ProductPriceId: {0}", (object)nullable1.Value);
                stringBuilder2.Append(str);
            }

            nullable1 = InitialDays;
            if (nullable1.HasValue)
            {
                var stringBuilder3 = stringBuilder1;
                nullable1 = InitialDays;
                var str = string.Format(", InitialDays: {0}", (object)nullable1.Value);
                stringBuilder3.Append(str);
            }

            var nullable2 = DefaultInitialNight;
            if (nullable2.HasValue)
            {
                var stringBuilder4 = stringBuilder1;
                nullable2 = DefaultInitialNight;
                var str = string.Format(", DefaultInitialNight: {0:C2}", (object)nullable2.Value);
                stringBuilder4.Append(str);
            }

            nullable2 = DefaultExtraNight;
            if (nullable2.HasValue)
            {
                var stringBuilder5 = stringBuilder1;
                nullable2 = DefaultExtraNight;
                var str = string.Format(", DefaultExtraNight: {0:C2}", (object)nullable2.Value);
                stringBuilder5.Append(str);
            }

            return stringBuilder1.ToString();
        }
    }
}