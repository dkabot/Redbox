using System.Collections.Generic;
using System.Globalization;
using Redbox.NetCore.Middleware.Abstractions;
using Redbox.NetCore.Middleware.Extensions;

namespace Redbox.NetCore.Middleware.Http
{
    public abstract class BaseHttpClient
    {
        public BaseHttpClient(IMiddlewareData middlewareData)
        {
            if (!string.IsNullOrEmpty(middlewareData.ActivityId))
                ActivityId = middlewareData.ActivityId;
            KioskId = middlewareData.KioskId;
        }

        public string ActivityId { get; }

        public long? KioskId { get; }

        protected List<Header> CreateHeaders(string bearer = null, string apikey = null, string applicationName = null)
        {
            var headers1 = new List<Header>();
            if (bearer != null)
            {
                if (!bearer.StartsWith(nameof(bearer), true, CultureInfo.InvariantCulture))
                    bearer = "Bearer " + bearer;
                headers1.AddAuthorizationHeader(bearer);
            }

            if (apikey != null)
                headers1.AddApiKeyHeader(apikey);
            if (applicationName != null)
                headers1.AddAppNameHeader(applicationName);
            if (!string.IsNullOrEmpty(ActivityId))
                headers1.AddActivityIdHeader(ActivityId);
            var kioskId1 = KioskId;
            if (kioskId1.HasValue)
            {
                var headers2 = headers1;
                kioskId1 = KioskId;
                var kioskId2 = kioskId1.Value;
                headers2.AddKioskIdHeader(kioskId2);
            }

            return headers1;
        }
    }
}