using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace Redbox.Core
{
    public class JavaScriptConverterRegistry
    {
        private readonly IDictionary<string, JavaScriptConverter> m_converters =
            new Dictionary<string, JavaScriptConverter>();

        private readonly object m_syncObject = new object();

        private JavaScriptConverterRegistry()
        {
            m_converters["DateTimeConverter"] = new DateTimeConverter();
            m_converters["TimeSpanConverter"] = new TimeSpanConverter();
        }

        public static JavaScriptConverterRegistry Instance => Singleton<JavaScriptConverterRegistry>.Instance;

        public void Clear()
        {
            lock (m_syncObject)
            {
                m_converters.Clear();
            }
        }

        public void AddConverter(string name, JavaScriptConverter converter)
        {
            lock (m_syncObject)
            {
                m_converters[name] = converter;
            }
        }

        public void RemoveConverter(string name)
        {
            lock (m_syncObject)
            {
                if (!m_converters.ContainsKey(name))
                    return;
                m_converters.Remove(name);
            }
        }

        public JavaScriptSerializer GetSerializer()
        {
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            serializer.RegisterConverters(m_converters.Values.ToArray());
            return serializer;
        }
    }
}