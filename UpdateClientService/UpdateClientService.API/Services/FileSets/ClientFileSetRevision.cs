using System.Collections.Generic;
using System.Linq;

namespace UpdateClientService.API.Services.FileSets
{
    public class ClientFileSetRevision : RevisionChangeSetKey
    {
        public string FileSetName { get; set; }

        public int RetentionDays { get; set; }

        public int RetentionRevisions { get; set; }

        public string RevisionName { get; set; }

        public string RevisionVersion { get; set; }

        public string SetPath { get; set; }

        public int SetCompressionType { get; set; }

        public string SetFileHash { get; set; }

        public long SetFileSize { get; set; }

        public string PatchSetPath { get; set; }

        public int PatchSetCompressionType { get; set; }

        public string PatchSetFileHash { get; set; }

        public long PatchSetFileSize { get; set; }

        public IEnumerable<ClientFileSetFile> Files { get; set; }

        public IEnumerable<ClientPatchFileSetFile> PatchFiles { get; set; }

        public virtual IEnumerable<ClientFileSetRevisionDependency> Dependencies { get; set; }

        public ClientFileSetRevision Clone()
        {
            var clientFileSetRevision1 = new ClientFileSetRevision();
            clientFileSetRevision1.FileSetId = FileSetId;
            clientFileSetRevision1.FileSetName = FileSetName;
            clientFileSetRevision1.RetentionDays = RetentionDays;
            clientFileSetRevision1.RetentionRevisions = RetentionRevisions;
            clientFileSetRevision1.RevisionId = RevisionId;
            clientFileSetRevision1.RevisionName = RevisionName;
            clientFileSetRevision1.RevisionVersion = RevisionVersion;
            clientFileSetRevision1.PatchRevisionId = PatchRevisionId;
            clientFileSetRevision1.SetPath = SetPath;
            clientFileSetRevision1.SetCompressionType = SetCompressionType;
            clientFileSetRevision1.SetFileHash = SetFileHash;
            clientFileSetRevision1.SetFileSize = SetFileSize;
            clientFileSetRevision1.PatchSetPath = PatchSetPath;
            clientFileSetRevision1.PatchSetCompressionType = PatchSetCompressionType;
            clientFileSetRevision1.PatchSetFileHash = PatchSetFileHash;
            clientFileSetRevision1.PatchSetFileSize = PatchSetFileSize;
            var clientFileSetRevision2 = clientFileSetRevision1;
            var files = new List<ClientFileSetFile>();
            if (Files != null)
                Files.ToList().ForEach(item => files.Add(item));
            clientFileSetRevision2.Files = files;
            var patchFiles = new List<ClientPatchFileSetFile>();
            if (PatchFiles != null)
                PatchFiles.ToList().ForEach(item => patchFiles.Add(item));
            clientFileSetRevision2.PatchFiles = patchFiles;
            var dependencies = new List<ClientFileSetRevisionDependency>();
            if (Dependencies != null)
                Dependencies.ToList().ForEach(item => dependencies.Add(item));
            clientFileSetRevision2.Dependencies = dependencies;
            return clientFileSetRevision2;
        }
    }
}