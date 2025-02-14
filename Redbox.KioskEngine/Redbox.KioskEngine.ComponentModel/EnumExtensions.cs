using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskEngine.ComponentModel
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum val)
        {
            var element = val.GetType().GetMember(val.ToString()).FirstOrDefault();
            return ((object)element != null ? element.GetCustomAttribute<DisplayAttribute>(false)?.Name : null) ??
                   val.ToString();
        }

        public static string GetDisplayDescription(this Enum val)
        {
            var element = val.GetType().GetMember(val.ToString()).FirstOrDefault();
            return ((object)element != null
                ? element.GetCustomAttribute<DisplayAttribute>(false)?.Description
                : null) ?? val.ToString();
        }

        public static string GetResponseCodeAttributeName(this Enum val)
        {
            var element = val.GetType().GetMember(val.ToString()).FirstOrDefault();
            return ((object)element != null ? element.GetCustomAttribute<ResponseCodeAttribute>(false)?.Name : null) ??
                   val.ToString();
        }
    }
}