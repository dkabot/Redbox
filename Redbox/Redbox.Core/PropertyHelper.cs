using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Redbox.Core
{
    public static class PropertyHelper
    {
        public static string FormatObjectAsXml(object instance, string rootName)
        {
            using (var w = new StringWriter())
            {
                using (var xmlTextWriter = new XmlTextWriter(w))
                {
                    xmlTextWriter.WriteStartDocument();
                    xmlTextWriter.WriteStartElement(rootName);
                    WalkObjectTree(instance, xmlTextWriter);
                    xmlTextWriter.WriteEndElement();
                    xmlTextWriter.WriteEndDocument();
                    xmlTextWriter.Flush();
                    return w.ToString();
                }
            }
        }

        public static void UpdateObjectFromXml(object instance, XmlNode parentNode)
        {
            if (instance == null || parentNode == null)
                return;
            var xmlNodeList1 = parentNode.SelectNodes("property");
            if (xmlNodeList1 == null)
                return;
            foreach (XmlNode parentNode1 in xmlNodeList1)
                if (parentNode1.Attributes["read-only"] == null ||
                    string.Compare(parentNode1.Attributes["read-only"].Value, bool.TrueString, true) != 0)
                {
                    var element = (PropertyInfo)null;
                    if (parentNode1.Attributes["name"] != null)
                        element = instance.GetType().GetProperty(parentNode1.Attributes["name"].Value,
                            BindingFlags.Instance | BindingFlags.Public);
                    if (!(element == null))
                    {
                        var xmlNodeList2 = parentNode1.SelectNodes("property");
                        if (xmlNodeList2 != null && xmlNodeList2.Count > 0)
                        {
                            UpdateObjectFromXml(element.GetValue(instance, new object[0]), parentNode1);
                        }
                        else
                        {
                            var customAttribute =
                                (CustomEditorAttribute)Attribute.GetCustomAttribute(element,
                                    typeof(CustomEditorAttribute));
                            if (parentNode1.ChildNodes.Count > 0 && customAttribute != null &&
                                customAttribute.SetMethodName != null)
                            {
                                var method = instance.GetType().GetMethod(customAttribute.SetMethodName,
                                    BindingFlags.Instance | BindingFlags.Public);
                                if (method != null)
                                    method.Invoke(instance, new object[1]
                                    {
                                        parentNode1
                                    });
                            }
                            else if (parentNode1.Attributes["value"] != null)
                            {
                                var str = parentNode1.Attributes["value"].Value;
                                element.SetValue(instance, ConversionHelper.ChangeType(str, element.PropertyType),
                                    null);
                            }
                        }
                    }
                }
        }

        private static void WalkObjectTree(object instance, XmlWriter xmlWriter)
        {
            if (instance == null)
                return;
            foreach (var property in instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var customAttribute1 =
                    (BrowsableAttribute)Attribute.GetCustomAttribute(property, typeof(BrowsableAttribute));
                if (customAttribute1 == null || customAttribute1.Browsable)
                {
                    xmlWriter.WriteStartElement("property");
                    xmlWriter.WriteAttributeString("name", property.Name);
                    var customAttribute2 =
                        (DisplayNameAttribute)Attribute.GetCustomAttribute(property, typeof(DisplayNameAttribute));
                    if (customAttribute2 != null)
                        xmlWriter.WriteAttributeString("display-name", customAttribute2.DisplayName);
                    var customAttribute3 =
                        (DescriptionAttribute)Attribute.GetCustomAttribute(property, typeof(DescriptionAttribute));
                    if (customAttribute3 != null)
                        xmlWriter.WriteAttributeString("description", customAttribute3.Description);
                    var customAttribute4 =
                        (CategoryAttribute)Attribute.GetCustomAttribute(property, typeof(CategoryAttribute));
                    if (customAttribute4 != null)
                        xmlWriter.WriteAttributeString("category", customAttribute4.Category);
                    var customAttribute5 =
                        (ReadOnlyAttribute)Attribute.GetCustomAttribute(property, typeof(ReadOnlyAttribute));
                    if (customAttribute5 != null)
                        xmlWriter.WriteAttributeString("read-only", XmlConvert.ToString(customAttribute5.IsReadOnly));
                    var customAttribute6 =
                        (CustomEditorAttribute)Attribute.GetCustomAttribute(property, typeof(CustomEditorAttribute));
                    if (customAttribute6 != null)
                    {
                        xmlWriter.WriteAttributeString("custom-editor", customAttribute6.Text);
                        if (customAttribute6.GetMethodName != null)
                        {
                            var method = instance.GetType().GetMethod(customAttribute6.GetMethodName,
                                BindingFlags.Instance | BindingFlags.Public);
                            if (method != null)
                                method.Invoke(instance, new object[1]
                                {
                                    xmlWriter
                                });
                        }
                    }
                    else
                    {
                        var obj = (object)null;
                        try
                        {
                            obj = property.GetValue(instance, new object[0]);
                            if (obj != null)
                            {
                                var converter = TypeDescriptor.GetConverter(obj);
                                var str = converter == null || !converter.CanConvertTo(typeof(string))
                                    ? obj.ToString()
                                    : converter.ConvertToString(obj);
                                xmlWriter.WriteAttributeString("value", str);
                                xmlWriter.WriteAttributeString("default-value", str);
                            }
                        }
                        catch (Exception ex)
                        {
                        }

                        if (!Attribute.IsDefined(property, typeof(ExcludeTypeAttribute)))
                            xmlWriter.WriteAttributeString("type", property.PropertyType.AssemblyQualifiedName);
                        if (Attribute.IsDefined(property, typeof(RecurseAttribute)) && obj != null)
                        {
                            WalkObjectTree(obj, xmlWriter);
                        }
                        else
                        {
                            var customAttribute7 =
                                (ValidValueListProviderAttribute)Attribute.GetCustomAttribute(property,
                                    typeof(ValidValueListProviderAttribute));
                            if (customAttribute7 != null)
                            {
                                var method = instance.GetType().GetMethod(customAttribute7.MethodName,
                                    BindingFlags.Instance | BindingFlags.Public);
                                if (method != null && method.Invoke(instance, null) is string[] strArray)
                                {
                                    xmlWriter.WriteAttributeString("valid-value-count", strArray.Length.ToString());
                                    foreach (var str in strArray)
                                    {
                                        xmlWriter.WriteStartElement("valid");
                                        xmlWriter.WriteAttributeString("value", str);
                                        xmlWriter.WriteEndElement();
                                    }
                                }
                            }
                        }
                    }

                    xmlWriter.WriteEndElement();
                }
            }
        }
    }
}