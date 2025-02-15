using System;
using System.Collections.Generic;
using System.Xml;
using Redbox.Core;

namespace Redbox.REDS.Framework
{
    public class Aspect : IAspect
    {
        private IDictionary<string, string> m_properties;

        protected internal Aspect()
        {
        }

        internal object Content { get; set; }

        public object GetContent()
        {
            if (Content is IAspectContentLoader content)
                Content = content.GetContent();
            return Content;
        }

        public void SetContent(object value)
        {
            Content = value;
        }

        public ErrorList Save(XmlTextWriter writer)
        {
            var errorList = new ErrorList();
            writer.WriteStartElement("aspect");
            writer.WriteAttributeString("name", Name);
            foreach (var property in Properties)
                if (property.Value != null)
                    writer.WriteAttributeString(property.Key, property.Value);
            if (!Properties.ContainsKey("file"))
            {
                var content = GetContent();
                switch (content)
                {
                    case byte[] _:
                        AspectBinaryEncodingType? nullable = AspectBinaryEncodingType.Base64;
                        if (Properties.ContainsKey("encoding"))
                            nullable = Enum<AspectBinaryEncodingType?>.ParseIgnoringCase(Properties["encoding"],
                                AspectBinaryEncodingType.Base64);
                        else
                            writer.WriteAttributeString("encoding", nullable.ToString());
                        if (nullable.HasValue)
                            switch (nullable.GetValueOrDefault())
                            {
                                case AspectBinaryEncodingType.Base64:
                                    writer.WriteString(Convert.ToBase64String((byte[])content));
                                    break;
                                case AspectBinaryEncodingType.ASCII85:
                                    writer.WriteCData(ASCII85.Encode((byte[])content));
                                    break;
                            }
                        else
                            break;

                        break;
                    case XmlNode _:
                        writer.WriteString(((XmlNode)content).OuterXml);
                        break;
                    case string _:
                        writer.WriteString((string)content);
                        break;
                }
            }

            writer.WriteEndElement();
            return errorList;
        }

        public string Name { get; internal set; }

        public IAspectType Type { get; internal set; }

        public AspectFlags Flags { get; internal set; }

        public IDictionary<string, string> Properties
        {
            get
            {
                if (m_properties == null)
                    m_properties = new Dictionary<string, string>();
                return m_properties;
            }
        }
    }
}