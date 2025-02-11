using System;
using System.Xml;

namespace Redbox.Core
{
    public static class XmlNodeExtensions
    {
        public static void SelectSingleNodeAndSetValue<T>(this XmlNode node, string path, T value)
        {
            GetTargetNode(node, path).InnerText = value.ToString();
        }

        public static void SelectSingleNodeAndSetInnerXml<T>(
            this XmlNode node,
            string path,
            T instance)
        {
            GetTargetNode(node, path).InnerXml = instance.ToString();
        }

        public static string SelectSingleNodeAndGetInnerText(this XmlNode node, string path)
        {
            var node1 = node.SelectSingleNode(path);
            return node1 != null ? node1.GetNodeInnerText() : null;
        }

        public static string GetNodeInnerXml(this XmlNode node)
        {
            return node.InnerXml;
        }

        public static string GetNodeInnerText(this XmlNode node)
        {
            return node.InnerText;
        }

        public static T GetNodeValue<T>(this XmlNode node)
        {
            return node.GetNodeValue(default(T));
        }

        public static T GetAttributeValue<T>(this XmlNode node, string attributeName)
        {
            return node.GetAttributeValue(attributeName, default(T));
        }

        public static T GetAttributeValue<T>(this XmlNode node, string attributeName, T defaultValue)
        {
            try
            {
                if (node != null)
                    if (node.Attributes[attributeName] != null)
                        return (T)ConversionHelper.ChangeType(node.Attributes[attributeName].Value, typeof(T));
            }
            catch (ArgumentException ex)
            {
            }

            return defaultValue;
        }

        public static void SetAttributeValue<T>(this XmlNode node, string attributeName, T value)
        {
            var attribute = node.Attributes[attributeName];
            if (attribute != null && !typeof(T).IsValueType && value == null)
            {
                node.Attributes.Remove(attribute);
            }
            else
            {
                if (attribute == null)
                {
                    attribute = node.OwnerDocument.CreateAttribute(attributeName);
                    node.Attributes.Append(attribute);
                }

                attribute.Value = value.ToString();
            }
        }

        public static T GetNodeValue<T>(this XmlNode node, T defaultValue)
        {
            try
            {
                if (node != null)
                    if (!string.IsNullOrEmpty(node.InnerText))
                        return (T)ConversionHelper.ChangeType(node.InnerText, typeof(T));
            }
            catch (ArgumentException ex)
            {
            }

            return defaultValue;
        }

        private static XmlNode GetTargetNode(XmlNode node, string path)
        {
            var strArray = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var targetNode = node;
            foreach (var str in strArray)
            {
                var newChild = targetNode.SelectSingleNode(str);
                if (newChild == null)
                {
                    newChild = targetNode.OwnerDocument.CreateElement(str);
                    targetNode.AppendChild(newChild);
                }

                targetNode = newChild;
            }

            return targetNode;
        }
    }
}