using System.Collections.Generic;

namespace Redbox.REDS.Framework
{
    internal class ResourceBundleFilter : IResourceBundleFilter
    {
        private readonly IDictionary<string, string> m_filters = new Dictionary<string, string>();

        public string this[string key]
        {
            get => !m_filters.ContainsKey(key) ? null : m_filters[key];
            set => m_filters[key] = value;
        }
    }
}