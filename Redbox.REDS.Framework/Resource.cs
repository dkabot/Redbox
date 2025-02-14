using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml;
using Redbox.Core;

namespace Redbox.REDS.Framework
{
    public class Resource : IResource
    {
        private IDictionary<string, Aspect> m_aspects;
        private IDictionary<string, object> m_properties;

        protected internal Resource()
        {
        }

        internal List<string> InnerDependencies { get; } = new List<string>();

        internal IDictionary<string, Aspect> Aspects
        {
            get
            {
                if (m_aspects == null)
                    m_aspects = new Dictionary<string, Aspect>();
                return m_aspects;
            }
        }

        internal IDictionary<string, object> Properties
        {
            get
            {
                if (m_properties == null)
                    m_properties = new Dictionary<string, object>();
                return m_properties;
            }
        }

        public bool IsOverriden(ReadOnlyCollection<string> exclusions)
        {
            foreach (var filterProperty in Type.GetFilterProperties())
                if (!exclusions.Contains(filterProperty.Name) && this[filterProperty.Name] != null)
                    return true;
            return false;
        }

        public IAspect NewAspect(string name, IAspectType type)
        {
            var aspect = new Aspect
            {
                Name = name,
                Type = type
            };
            Aspects[name] = aspect;
            return aspect;
        }

        public IAspect GetAspect(string name)
        {
            return !Aspects.ContainsKey(name) ? null : (IAspect)Aspects[name];
        }

        public string GetFullName()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(Name);
            foreach (var filterProperty in Type.GetFilterProperties())
            {
                var str = this[filterProperty.Name] as string;
                if (!string.IsNullOrEmpty(str))
                    stringBuilder.AppendFormat("_{0}", str);
            }

            return stringBuilder.ToString();
        }

        public void AddDependency(string name)
        {
            if (InnerDependencies.Contains(name))
                return;
            InnerDependencies.Add(name);
        }

        public void RemoveDependency(string name)
        {
            InnerDependencies.Remove(name);
        }

        public ErrorList Save(IResourceBundle bundle)
        {
            var errorList = new ErrorList();
            var sb = new StringBuilder();
            var writer = new XmlTextWriter(new StringWriter(sb))
            {
                Formatting = Formatting.Indented
            };
            writer.WriteStartDocument();
            writer.WriteStartElement("resource");
            writer.WriteAttributeString("type", Type.Name);
            foreach (var property in Properties)
                if (property.Value != null)
                {
                    writer.WriteStartElement("property");
                    writer.WriteAttributeString("name", property.Key);
                    if (property.Value is List<object>)
                        foreach (var obj in (List<object>)property.Value)
                            writer.WriteElementString("value", obj.ToString());
                    else
                        writer.WriteAttributeString("value", property.Value.ToString());
                    writer.WriteEndElement();
                }

            foreach (var aspect in Aspects)
                aspect.Value.Save(writer);
            writer.WriteEndElement();
            writer.WriteEndDocument();
            bundle?.StoreResource(GetStorageFriendlyName(Name), sb.ToString());
            return errorList;
        }

        public bool PropertyEquals(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            if (this[name] is List<object> objectList)
                return objectList.Contains(value);
            if (this[name] is string strA && value is string)
                return string.Compare(strA, (string)value, true) == 0;
            return this[name] != null && this[name].Equals(value);
        }

        public bool PropertyEquals2(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            var flag = name.Substring(0, 2) == "!=";
            return this[name] is List<object> objectList
                ?
                !flag ? objectList.Contains(value) : !objectList.Contains(value)
                : this[name] is string strA && value is string
                    ? !flag
                        ? string.Compare(strA, (string)value, true) == 0
                        : string.Compare(strA, (string)value, true) != 0
                    : !flag
                        ? this[name] != null && this[name].Equals(value)
                        : this[name] != null && !this[name].Equals(value);
        }

        public string Name => (Properties.ContainsKey("name") ? Properties["name"] : null) as string;

        public object this[string name]
        {
            get
            {
                var propertyType = Type.GetPropertyType(name);
                if (propertyType == null)
                    return null;
                return !Properties.ContainsKey(name) ? propertyType.DefaultValue : Properties[name];
            }
            set => Properties[name] =
                (Type.GetPropertyType(name) ??
                 throw new ArgumentException(string.Format("Property '{0}' is not a valid part of the type.", name)))
                .GetTypedValue(value);
        }

        public IResourceBundleStorageEntry StorageEntry { get; internal set; }

        public ReadOnlyCollection<string> Dependencies => InnerDependencies.AsReadOnly();

        public IResourceType Type { get; internal set; }

        public static ErrorList FromXml(
            ResourceBundle bundle,
            XmlDocument document,
            out IResource resource)
        {
            resource = null;
            var errorList = new ErrorList();
            try
            {
                if (document == null || document.DocumentElement == null)
                {
                    errorList.Add(Error.NewError("R001", "The specified XML document is invalid.",
                        "Review the resource bundle to ensure valid XML is embedded."));
                    return errorList;
                }

                var attributeValue1 = document.DocumentElement.GetAttributeValue<string>("type");
                if (string.IsNullOrEmpty(attributeValue1))
                {
                    errorList.Add(Error.NewError("R002", "The type attribute must be specified.",
                        "Specify the type attribute in the resource XML and recompile."));
                    return errorList;
                }

                if (!bundle.ResourceTypes.ContainsKey(attributeValue1))
                {
                    errorList.Add(Error.NewError("R003",
                        string.Format("The type '{0}' is not defined.", attributeValue1),
                        "Specify a valid resource type in the resource XML and recompile."));
                    return errorList;
                }

                var resourceType = bundle.ResourceTypes[attributeValue1];
                var node1 = document.DocumentElement.SelectSingleNode("property[@name='name']");
                resource = resourceType.NewResourceInstance(node1.GetAttributeValue<string>("value"));
                var xmlNodeList1 = document.DocumentElement.SelectNodes("property");
                if (xmlNodeList1 != null)
                    foreach (XmlNode node2 in xmlNodeList1)
                    {
                        var attributeValue2 = node2.GetAttributeValue<string>("name");
                        if (node2.Attributes["value"] != null)
                        {
                            resource[attributeValue2] = node2.GetAttributeValue<object>("value");
                        }
                        else
                        {
                            var xmlNodeList2 = node2.SelectNodes("value");
                            if (xmlNodeList2 == null || xmlNodeList2.Count == 0)
                            {
                                errorList.Add(Error.NewError("R006",
                                    string.Format(
                                        "The property '{0}' does not have a value attribute or child value nodes.",
                                        attributeValue1),
                                    "Each property must have a valid value attribute or one or more valid value child nodes."));
                            }
                            else
                            {
                                var objectList = new List<object>();
                                foreach (XmlNode node3 in xmlNodeList2)
                                    objectList.Add(node3.GetNodeValue<object>());
                                resource[attributeValue2] = objectList;
                            }
                        }
                    }

                var attributeValue3 = document.DocumentElement.GetAttributeValue<string>("depends");
                if (!string.IsNullOrEmpty(attributeValue3))
                    foreach (var str in attributeValue3.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    {
                        var name = str.Trim();
                        if (!(name == resource.Name))
                            resource.AddDependency(name);
                    }

                var xmlNodeList3 = document.DocumentElement.SelectNodes("aspect");
                if (xmlNodeList3 != null)
                    foreach (XmlNode xmlNode in xmlNodeList3)
                    {
                        var attributeValue4 = xmlNode.GetAttributeValue<string>("name");
                        if (string.IsNullOrEmpty(attributeValue4))
                        {
                            errorList.Add(Error.NewError("R004",
                                "The name attribute on the aspect element must be specified.",
                                "Specify the name attribute on the aspect element in the resource XML and recompile."));
                        }
                        else
                        {
                            var aspectType = resource.Type.GetAspectType(attributeValue4);
                            if (aspectType == null)
                            {
                                errorList.Add(Error.NewError("R005",
                                    string.Format("The aspect type '{0}' is not defined.", attributeValue4),
                                    "Specify a valid aspect type in the resource XML and recompile."));
                                return errorList;
                            }

                            var aspect = resource.NewAspect(attributeValue4, aspectType);
                            foreach (XmlAttribute attribute in xmlNode.Attributes)
                                if (!(attribute.Name == "name"))
                                    aspect.Properties[attribute.Name] = attribute.Value;
                            errorList.AddRange(aspect.Type.ParseAspect(bundle, aspect, xmlNode));
                        }
                    }
            }
            catch (Exception ex)
            {
                errorList.Add(Error.NewError("R999", "An unhandled exception was raised in Resource.FromXml.", ex));
            }

            return errorList;
        }

        public override string ToString()
        {
            return Name ?? "(unnamed)";
        }

        public void ForEachPropertyNameDo(Action<string> action)
        {
            foreach (var key in Properties.Keys)
                action(key);
        }

        public void ForEachPropertyValueDo(Action<object> action)
        {
            foreach (var obj in Properties.Values)
                action(obj);
        }

        private static string GetStorageFriendlyName(string name)
        {
            name = name.Replace("_", "-");
            name = name.Replace(" ", "-");
            return Path.ChangeExtension(name, ".resource");
        }
    }
}