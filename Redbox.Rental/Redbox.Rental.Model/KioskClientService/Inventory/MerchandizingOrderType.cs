using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;

namespace Redbox.Rental.Model.KioskClientService.Inventory
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MerchandizingOrderType
    {
        [Description("RebalancingExtract")] RebalancingExtract = 1,
        [Description("Fill")] Fill = 2,
        [Description("ThinningExtract")] ThinningExtract = 3
    }
}