using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.IoT.Certificate.Security;

namespace UpdateClientService.API.Services.Configuration
{
    public abstract class BaseConfiguration
    {
        protected readonly IEncryptionService _encryptionService;
        protected readonly ILogger<BaseConfiguration> _logger;
        private Dictionary<(string categoryName, string settingName), object> _defaultValues;
        protected KioskConfigurationSettings _kioskConfigurationSettings;
        protected IOptionsMonitor<KioskConfigurationSettings> _optionsMonitorKioskConfigurationSettings;

        public BaseConfiguration(
            ILogger<BaseConfiguration> logger,
            IOptions<KioskConfigurationSettings> optionsKioskConfigurationSettings,
            IEncryptionService encryptionService)
        {
            _logger = logger;
            _encryptionService = encryptionService;
            _kioskConfigurationSettings = optionsKioskConfigurationSettings.Value;
            PopulateConfigurationSettingValues();
        }

        public BaseConfiguration(
            ILogger<BaseConfiguration> logger,
            IOptionsMonitor<KioskConfigurationSettings> optionsMonitorKioskConfigurationSettings,
            IEncryptionService encryptionService)
        {
            _logger = logger;
            _encryptionService = encryptionService;
            _optionsMonitorKioskConfigurationSettings = optionsMonitorKioskConfigurationSettings;
            _optionsMonitorKioskConfigurationSettings.OnChange(OnChangeKioskConfigurationSettings);
            _kioskConfigurationSettings = optionsMonitorKioskConfigurationSettings.CurrentValue;
            PopulateConfigurationSettingValues();
        }

        public long ConfigurationVersion
        {
            get
            {
                var configurationSettings = _kioskConfigurationSettings;
                return configurationSettings == null ? 0L : configurationSettings.ConfigurationVersion;
            }
        }

        private void OnChangeKioskConfigurationSettings(
            KioskConfigurationSettings kioskConfigurationSettings,
            string data)
        {
            _kioskConfigurationSettings = kioskConfigurationSettings;
            PopulateConfigurationSettingValues();
        }

        public void Log(ConfigLogParameters loggingParameters = null)
        {
            var str = string.Format("KioskConfiguration Settings for configuration version: {0}",
                _kioskConfigurationSettings.ConfigurationVersion);
            if (_kioskConfigurationSettings.ConfigurationVersion == 0L)
                _logger.LogWarning(str + "  KioskConfiguration is not current.");
            else
                _logger.LogInformation(str);
            foreach (var property in GetType().GetProperties())
                if (property.PropertyType.IsSubclassOf(typeof(BaseCategorySetting)))
                    LogCategorySettingValues(GetCategoryClassInstance(property), loggingParameters);
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
            var flag1 = loggingParameters?.IncludeCategories == null || !loggingParameters.IncludeCategories.Any();
            int num1;
            if (loggingParameters?.IncludeCategories != null)
                num1 = loggingParameters.IncludeCategories.Intersect(new string[2]
                {
                    categoryName,
                    name1
                }).Any()
                    ? 1
                    : 0;
            else
                num1 = 0;
            var flag2 = num1 != 0;
            int num2;
            if (loggingParameters?.ExcludeCategories != null)
                num2 = loggingParameters.ExcludeCategories.Intersect(new string[2]
                {
                    categoryName,
                    name1
                }).Any()
                    ? 1
                    : 0;
            else
                num2 = 0;
            var flag3 = num2 != 0;
            int num3;
            if (loggingParameters?.IncludeSettings != null)
                num3 = loggingParameters.IncludeSettings.Intersect(new (string, string)[4]
                {
                    (categoryName, settingName),
                    (categoryName, name2),
                    (name1, settingName),
                    (name1, name2)
                }).Any()
                    ? 1
                    : 0;
            else
                num3 = 0;
            var flag4 = num3 != 0;
            int num4;
            if (loggingParameters?.ExcludeSettings != null)
                num4 = loggingParameters.ExcludeSettings.Intersect(new (string, string)[4]
                {
                    (categoryName, settingName),
                    (categoryName, name2),
                    (name1, settingName),
                    (name1, name2)
                }).Any()
                    ? 1
                    : 0;
            else
                num4 = 0;
            var flag5 = num4 != 0;
            if (flag4)
                return true;
            return ((!flag1 ? 0 : !flag3 ? 1 : 0) | (flag2 ? 1 : 0)) != 0 && !flag5;
        }

        private void LogCategorySettingValues(
            object categoryClassInstance,
            ConfigLogParameters loggingParameters = null)
        {
            if (categoryClassInstance == null)
                return;
            var categoryName = GetCategoryName(categoryClassInstance);
            foreach (var property in categoryClassInstance.GetType().GetProperties())
                if (property.CanWrite && property.Name != "ParentKioskConfiguration" &&
                    isSettingIncluded(categoryClassInstance, property, loggingParameters))
                {
                    var settingName = GetSettingName(property);
                    var settingValue = (object)null;
                    var flag1 = false;
                    var kioskSettingValue = _kioskConfigurationSettings.GetKioskSettingValue(categoryName, settingName);
                    if (kioskSettingValue != null)
                        try
                        {
                            var cipherText = kioskSettingValue.Value;
                            var flag2 = true;
                            if (kioskSettingValue.EncryptionType.HasValue)
                            {
                                if (GetMaskLogValueAttribute(property) != null)
                                {
                                    cipherText = _encryptionService
                                        .Decrypt(kioskSettingValue.EncryptionType.Value, cipherText).Result;
                                }
                                else
                                {
                                    flag2 = false;
                                    cipherText = "[Encrypted]";
                                }
                            }

                            settingValue = flag2 ? Convert.ChangeType(cipherText, property.PropertyType) : cipherText;
                        }
                        catch (Exception ex)
                        {
                            flag1 = true;
                            _logger.LogErrorWithSource(ex, "Unable to get " + categoryName + " property " + settingName,
                                "/sln/src/UpdateClientService.API/Services/Configuration/BaseConfiguration.cs");
                        }
                    else
                        flag1 = true;

                    if (flag1)
                        try
                        {
                            object obj;
                            if (_defaultValues.TryGetValue((categoryName, settingName), out obj))
                                settingValue = obj;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogErrorWithSource(ex,
                                "Unable to get " + categoryName + " property " + settingName + " with default value.",
                                "/sln/src/UpdateClientService.API/Services/Configuration/BaseConfiguration.cs");
                        }

                    var obj1 = (object)MaskSettingValue(property, settingValue);
                    var str = flag1 ? " [Default value]" : null;
                    _logger.LogInformation(string.Format("KioskConfiguration.{0}.{1} = {2}{3}", categoryName,
                        settingName, obj1, str));
                }
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
            return (MaskLogValue)Attribute.GetCustomAttribute(propertyInfo, typeof(MaskLogValue));
        }

        private void PopulateConfigurationSettingValues()
        {
            try
            {
                if (_optionsMonitorKioskConfigurationSettings != null)
                    _kioskConfigurationSettings = _optionsMonitorKioskConfigurationSettings.CurrentValue;
                var populateDefaultValues = _defaultValues == null;
                if (populateDefaultValues)
                    _defaultValues = new Dictionary<(string, string), object>();
                foreach (var property in GetType().GetProperties())
                    if (property.PropertyType.IsSubclassOf(typeof(BaseCategorySetting)))
                        PopulateCategorySettingValues(GetCategoryClassInstance(property), populateDefaultValues);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while populating configuration setting values.",
                    "/sln/src/UpdateClientService.API/Services/Configuration/BaseConfiguration.cs");
            }
        }

        private void PopulateCategorySettingValues(
            object categoryClassInstance,
            bool populateDefaultValues)
        {
            if (categoryClassInstance == null)
                return;
            var categoryName = GetCategoryName(categoryClassInstance);
            foreach (var property in categoryClassInstance.GetType().GetProperties())
                if (property.CanWrite)
                {
                    var settingName = GetSettingName(property);
                    if (populateDefaultValues)
                        _defaultValues[(categoryName, settingName)] = property.GetValue(categoryClassInstance, null);
                    var flag = false;
                    var kioskSettingValue = _kioskConfigurationSettings.GetKioskSettingValue(categoryName, settingName);
                    if (kioskSettingValue != null)
                        try
                        {
                            var result = kioskSettingValue.Value;
                            var encryptionType1 = kioskSettingValue.EncryptionType;
                            if (encryptionType1.HasValue)
                            {
                                var encryptionService = _encryptionService;
                                encryptionType1 = kioskSettingValue.EncryptionType;
                                var encryptionType2 = (int)encryptionType1.Value;
                                var cipherText = result;
                                result = encryptionService
                                    .Decrypt((ConfigurationEncryptionType)encryptionType2, cipherText).Result;
                            }

                            var obj = Convert.ChangeType(result, property.PropertyType);
                            property.SetValue(categoryClassInstance, obj, null);
                        }
                        catch (Exception ex)
                        {
                            flag = true;
                            _logger.LogErrorWithSource(ex, "Unable to set " + categoryName + " property " + settingName,
                                "/sln/src/UpdateClientService.API/Services/Configuration/BaseConfiguration.cs");
                        }
                    else
                        flag = true;

                    if (flag)
                        try
                        {
                            object obj;
                            if (_defaultValues.TryGetValue((categoryName, settingName), out obj))
                                property.SetValue(categoryClassInstance, obj, null);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogErrorWithSource(ex,
                                "Unable to set " + categoryName + " property " + settingName + " with default value.",
                                "/sln/src/UpdateClientService.API/Services/Configuration/BaseConfiguration.cs");
                        }
                }
        }

        private string GetSettingName(PropertyInfo settingPropertyInfo)
        {
            var name = settingPropertyInfo.Name;
            var customAttribute =
                (SettingAttribute)Attribute.GetCustomAttribute(settingPropertyInfo, typeof(SettingAttribute));
            if (customAttribute != null)
                name = customAttribute.Name;
            return name;
        }

        private string GetCategoryName(object categoryClassInstance)
        {
            var name = categoryClassInstance.GetType().Name;
            var customAttribute =
                (CategoryAttribute)Attribute.GetCustomAttribute(categoryClassInstance.GetType(),
                    typeof(CategoryAttribute));
            if (!string.IsNullOrEmpty(customAttribute?.Name))
                name = customAttribute?.Name;
            return name;
        }

        private object GetCategoryClassInstance(PropertyInfo categoryClassPropertyInfo)
        {
            var instance = categoryClassPropertyInfo?.GetValue(this, null);
            if (instance == null)
                try
                {
                    instance = Activator.CreateInstance(categoryClassPropertyInfo.PropertyType);
                    if (instance != null)
                        categoryClassPropertyInfo.SetValue(this, instance);
                    else
                        _logger.LogErrorWithSource(
                            "Unable to instantiate configuration category class " +
                            categoryClassPropertyInfo?.PropertyType?.Name,
                            "/sln/src/UpdateClientService.API/Services/Configuration/BaseConfiguration.cs");
                    SetParentKioskConfiguration(instance);
                }
                catch (Exception ex)
                {
                    _logger.LogErrorWithSource(ex,
                        "Unable to instantiate configuration category class " +
                        categoryClassPropertyInfo?.PropertyType?.Name,
                        "/sln/src/UpdateClientService.API/Services/Configuration/BaseConfiguration.cs");
                }

            return instance;
        }

        private void SetParentKioskConfiguration(object categoryClassInstance)
        {
            if (categoryClassInstance == null)
                return;

            var property = categoryClassInstance.GetType().GetProperty("ParentKioskConfiguration");
            if (property == null || property.GetValue(categoryClassInstance) != null)
                return;

            property.SetValue(categoryClassInstance, this);
        }
    }
}