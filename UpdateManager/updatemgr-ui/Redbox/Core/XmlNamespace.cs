namespace Redbox.Core
{
    internal sealed class XmlNamespace
    {
        public XmlNamespace(string prefix, string uri)
        {
            this.Prefix = prefix;
            this.Uri = uri;
        }

        public string Prefix { get; set; }

        public string Uri { get; set; }
    }
}
