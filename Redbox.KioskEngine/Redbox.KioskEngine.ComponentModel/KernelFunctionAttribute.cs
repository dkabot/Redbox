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
        public string Name { get; set; }

        public string ExtensionName { get; set; }

        public static ReadOnlyCollection<KernelFunctionInfo> GetKernelFunctionInfos(
            Assembly assembly,
            string extension)
        {
            var kernelFunctionInfoList = new List<KernelFunctionInfo>();
            foreach (var type in assembly.GetTypes())
            foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var customAttribute1 = (DescriptionAttribute)GetCustomAttribute(method, typeof(DescriptionAttribute));
                var customAttribute2 = (ObsoleteAttribute)GetCustomAttribute(method, typeof(ObsoleteAttribute));
                var customAttributes =
                    (KernelFunctionAttribute[])GetCustomAttributes(method, typeof(KernelFunctionAttribute));
                if (customAttributes != null && customAttributes.Length != 0)
                {
                    var kernelFunctionInfo = new KernelFunctionInfo
                    {
                        Method = method,
                        Extension = extension,
                        Description = customAttribute1?.Description,
                        DeprecationWarning = customAttribute2?.Message
                    };
                    kernelFunctionInfo.InnerAttributes.AddRange(customAttributes);
                    kernelFunctionInfoList.Add(kernelFunctionInfo);
                }
            }

            return kernelFunctionInfoList.AsReadOnly();
        }

        public string GetNamespace()
        {
            var stringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(Name))
            {
                var strArray = Name.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                for (var index = 0; index < strArray.Length - 1; ++index)
                {
                    if (index > 0)
                        stringBuilder.Append(".");
                    stringBuilder.Append(strArray[index]);
                }
            }

            return stringBuilder.ToString();
        }
    }
}