using Redbox.KioskEngine.ComponentModel.KioskServices;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Redbox.KioskEngine.ComponentModel
{
  public static class EnumExtensions
  {
    public static string GetDisplayName(this Enum val)
    {
      MemberInfo element = ((IEnumerable<MemberInfo>) val.GetType().GetMember(val.ToString())).FirstOrDefault<MemberInfo>();
      return ((object) element != null ? element.GetCustomAttribute<DisplayAttribute>(false)?.Name : (string) null) ?? val.ToString();
    }

    public static string GetDisplayDescription(this Enum val)
    {
      MemberInfo element = ((IEnumerable<MemberInfo>) val.GetType().GetMember(val.ToString())).FirstOrDefault<MemberInfo>();
      return ((object) element != null ? element.GetCustomAttribute<DisplayAttribute>(false)?.Description : (string) null) ?? val.ToString();
    }

    public static string GetResponseCodeAttributeName(this Enum val)
    {
      MemberInfo element = ((IEnumerable<MemberInfo>) val.GetType().GetMember(val.ToString())).FirstOrDefault<MemberInfo>();
      return ((object) element != null ? element.GetCustomAttribute<ResponseCodeAttribute>(false)?.Name : (string) null) ?? val.ToString();
    }
  }
}
