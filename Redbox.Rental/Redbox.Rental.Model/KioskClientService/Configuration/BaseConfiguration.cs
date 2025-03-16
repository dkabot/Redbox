using Redbox.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Redbox.Rental.Model.KioskClientService.Configuration
{
    public class BaseConfiguration
    {
        public long ConfigurationVersion { get; set; }

        public void Log(ConfigLogParameters loggingParameters = null)
        {
            LogHelper.Instance.Log(string.Format("KioskConfiguration Settings for configuration version: {0}",
                (object)ConfigurationVersion));
            foreach (var property in GetType().GetProperties())
                if (property.PropertyType.IsSubclassOf(typeof(BaseCategorySetting)))
                    LogCategorySettingValues(GetCategoryClassInstance(property), loggingParameters);
        }

        private void LogCategorySettingValues(
            object categoryClassInstance,
            ConfigLogParameters loggingParameters = null)
        {
            if (categoryClassInstance == null)
                return;
            var categoryName = GetCategoryName(categoryClassInstance);
            foreach (var property in categoryClassInstance.GetType().GetProperties())
                if (property.CanWrite && isSettingIncluded(categoryClassInstance, property, loggingParameters))
                {
                    var settingName = GetSettingName(property);
                    var settingValue = property.GetValue(categoryClassInstance);
                    var obj = (object)MaskSettingValue(property, settingValue);
                    LogHelper.Instance.Log(string.Format("KioskConfiguration.{0}.{1} = {2}", (object)categoryName,
                        (object)settingName, obj));
                }
        }

        private bool isSettingIncluded(
            object categoryClassInstance,
            PropertyInfo settingProperty,
            ConfigLogParameters loggingParameters)
        {
            var categoryName = GetCategoryName(categoryClassInstance);
            var name1 = categoryClassInstance.GetType().Name;
            var name2 = settingProperty.Name;
            var settingName = GetSettingName(settingProperty);
            var flag1 = loggingParameters?.IncludeCategories == null ||
                        !loggingParameters.IncludeCategories.Any<string>();
            int num1;
            if (loggingParameters?.IncludeCategories != null)
                num1 = loggingParameters.IncludeCategories.Intersect<string>((IEnumerable<string>)new string[2]
                {
                    categoryName,
                    name1
                }).Any<string>()
                    ? 1
                    : 0;
            else
                num1 = 0;
            var flag2 = num1 != 0;
            int num2;
            if (loggingParameters?.ExcludeCategories != null)
                num2 = loggingParameters.ExcludeCategories.Intersect<string>((IEnumerable<string>)new string[2]
                {
                    categoryName,
                    name1
                }).Any<string>()
                    ? 1
                    : 0;
            else
                num2 = 0;
            var flag3 = num2 != 0;
            int num3;
            if (loggingParameters?.IncludeSettings != null)
                num3 = loggingParameters.IncludeSettings.Intersect<(string, string)>(
                    (IEnumerable<(string, string)>)new (string, string)[4]
                    {
                        (categoryName, settingName),
                        (categoryName, name2),
                        (name1, settingName),
                        (name1, name2)
                    }).Any<(string, string)>()
                    ? 1
                    : 0;
            else
                num3 = 0;
            var flag4 = num3 != 0;
            int num4;
            if (loggingParameters?.ExcludeSettings != null)
                num4 = loggingParameters.ExcludeSettings.Intersect<(string, string)>(
                    (IEnumerable<(string, string)>)new (string, string)[4]
                    {
                        (categoryName, settingName),
                        (categoryName, name2),
                        (name1, settingName),
                        (name1, name2)
                    }).Any<(string, string)>()
                    ? 1
                    : 0;
            else
                num4 = 0;
            var flag5 = num4 != 0;
            if (flag4)
                return true;
            return ((!flag1 ? 0 : !flag3 ? 1 : 0) | (flag2 ? 1 : 0)) != 0 && !flag5;
        }

        private string GetSettingName(PropertyInfo settingPropertyInfo)
        {
            var name = settingPropertyInfo.Name;
            var customAttribute =
                (SettingAttribute)Attribute.GetCustomAttribute((MemberInfo)settingPropertyInfo,
                    typeof(SettingAttribute));
            if (customAttribute != null)
                name = customAttribute.Name;
            return name;
        }

        private string MaskSettingValue(PropertyInfo settingPropertyInfo, object settingValue)
        {
            var str = string.Format("{0}", settingValue);
            var logValueAttribute = GetMaskLogValueAttribute(settingPropertyInfo);
            if (logValueAttribute != null && !string.IsNullOrEmpty(str))
            {
                var count = Math.Max(0, str.Length - logValueAttribute.VisibleChars);
                var startIndex = Math.Max(0, str.Length - logValueAttribute.VisibleChars);
                str = new string('X', count) + str.Substring(startIndex);
            }

            return str;
        }

        private MaskLogValue GetMaskLogValueAttribute(PropertyInfo propertyInfo)
        {
            return (MaskLogValue)Attribute.GetCustomAttribute((MemberInfo)propertyInfo, typeof(MaskLogValue));
        }

        private string GetCategoryName(object categoryClassInstance)
        {
            var name = categoryClassInstance.GetType().Name;
            var customAttribute =
                (CategoryAttribute)Attribute.GetCustomAttribute((MemberInfo)categoryClassInstance.GetType(),
                    typeof(CategoryAttribute));
            if (!string.IsNullOrEmpty(customAttribute?.Name))
                name = customAttribute?.Name;
            return name;
        }

        private object GetCategoryClassInstance(PropertyInfo categoryClassPropertyInfo)
        {
            var instance = categoryClassPropertyInfo?.GetValue((object)this, (object[])null);
            if (instance == null)
                try
                {
                    instance = Activator.CreateInstance(categoryClassPropertyInfo.PropertyType);
                    if (instance != null)
                        categoryClassPropertyInfo.SetValue((object)this, instance);
                    else
                        LogHelper.Instance.Log("Unable to instantiate configuration category class " +
                                               categoryClassPropertyInfo?.PropertyType?.Name);
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.LogException(
                        "Unable to instantiate configuration category class " +
                        categoryClassPropertyInfo?.PropertyType?.Name, ex);
                }

            return instance;
        }
    }
}