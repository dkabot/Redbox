using System.Text;

namespace Redbox.REDS.Framework
{
    public class TextContentLoader : IAspectContentLoader
    {
        internal object Value { get; set; }

        internal IResourceBundle Bundle { get; set; }

        public object GetContent()
        {
            return Value is IResourceBundleStorageEntry
                ? Encoding.UTF8.GetString(Bundle.Storage.GetEntryContent((IResourceBundleStorageEntry)Value))
                : Value;
        }
    }
}