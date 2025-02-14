using System;
using System.Reflection;
using Redbox.Core;

namespace Redbox.KioskEngine.ComponentModel.KioskServices
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ResponseCodeAttribute : Attribute
    {
        public string Name { get; set; }

        public static object GetFieldsForResponseCode(Type type, string code)
        {
            foreach (var field in ConversionHelper.GetNullableTypeIfWrapped(type).GetFields())
                if (GetResponseCodes(field, code) != null)
                    return field.GetValue(null);
            return null;
        }

        private static ResponseCodeAttribute GetResponseCodes(MemberInfo fieldInfo, string code)
        {
            var customAttributes =
                (ResponseCodeAttribute[])GetCustomAttributes(fieldInfo, typeof(ResponseCodeAttribute));
            foreach (var responseCodes in customAttributes)
                if (string.Compare(responseCodes.Name, code, true) == 0)
                    return responseCodes;
            foreach (var responseCodes in customAttributes)
                if (responseCodes.Name == "*")
                    return responseCodes;
            return null;
        }
    }
}