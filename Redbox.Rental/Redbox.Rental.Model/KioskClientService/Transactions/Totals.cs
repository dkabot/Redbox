using System;

namespace Redbox.Rental.Model.KioskClientService.Transactions
{
    public class Totals
    {
        public decimal TaxRate { get; set; }

        public decimal Subtotal { get; set; }

        public decimal DiscountedSubtotal { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal GrandTotal { get; set; }

        public override string ToString()
        {
            return string.Format(
                "TaxRate: {0:P}, Subtotal: {1:C2}, DiscountedSubtotal: {2:C2}, TaxAmount: {3:C2}, GrandTotal: {4:C2}",
                (object)(TaxRate / 100M), (object)Subtotal, (object)DiscountedSubtotal, (object)TaxAmount,
                (object)GrandTotal);
        }
    }
}