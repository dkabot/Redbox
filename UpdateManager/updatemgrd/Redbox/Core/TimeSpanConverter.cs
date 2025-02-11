using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Redbox.Core
{
    internal class TimeSpanConverter : JavaScriptConverter
    {
        private const string TimeSpanKey = "theTimeSpan";

        public override IEnumerable<Type> SupportedTypes
        {
            get
            {
                return (IEnumerable<Type>)new List<Type>()
        {
          typeof (TimeSpan),
          typeof (TimeSpan?)
        };
            }
        }

        public override IDictionary<string, object> Serialize(
          object obj,
          JavaScriptSerializer serializer)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            if (obj.GetType() == typeof(TimeSpan))
            {
                dictionary["theTimeSpan"] = (object)((TimeSpan)obj).Ticks;
                return (IDictionary<string, object>)dictionary;
            }
            if (obj.GetType() == typeof(DateTime?))
            {
                TimeSpan? nullable = (TimeSpan?)obj;
                if (nullable.HasValue)
                    dictionary["theTimeSpan"] = (object)nullable.Value.Ticks;
            }
            return (IDictionary<string, object>)dictionary;
        }

        public override object Deserialize(
          IDictionary<string, object> dictionary,
          Type type,
          JavaScriptSerializer serializer)
        {
            return dictionary.ContainsKey("theTimeSpan") ? (object)TimeSpan.FromTicks(long.Parse(dictionary["theTimeSpan"].ToString())) : (object)null;
        }
    }
}
