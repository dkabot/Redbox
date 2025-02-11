using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Configuration
{
    public sealed class ConfigurationService : IConfigurationService
    {
        internal const string IndexerGroup = "indexer";
        internal const string TextIndexerRegex = "\\[\"(?<indexer>.*?)\"\\]";
        internal const string NumericIndexerRegex = "\\[(?<indexer>[0-9]*?)\\]";
        private readonly string ConfigurationFile;

        private readonly Dictionary<string, IAttributeXmlConfiguration> Configurations =
            new Dictionary<string, IAttributeXmlConfiguration>();

        private readonly BindingFlags GetObjectFromPathBindingFlags = BindingFlags.Instance | BindingFlags.Public |
                                                                      BindingFlags.NonPublic |
                                                                      BindingFlags.InvokeMethod |
                                                                      BindingFlags.GetField | BindingFlags.GetProperty;

        private readonly char[] PropertyPathSeparators = new char[1]
        {
            '.'
        };

        private ConfigurationService(string file)
        {
            if (string.IsNullOrEmpty(file))
                return;
            ConfigurationFile = file;
        }

        public string FormatAsXml(string friendly)
        {
            friendly = friendly.ToLower();
            if (!Configurations.ContainsKey(friendly))
                return null;
            var configuration = Configurations[friendly];
            return PropertyHelper.FormatObjectAsXml(configuration, configuration.RootName);
        }

        public void UpdateFromXml(string key, string xmlData, ErrorList errors)
        {
            LogHelper.Instance.Log("[Configuration Service] Configuration change.");
            var lower = key.ToLower();
            if (!Configurations.ContainsKey(lower))
            {
                LogHelper.Instance.Log("[Configuration Service UpdateFromXml] The configuration {0} doesn't exist.",
                    key);
                errors.Add(Error.NewError("S001",
                    string.Format("The requested configuration name '{0}' is not known.", key),
                    "Specify one of the valid configuration names."));
            }
            else
            {
                var configuration = Configurations[lower];
                BroadcastChangeStart(configuration);
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xmlData);
                PropertyHelper.UpdateObjectFromXml(configuration, xmlDocument.DocumentElement);
                Save(errors, configuration);
                BroadcastChangeEnd(configuration);
            }
        }

        public void Load(ErrorList errors)
        {
            if (!File.Exists(ConfigurationFile))
            {
                errors.Add(Error.NewError("P001",
                    string.Format("The configuration file {0} does not exist.", ConfigurationFile),
                    "Specify a valid configuration file path."));
            }
            else
            {
                var document = new XmlDocument();
                document.Load(ConfigurationFile);
                if (document.DocumentElement == null)
                    return;
                foreach (var key in Configurations.Keys)
                    Configurations[key].LoadProperties(document, errors);
                LogHelper.Instance.Log("[Configuration Service] Broadcast configuration load.");
                foreach (var key in Configurations.Keys)
                    Configurations[key].NotifyConfigurationLoaded();
            }
        }

        public void Save(ErrorList errors)
        {
            if (!File.Exists(ConfigurationFile))
            {
                errors.Add(Error.NewError("P001",
                    string.Format("The configuration file {0} does not exist.", ConfigurationFile),
                    "Specify a valid configuration file path."));
            }
            else
            {
                var document = new XmlDocument();
                document.Load(ConfigurationFile);
                if (document.DocumentElement == null)
                    return;
                foreach (var key in Configurations.Keys)
                    Configurations[key].StoreProperties(document, errors);
                document.Save(ConfigurationFile);
            }
        }

        public object GetPropertyByName(string key, string name)
        {
            if (name == null)
                return null;
            var rootObject = FromKey(key);
            return rootObject != null ? GetValueForPath(name, rootObject) : null;
        }

        public void SetPropertyByName(string key, string name, object[] value)
        {
            LogHelper.Instance.Log("[Configuration Service] Property update.");
            if (name == null || value == null)
                return;
            var xmlConfiguration = FromKey(key);
            if (xmlConfiguration == null)
                return;
            BroadcastChangeStart(xmlConfiguration);
            SetValueForPath(name, xmlConfiguration, 1 == value.Length ? value[0] : value);
            Save(new ErrorList(), xmlConfiguration);
            BroadcastChangeEnd(xmlConfiguration);
        }

        public void RegisterConfiguration(string key, IAttributeXmlConfiguration me)
        {
            var lower = key.ToLower();
            if (Configurations.ContainsKey(lower))
                return;
            Configurations[lower] = me;
        }

        public void LoadAndImport(ErrorList errors)
        {
            var document = new XmlDocument();
            document.Load(ConfigurationFile);
            if (document.DocumentElement == null)
                return;
            Load(errors);
            foreach (var key in Configurations.Keys)
            {
                var configuration = Configurations[key];
                configuration.Import(errors);
                configuration.StoreProperties(document, errors);
            }

            document.Save(ConfigurationFile);
        }

        public void LoadAndUpgrade(ErrorList errors)
        {
            var document = new XmlDocument();
            document.Load(ConfigurationFile);
            if (document.DocumentElement == null)
                return;
            foreach (var key in Configurations.Keys)
                Configurations[key].Upgrade(document, errors);
            document.Save(ConfigurationFile);
        }

        public IAttributeXmlConfiguration FindConfiguration(string name)
        {
            return FromKey(name);
        }

        public IAttributeXmlConfiguration FindConfiguration(Configurations configuration)
        {
            return FindConfiguration(configuration.ToString());
        }

        public static ConfigurationService Make(string configFile)
        {
            return new ConfigurationService(configFile);
        }

        private void Save(ErrorList errors, IAttributeXmlConfiguration configuration)
        {
            if (!File.Exists(ConfigurationFile))
            {
                errors.Add(Error.NewError("P001",
                    string.Format("The configuration file {0} does not exist.", ConfigurationFile),
                    "Specify a valid configuration file path."));
            }
            else
            {
                var document = new XmlDocument();
                document.Load(ConfigurationFile);
                if (document.DocumentElement == null)
                    return;
                configuration.StoreProperties(document, errors);
                document.Save(ConfigurationFile);
            }
        }

        private void BroadcastChangeEnd(IAttributeXmlConfiguration config)
        {
            LogHelper.Instance.Log("[ConfigurationService] Broadcast config change end.");
            config.NotifyConfigurationChangeEnd();
        }

        private void BroadcastChangeStart(IAttributeXmlConfiguration config)
        {
            LogHelper.Instance.Log("[ConfigurationService] Broadcast config change start.");
            config.NotifyConfigurationChangeStart();
        }

        private object GetValueForPath(string path, object rootObject)
        {
            return GetObjectFromPath(path, rootObject, new int?());
        }

        private IAttributeXmlConfiguration FromKey(string key)
        {
            key = key.ToLower();
            return Configurations.ContainsKey(key) ? Configurations[key] : null;
        }

        private object ExtractIndexInternal(string part)
        {
            if (part == null)
                return null;
            var match1 = Regex.Match(part, "\\[\"(?<indexer>.*?)\"\\]", RegexOptions.Singleline);
            var match2 = Regex.Match(part, "\\[(?<indexer>[0-9]*?)\\]", RegexOptions.Singleline);
            var indexInternal = (object)null;
            if (match1.Success)
            {
                indexInternal = match1.Groups["indexer"].Captures[0].Value;
            }
            else
            {
                int result;
                if (match2.Success && int.TryParse(match2.Groups["indexer"].Captures[0].Value, out result))
                    indexInternal = result;
            }

            return indexInternal;
        }

        private object GetObjectFromPath(string path, object rootObject, int? depthAdjust)
        {
            if (path == null)
                return null;
            var strArray = path.Split(PropertyPathSeparators);
            if (strArray.Length == 0 || rootObject == null)
                return null;
            var length = strArray.Length;
            if (depthAdjust.HasValue)
                length -= depthAdjust.Value;
            var target = rootObject;
            for (var index = 0; index < length; ++index)
            {
                var args = (object[])null;
                var str = strArray[index];
                var indexInternal = ExtractIndexInternal(str);
                if (indexInternal != null)
                {
                    str = str.Substring(0, str.IndexOf("["));
                    args = new object[1] { indexInternal };
                }

                try
                {
                    target = target.GetType().InvokeMember(str, GetObjectFromPathBindingFlags, null, target, args);
                    if (target == null)
                        break;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }

            return target;
        }

        private void SetValueForPath(string path, object rootObject, object value)
        {
            var objectFromPath = GetObjectFromPath(path, rootObject, 1);
            if (objectFromPath == null)
                return;
            var strArray = path.Split(PropertyPathSeparators);
            var property = objectFromPath.GetType()
                .GetProperty(strArray[strArray.Length - 1], BindingFlags.Instance | BindingFlags.Public);
            property?.SetValue(objectFromPath, ConversionHelper.ChangeType(value, property.PropertyType), null);
        }
    }
}