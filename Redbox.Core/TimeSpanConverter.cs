using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Redbox.Core
{
    internal class TimeSpanConverter : JavaScriptConverter
    {
        private const string TimeSpanKey = "theTimeSpan";

        public override IEnumerable<Type> SupportedTypes =>
            new List<Type>
            {
                typeof(TimeSpan),
                typeof(TimeSpan?)
            };

        public override IDictionary<string, object> Serialize(
            object obj,
            JavaScriptSerializer serializer)
        {
            var dictionary = new Dictionary<string, object>();
            if (obj.GetType() == typeof(TimeSpan))
            {
                dictionary["theTimeSpan"] = ((TimeSpan)obj).Ticks;
                return dictionary;
            }

            if (obj.GetType() == typeof(DateTime?))
            {
                var nullable = (TimeSpan?)obj;
                if (nullable.HasValue)
                    dictionary["theTimeSpan"] = nullable.Value.Ticks;
            }

            return dictionary;
        }

        public override object Deserialize(
            IDictionary<string, object> dictionary,
            Type type,
            JavaScriptSerializer serializer)
        {
            return dictionary.ContainsKey("theTimeSpan")
                ? TimeSpan.FromTicks(long.Parse(dictionary["theTimeSpan"].ToString()))
                : (object)null;
        }
    }
}