namespace Redbox.Core
{
    public sealed class XmlNamespace
    {
        public XmlNamespace(string prefix, string uri)
        {
            Prefix = prefix;
            Uri = uri;
        }

        public string Prefix { get; set; }

        public string Uri { get; set; }
    }
}