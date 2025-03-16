using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Redbox.Rental.Model.Ads
{
    public class GetAdTimeoutEvent
    {
        public DateTime CreatedOn => DateTime.UtcNow;

        [JsonConverter(typeof(StringEnumConverter))]
        public AdLocation AdLocation { get; }

        public int TimeoutSetting { get; }

        public GetAdTimeoutEvent(AdLocation adLocation, int timeoutSetting)
        {
            AdLocation = adLocation;
            TimeoutSetting = timeoutSetting;
        }
    }
}