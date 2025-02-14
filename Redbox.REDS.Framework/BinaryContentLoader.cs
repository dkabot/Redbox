namespace Redbox.REDS.Framework
{
    public class BinaryContentLoader : IAspectContentLoader
    {
        internal object Value { get; set; }

        internal IResourceBundle Bundle { get; set; }

        public object GetContent()
        {
            return Value is IResourceBundleStorageEntry
                ? Bundle.Storage.GetEntryContent((IResourceBundleStorageEntry)Value)
                : Value;
        }
    }
}