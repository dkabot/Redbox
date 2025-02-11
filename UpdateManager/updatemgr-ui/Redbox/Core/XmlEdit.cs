namespace Redbox.Core
{
    internal sealed class XmlEdit
    {
        public XmlEdit(string xpath, string value)
        {
            this.XPath = xpath;
            this.Value = value;
        }

        public string XPath { get; set; }

        public string Value { get; set; }
    }
}
