using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Redbox.KioskEngine.ComponentModel
{
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
  public class KernelFunctionAttribute : Attribute
  {
    public static ReadOnlyCollection<KernelFunctionInfo> GetKernelFunctionInfos(
      Assembly assembly,
      string extension)
    {
      List<KernelFunctionInfo> kernelFunctionInfoList = new List<KernelFunctionInfo>();
      foreach (Type type in assembly.GetTypes())
      {
        foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
        {
          DescriptionAttribute customAttribute1 = (DescriptionAttribute) Attribute.GetCustomAttribute((MemberInfo) method, typeof (DescriptionAttribute));
          ObsoleteAttribute customAttribute2 = (ObsoleteAttribute) Attribute.GetCustomAttribute((MemberInfo) method, typeof (ObsoleteAttribute));
          KernelFunctionAttribute[] customAttributes = (KernelFunctionAttribute[]) Attribute.GetCustomAttributes((MemberInfo) method, typeof (KernelFunctionAttribute));
          if (customAttributes != null && customAttributes.Length != 0)
          {
            KernelFunctionInfo kernelFunctionInfo = new KernelFunctionInfo()
            {
              Method = method,
              Extension = extension,
              Description = customAttribute1?.Description,
              DeprecationWarning = customAttribute2?.Message
            };
            kernelFunctionInfo.InnerAttributes.AddRange((IEnumerable<KernelFunctionAttribute>) customAttributes);
            kernelFunctionInfoList.Add(kernelFunctionInfo);
          }
        }
      }
      return kernelFunctionInfoList.AsReadOnly();
    }

    public string GetNamespace()
    {
      StringBuilder stringBuilder = new StringBuilder();
      if (!string.IsNullOrEmpty(this.Name))
      {
        string[] strArray = this.Name.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        for (int index = 0; index < strArray.Length - 1; ++index)
        {
          if (index > 0)
            stringBuilder.Append(".");
          stringBuilder.Append(strArray[index]);
        }
      }
      return stringBuilder.ToString();
    }

    public string Name { get; set; }

    public string ExtensionName { get; set; }
  }
}
