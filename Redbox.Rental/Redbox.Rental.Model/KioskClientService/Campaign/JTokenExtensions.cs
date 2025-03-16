using Newtonsoft.Json.Linq;
using System;

namespace Redbox.Rental.Model.KioskClientService.Campaign
{
    public static class JTokenExtensions
    {
        public static JToken GetCaseInsensitveKeyValue(this JToken source, string key)
        {
            return (source as JObject).GetValue(key, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}