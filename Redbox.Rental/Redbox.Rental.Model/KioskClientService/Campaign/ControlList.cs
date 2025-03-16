using Newtonsoft.Json;
using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Campaign
{
    [JsonConverter(typeof(ControlListConverter))]
    public class ControlList : List<IControl>
    {
    }
}