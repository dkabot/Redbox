namespace Redbox.UpdateManager.Environment
{
    internal class StoreFilePollRequest
    {
        public long StoreFile { get; set; }

        public long StoreFileData { get; set; }

        public int SyncId { get; set; }
    }
}
