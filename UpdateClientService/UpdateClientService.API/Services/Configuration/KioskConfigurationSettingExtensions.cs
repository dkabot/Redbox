using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using UpdateClientService.API.Services.IoT.Certificate.Security;

namespace UpdateClientService.API.Services.Configuration
{
    public static class KioskConfigurationSettingExtensions
    {
        public static T GetConfigValue<T>(
            this KioskConfigurationSettings kioskConfigurationSettings,
            string categoryName,
            string settingName,
            T defaultValue,
            ILogger logger,
            IEncryptionService encryptionService)
        {
            var configValue = defaultValue;
            try
            {
                var kioskSettingValue =
                    InnerGetKioskSettingValue(kioskConfigurationSettings, categoryName, settingName);
                if (kioskSettingValue != null)
                {
                    var result = kioskSettingValue.Value;
                    var encryptionType1 = kioskSettingValue.EncryptionType;
                    if (encryptionType1.HasValue)
                    {
                        var encryptionService1 = encryptionService;
                        encryptionType1 = kioskSettingValue.EncryptionType;
                        var encryptionType2 = (int)encryptionType1.Value;
                        var cipherText = result;
                        result = encryptionService1.Decrypt((ConfigurationEncryptionType)encryptionType2, cipherText)
                            .Result;
                    }

                    configValue = (T)Convert.ChangeType(result, typeof(T));
                }
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.LogError(ex,
                        "Unable to GetConfigValue for category " + categoryName + " and setting " + settingName);
            }

            return configValue;
        }

        public static KioskSettingValue GetKioskSettingValue(
            this KioskConfigurationSettings kioskConfigurationSettings,
            string categoryName,
            string settingName)
        {
            return InnerGetKioskSettingValue(kioskConfigurationSettings, categoryName, settingName);
        }

        private static KioskSettingValue InnerGetKioskSettingValue(
            KioskConfigurationSettings kioskConfigurationSettings,
            string categoryName,
            string settingName)
        {
            var kioskSettingValue = (KioskSettingValue)null;
            KioskSetting kioskSetting1;
            if (kioskConfigurationSettings == null)
            {
                kioskSetting1 = null;
            }
            else
            {
                var categories = kioskConfigurationSettings.Categories;
                if (categories == null)
                {
                    kioskSetting1 = null;
                }
                else
                {
                    var configurationCategory = categories.FirstOrDefault(x =>
                        x.Name.Equals(categoryName, StringComparison.CurrentCultureIgnoreCase));
                    kioskSetting1 = configurationCategory != null
                        ? configurationCategory.Settings.FirstOrDefault(y =>
                            y.Name.Equals(settingName, StringComparison.CurrentCultureIgnoreCase))
                        : null;
                }
            }

            var kioskSetting2 = kioskSetting1;
            if (kioskSetting2 != null)
                kioskSettingValue = kioskSetting2.SettingValues.Where(x =>
                {
                    if (!(x.EffectiveDateTime <= DateTime.Now))
                        return false;
                    if (!x.ExpireDateTime.HasValue)
                        return true;
                    var expireDateTime = x.ExpireDateTime;
                    var now = DateTime.Now;
                    return expireDateTime.HasValue && expireDateTime.GetValueOrDefault() > now;
                }).OrderByDescending(x => x.Rank).ThenByDescending(x => x.EffectiveDateTime).FirstOrDefault();
            return kioskSettingValue;
        }
    }
}