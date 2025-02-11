using System;
using System.Collections.Generic;
using System.Linq;

namespace Redbox.UpdateManager.FileSets
{
    internal class ClientFileSetRevision
    {
        public long FileSetId { get; set; }

        public string FileSetName { get; set; }

        public int RetentionDays { get; set; }

        public int RetentionRevisions { get; set; }

        public long RevisionId { get; set; }

        public string RevisionName { get; set; }

        public string RevisionVersion { get; set; }

        public long PatchRevisionId { get; set; }

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
            ClientFileSetRevision clientFileSetRevision = new ClientFileSetRevision();
            clientFileSetRevision.FileSetId = this.FileSetId;
            clientFileSetRevision.FileSetName = this.FileSetName;
            clientFileSetRevision.RetentionDays = this.RetentionDays;
            clientFileSetRevision.RetentionRevisions = this.RetentionRevisions;
            clientFileSetRevision.RevisionId = this.RevisionId;
            clientFileSetRevision.RevisionName = this.RevisionName;
            clientFileSetRevision.RevisionVersion = this.RevisionVersion;
            clientFileSetRevision.PatchRevisionId = this.PatchRevisionId;
            clientFileSetRevision.SetPath = this.SetPath;
            clientFileSetRevision.SetCompressionType = this.SetCompressionType;
            clientFileSetRevision.SetFileHash = this.SetFileHash;
            clientFileSetRevision.SetFileSize = this.SetFileSize;
            clientFileSetRevision.PatchSetPath = this.PatchSetPath;
            clientFileSetRevision.PatchSetCompressionType = this.PatchSetCompressionType;
            clientFileSetRevision.PatchSetFileHash = this.PatchSetFileHash;
            clientFileSetRevision.PatchSetFileSize = this.PatchSetFileSize;
            List<ClientFileSetFile> files = new List<ClientFileSetFile>();
            if (this.Files != null)
                this.Files.ToList<ClientFileSetFile>().ForEach((Action<ClientFileSetFile>)(item => files.Add(item)));
            clientFileSetRevision.Files = (IEnumerable<ClientFileSetFile>)files;
            List<ClientPatchFileSetFile> patchFiles = new List<ClientPatchFileSetFile>();
            if (this.PatchFiles != null)
                this.PatchFiles.ToList<ClientPatchFileSetFile>().ForEach((Action<ClientPatchFileSetFile>)(item => patchFiles.Add(item)));
            clientFileSetRevision.PatchFiles = (IEnumerable<ClientPatchFileSetFile>)patchFiles;
            List<ClientFileSetRevisionDependency> dependencies = new List<ClientFileSetRevisionDependency>();
            if (this.Dependencies != null)
                this.Dependencies.ToList<ClientFileSetRevisionDependency>().ForEach((Action<ClientFileSetRevisionDependency>)(item => dependencies.Add(item)));
            clientFileSetRevision.Dependencies = (IEnumerable<ClientFileSetRevisionDependency>)dependencies;
            return clientFileSetRevision;
        }
    }
}
