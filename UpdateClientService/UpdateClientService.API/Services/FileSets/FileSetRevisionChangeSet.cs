namespace UpdateClientService.API.Services.FileSets
{
    public class FileSetRevisionChangeSet
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
            var client = new ClientFileSetRevisionChangeSet();
            client.FileSetId = FileSetId;
            client.RevisionId = RevisionId;
            client.Path = Path;
            client.CompressionType = CompressionType;
            client.FileHash = FileHash;
            client.ContentHash = ContentHash;
            client.PatchRevisionId = PatchRevisionId;
            client.ContentSize = ContentSize;
            client.FileSize = FileSize;
            return client;
        }

        public FileSetRevisionChangeSet Clone()
        {
            return new FileSetRevisionChangeSet
            {
                FileSetId = FileSetId,
                RevisionId = RevisionId,
                Path = Path,
                CompressionType = CompressionType,
                ContentHash = ContentHash,
                FileHash = FileHash,
                PatchRevisionId = PatchRevisionId,
                ContentSize = ContentSize,
                FileSize = FileSize
            };
        }
    }
}