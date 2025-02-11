using Newtonsoft.Json;

namespace Redbox.NetCore.Middleware.Extensions
{
    public static class JsonExtensions
    {
        public static string ToJsonIndented(this object obj)
        {
            return JsonConvert.SerializeObject(obj, (Formatting)1, new JsonSerializerSettings
            {
                NullValueHandling = (NullValueHandling)1
            });
        }

        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj, 0, new JsonSerializerSettings
            {
                NullValueHandling = (NullValueHandling)1
            });
        }
    }
}