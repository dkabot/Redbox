using System;
using System.Collections.Generic;
using System.Xml;
using Redbox.Core;

namespace Redbox.REDS.Framework
{
    public class AspectType : IAspectType
    {
        private readonly IDictionary<string, string> m_properties = new Dictionary<string, string>();

        public ErrorList ParseAspect(IResourceBundle bundle, IAspect aspect, XmlNode aspectNode)
        {
            var errors = new ErrorList();
            OnParseAspect(bundle, aspect, aspectNode, errors);
            return errors;
        }

        public string Name { get; internal set; }

        public string this[string name]
        {
            get => !m_properties.ContainsKey(name) ? null : m_properties[name];
            set => m_properties[name] = value;
        }

        protected virtual void OnParseAspect(
            IResourceBundle bundle,
            IAspect aspect,
            XmlNode aspectNode,
            ErrorList errors)
        {
            if (aspect.Properties.ContainsKey("file"))
            {
                aspect.SetContent(new BinaryContentLoader
                {
                    Bundle = bundle,
                    Value = bundle.GetEntry(aspect.Properties["file"])
                });
            }
            else
            {
                if (!aspect.Properties.ContainsKey("encoding"))
                    return;
                var ignoringCase = Enum<AspectBinaryEncodingType?>.ParseIgnoringCase(aspect.Properties["encoding"],
                    new AspectBinaryEncodingType?());
                if (!ignoringCase.HasValue || !ignoringCase.HasValue)
                    return;
                switch (ignoringCase.GetValueOrDefault())
                {
                    case AspectBinaryEncodingType.Base64:
                        aspect.SetContent(Convert.FromBase64String(aspectNode.InnerText));
                        break;
                    case AspectBinaryEncodingType.ASCII85:
                        aspect.SetContent(ASCII85.Decode(aspectNode.InnerText));
                        break;
                }
            }
        }
    }
}