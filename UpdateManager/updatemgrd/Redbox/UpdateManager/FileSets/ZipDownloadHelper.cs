using Ionic.Zip;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.FileCache;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Redbox.UpdateManager.FileSets
{
    internal class ZipDownloadHelper
    {
        private string _zipPath;
        private long _fileSetId;
        private long _revisionId;
        private long _patchRevisionId;
        private IFileCacheService _fileCacheService;

        internal ZipDownloadHelper(
          string zipPath,
          long fileSetId,
          long revisionId,
          long patchRevisionId)
        {
            this._zipPath = zipPath;
            this._fileSetId = fileSetId;
            this._revisionId = revisionId;
            this._patchRevisionId = patchRevisionId;
        }

        public ErrorList Extract()
        {
            ErrorList errors = new ErrorList();
            try
            {
                using (ZipFile zip = ZipFile.Read(this._zipPath))
                    zip.Where<ZipEntry>((Func<ZipEntry, bool>)(e => Path.GetExtension(e.FileName).ToLower() == ".txt")).ToList<ZipEntry>().ForEach((Action<ZipEntry>)(entry =>
                    {
                        try
                        {
                            string str;
                            Dictionary<string, object> dictionary;
                            using (MemoryStream stream = new MemoryStream())
                            {
                                entry.Extract((Stream)stream);
                                stream.Position = 0L;
                                str = Encoding.ASCII.GetString(stream.GetBytes());
                                dictionary = str.ToDictionary();
                            }
                            string fileEntryName = Path.GetFileNameWithoutExtension(entry.FileName);
                            ZipEntry zipEntry = zip.Where<ZipEntry>((Func<ZipEntry, bool>)(e => e.FileName.Equals(fileEntryName))).FirstOrDefault<ZipEntry>();
                            if (fileEntryName == null)
                            {
                                errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ZipDownloadHelper.Extract", string.Format("File {0} is missing from zip file", (object)fileEntryName)));
                            }
                            else
                            {
                                byte[] bytes;
                                using (MemoryStream stream = new MemoryStream())
                                {
                                    zipEntry.Extract((Stream)stream);
                                    stream.Position = 0L;
                                    bytes = stream.GetBytes();
                                }
                                if (dictionary.ContainsKey("RevisionId"))
                                {
                                    errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.FileCacheService.AddRevision(this._fileSetId, this._revisionId, this._patchRevisionId, bytes, str));
                                }
                                else
                                {
                                    long int64_1 = Convert.ToInt64(dictionary["FileId"]);
                                    long int64_2 = Convert.ToInt64(dictionary["FileRevisionId"]);
                                    if (dictionary.ContainsKey("PatchFileRevisionId"))
                                    {
                                        long int64_3 = Convert.ToInt64(dictionary["PatchFileRevisionId"]);
                                        errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.FileCacheService.AddFilePatch(this._fileSetId, int64_1, int64_2, int64_3, bytes, str));
                                    }
                                    else
                                        errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.FileCacheService.AddFile(this._fileSetId, int64_1, int64_2, bytes, str));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ZipDownloadHelper.Extract", string.Format("An unhandled exception occurred extracting {0}", (object)entry.FileName), ex));
                        }
                    }));
            }
            catch (Exception ex)
            {
                errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ZipDownloadHelper.Extract", "An unhandled exception occurred", ex));
            }
            return errors;
        }

        internal IFileCacheService FileCacheService
        {
            get
            {
                if (this._fileCacheService == null)
                    this._fileCacheService = ServiceLocator.Instance.GetService<IFileCacheService>();
                return this._fileCacheService;
            }
        }
    }
}
