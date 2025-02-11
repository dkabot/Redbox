using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Script.Serialization;

namespace Redbox.Core
{
    internal class DateTimeConverter : JavaScriptConverter
    {
        private const string DateKey = "theDate";
        private const string KindKey = "dateTimeKind";
        private const string DateString = "dateString";

        public override IEnumerable<Type> SupportedTypes
        {
            get
            {
                return (IEnumerable<Type>)new List<Type>()
        {
          typeof (DateTime),
          typeof (DateTime?)
        };
            }
        }

        public override IDictionary<string, object> Serialize(
          object obj,
          JavaScriptSerializer serializer)
        {
            Dictionary<string, object> dictionary1 = new Dictionary<string, object>();
            DateTime? nullable = obj is DateTime dateTime1 ? new DateTime?(dateTime1) : (DateTime?)obj;
            if (nullable.HasValue)
            {
                Dictionary<string, object> dictionary2 = dictionary1;
                DateTime dateTime2 = nullable.Value;
                long ticks = dateTime2.Ticks;
                dictionary2["theDate"] = (object)ticks;
                Dictionary<string, object> dictionary3 = dictionary1;
                dateTime2 = nullable.Value;
                string str1 = dateTime2.Kind.ToString("G");
                dictionary3["dateTimeKind"] = (object)str1;
                dateTime2 = nullable.Value;
                if (dateTime2.Kind == DateTimeKind.Local)
                {
                    ref DateTime? local = ref nullable;
                    dateTime2 = nullable.Value;
                    DateTime dateTime3 = new DateTime(dateTime2.Ticks, DateTimeKind.Unspecified);
                    local = new DateTime?(dateTime3);
                }
                Dictionary<string, object> dictionary4 = dictionary1;
                dateTime2 = nullable.Value;
                string str2 = dateTime2.ToString("o");
                dictionary4["dateString"] = (object)str2;
            }
            return (IDictionary<string, object>)dictionary1;
        }

        public override object Deserialize(
          IDictionary<string, object> dictionary,
          Type type,
          JavaScriptSerializer serializer)
        {
            if (dictionary.ContainsKey("dateString"))
                return (object)DateTime.Parse(dictionary["dateString"].ToString(), (IFormatProvider)null, DateTimeStyles.RoundtripKind);
            DateTimeKind kind = DateTimeKind.Unspecified;
            if (dictionary.ContainsKey("dateTimeKind"))
                kind = (DateTimeKind)Enum.Parse(typeof(DateTimeKind), dictionary["dateTimeKind"].ToString());
            return dictionary.ContainsKey("theDate") ? (object)new DateTime(long.Parse(dictionary["theDate"].ToString()), kind) : (object)null;
        }
    }
}
