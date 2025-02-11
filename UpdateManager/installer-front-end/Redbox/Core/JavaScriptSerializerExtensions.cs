using System;
using System.Web.Script.Serialization;

namespace Redbox.Core
{
    internal static class JavaScriptSerializerExtensions
    {
        public static object Deserialize(this JavaScriptSerializer serializer, Type type, string json)
        {
            return serializer.GetType().GetMethod(nameof(Deserialize), new Type[1]
            {
        typeof (string)
            }).MakeGenericMethod(type).Invoke((object)serializer, (object[])new string[1]
            {
        json
            });
        }
    }
}
