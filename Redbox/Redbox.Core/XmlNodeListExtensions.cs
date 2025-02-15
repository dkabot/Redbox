using System;
using System.Xml;

namespace Redbox.Core
{
    public static class XmlNodeListExtensions
    {
        public static void ForEach(this XmlNodeList nodelist, Action<XmlNode> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            var count = nodelist.Count;
            for (var i = 0; i < count; ++i)
                action(nodelist[i]);
        }
    }
}