namespace Redbox.UpdateManager.FileSets
{
    internal class FileSetRevisionChangeSet
    {
        public long FileSetId { get; set; }

        public long RevisionId { get; set; }

        public string Path { get; set; }

        public int CompressionType { get; set; }

        public string ContentHash { get; set; }

        public string FileHash { get; set; }

        public long ContentSize { get; set; }

        public long FileSize { get; set; }

        public long PatchRevisionId { get; set; }

        public ClientFileSetRevisionChangeSet ToClient()
        {
            return new ClientFileSetRevisionChangeSet()
            {
                FileSetId = this.FileSetId,
                RevisionId = this.RevisionId,
                Path = this.Path,
                CompressionType = this.CompressionType,
                FileHash = this.FileHash,
                ContentHash = this.ContentHash,
                PatchRevisionId = this.PatchRevisionId,
                ContentSize = this.ContentSize,
                FileSize = this.FileSize
            };
        }

        public FileSetRevisionChangeSet Clone()
        {
            return new FileSetRevisionChangeSet()
            {
                FileSetId = this.FileSetId,
                RevisionId = this.RevisionId,
                Path = this.Path,
                CompressionType = this.CompressionType,
                ContentHash = this.ContentHash,
                FileHash = this.FileHash,
                PatchRevisionId = this.PatchRevisionId,
                ContentSize = this.ContentSize,
                FileSize = this.FileSize
            };
        }
    }
}
