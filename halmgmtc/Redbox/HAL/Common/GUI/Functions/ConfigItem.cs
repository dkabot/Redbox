using System;
using System.Collections.Generic;
using System.Xml;

namespace Redbox.HAL.Common.GUI.Functions
{
    public sealed class ConfigItem
    {
        private object m_value;

        public ConfigItem(XmlNode node)
        {
            Node = node;
            CustomEditor = node.GetAttributeValue<string>("custom-editor");
            ItemType = Type.GetType(node.GetAttributeValue("type", typeof(string).ToString()));
            ReadOnly = node.GetAttributeValue("read-only", false);
            Name = node.GetAttributeValue<string>("name");
            m_value = CustomEditor != null ? CustomEditor : (object)node.GetAttributeValue<string>("value");
            DefaultValue = m_value;
            Description = node.GetAttributeValue<string>("description");
            DisplayName = node.GetAttributeValue<string>("display-name");
            CategoryName = node.GetAttributeValue<string>("category");
            ValidValuesCount = node.GetAttributeValue("valid-value-count", 0);
            if (ValidValuesCount <= 0)
                return;
            ValidValues = new List<string>();
            foreach (XmlNode childNode in node.ChildNodes)
            {
                var attributeValue = childNode.GetAttributeValue<string>("value");
                if (attributeValue != null)
                    ValidValues.Add(attributeValue);
            }

            if (Value != null && !ValidValues.Contains(Value.ToString()))
                ValidValues.Add(Value.ToString());
            ItemType = ValidValues.GetType();
        }

        public bool ReadOnly { get; private set; }

        public string Name { get; private set; }

        public object Value
        {
            get => m_value;
            set
            {
                m_value = value;
                Node.SetAttributeValue(nameof(value), value.ToString());
            }
        }

        public string Description { get; private set; }

        public object DefaultValue { get; set; }

        public string DisplayName { get; set; }

        public string CategoryName { get; private set; }

        public List<string> ValidValues { get; }

        public int ValidValuesCount { get; }

        public string CustomEditor { get; }

        public Type ItemType { get; private set; }

        public XmlNode Node { get; }
    }
}