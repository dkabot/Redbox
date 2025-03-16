using System.Collections.Generic;
using System.Text;

namespace Redbox.Rental.Model.KioskClientService.Transactions
{
    public class LineItemGroup
    {
        public LineItemGroupType GroupType { get; set; }

        public List<LineItem> Items { get; set; } = new List<LineItem>();

        public Totals Totals { get; set; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(string.Format("GroupType: {0}, ", (object)GroupType));
            if (Items != null)
                stringBuilder.Append("Items: [(" + string.Join<LineItem>("), (", (IEnumerable<LineItem>)Items) +
                                     ")], ");
            stringBuilder.Append(string.Format("Totals: ({0})", (object)Totals));
            return stringBuilder.ToString();
        }
    }
}