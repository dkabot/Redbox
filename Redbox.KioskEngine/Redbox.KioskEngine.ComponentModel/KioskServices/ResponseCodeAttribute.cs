using Redbox.Core;
using System;
using System.Reflection;

namespace Redbox.KioskEngine.ComponentModel.KioskServices
{
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
  public class ResponseCodeAttribute : Attribute
  {
    public static object GetFieldsForResponseCode(Type type, string code)
    {
      foreach (FieldInfo field in ConversionHelper.GetNullableTypeIfWrapped(type).GetFields())
      {
        if (ResponseCodeAttribute.GetResponseCodes((MemberInfo) field, code) != null)
          return field.GetValue((object) null);
      }
      return (object) null;
    }

    public string Name { get; set; }

    private static ResponseCodeAttribute GetResponseCodes(MemberInfo fieldInfo, string code)
    {
      ResponseCodeAttribute[] customAttributes = (ResponseCodeAttribute[]) Attribute.GetCustomAttributes(fieldInfo, typeof (ResponseCodeAttribute));
      foreach (ResponseCodeAttribute responseCodes in customAttributes)
      {
        if (string.Compare(responseCodes.Name, code, true) == 0)
          return responseCodes;
      }
      foreach (ResponseCodeAttribute responseCodes in customAttributes)
      {
        if (responseCodes.Name == "*")
          return responseCodes;
      }
      return (ResponseCodeAttribute) null;
    }
  }
}
