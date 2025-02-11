using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace Redbox.Core
{
    internal class JavaScriptConverterRegistry
    {
        private readonly object m_syncObject = new object();
        private readonly IDictionary<string, JavaScriptConverter> m_converters = (IDictionary<string, JavaScriptConverter>)new Dictionary<string, JavaScriptConverter>();

        public static JavaScriptConverterRegistry Instance
        {
            get => Singleton<JavaScriptConverterRegistry>.Instance;
        }

        public void Clear()
        {
            lock (this.m_syncObject)
                this.m_converters.Clear();
        }

        public void AddConverter(string name, JavaScriptConverter converter)
        {
            lock (this.m_syncObject)
                this.m_converters[name] = converter;
        }

        public void RemoveConverter(string name)
        {
            lock (this.m_syncObject)
            {
                if (!this.m_converters.ContainsKey(name))
                    return;
                this.m_converters.Remove(name);
            }
        }

        public JavaScriptSerializer GetSerializer()
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            serializer.RegisterConverters((IEnumerable<JavaScriptConverter>)this.m_converters.Values.ToArray<JavaScriptConverter>());
            return serializer;
        }

        private JavaScriptConverterRegistry()
        {
            this.m_converters["DateTimeConverter"] = (JavaScriptConverter)new DateTimeConverter();
            this.m_converters["TimeSpanConverter"] = (JavaScriptConverter)new TimeSpanConverter();
        }
    }
}
