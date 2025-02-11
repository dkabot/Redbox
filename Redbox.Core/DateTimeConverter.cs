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

        public override IEnumerable<Type> SupportedTypes =>
            new List<Type>
            {
                typeof(DateTime),
                typeof(DateTime?)
            };

        public override IDictionary<string, object> Serialize(
            object obj,
            JavaScriptSerializer serializer)
        {
            var dictionary1 = new Dictionary<string, object>();
            var nullable = obj is DateTime dateTime1 ? dateTime1 : (DateTime?)obj;
            if (nullable.HasValue)
            {
                var dictionary2 = dictionary1;
                var dateTime2 = nullable.Value;
                var ticks = dateTime2.Ticks;
                dictionary2["theDate"] = ticks;
                var dictionary3 = dictionary1;
                dateTime2 = nullable.Value;
                var str1 = dateTime2.Kind.ToString("G");
                dictionary3["dateTimeKind"] = str1;
                dateTime2 = nullable.Value;
                if (dateTime2.Kind == DateTimeKind.Local)
                {
                    ref var local = ref nullable;
                    dateTime2 = nullable.Value;
                    var dateTime3 = new DateTime(dateTime2.Ticks, DateTimeKind.Unspecified);
                    local = dateTime3;
                }

                var dictionary4 = dictionary1;
                dateTime2 = nullable.Value;
                var str2 = dateTime2.ToString("o");
                dictionary4["dateString"] = str2;
            }

            return dictionary1;
        }

        public override object Deserialize(
            IDictionary<string, object> dictionary,
            Type type,
            JavaScriptSerializer serializer)
        {
            if (dictionary.ContainsKey("dateString"))
                return DateTime.Parse(dictionary["dateString"].ToString(), null, DateTimeStyles.RoundtripKind);
            var kind = DateTimeKind.Unspecified;
            if (dictionary.ContainsKey("dateTimeKind"))
                kind = (DateTimeKind)Enum.Parse(typeof(DateTimeKind), dictionary["dateTimeKind"].ToString());
            return dictionary.ContainsKey("theDate")
                ? new DateTime(long.Parse(dictionary["theDate"].ToString()), kind)
                : (object)null;
        }
    }
}