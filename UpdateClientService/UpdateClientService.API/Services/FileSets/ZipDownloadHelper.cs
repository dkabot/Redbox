using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.FileCache;

namespace UpdateClientService.API.Services.FileSets
{
    public class ZipDownloadHelper : IZipDownloadHelper
    {
        private readonly IFileCacheService _fileCacheService;
        private readonly ILogger<ZipDownloadHelper> _logger;

        public ZipDownloadHelper(ILogger<ZipDownloadHelper> logger, IFileCacheService fileCacheService)
        {
            _fileCacheService = fileCacheService;
            _logger = logger;
        }

        public bool Extract(string zipPath, RevisionChangeSetKey revisionChangeSetKey)
        {
            var result = true;
            try
            {
                using (var zipArchive = ZipFile.OpenRead(zipPath))
                {
                    zipArchive.Entries
                        .Where(eachZipArchiveEntry => Path.GetExtension(eachZipArchiveEntry.Name).ToLower() == ".json")
                        .ToList().ForEach(eachJsonZipArchiveEntry =>
                        {
                            try
                            {
                                var extractionData = new ExtractionData
                                {
                                    ZipArchive = zipArchive,
                                    ZipArchiveEntry = eachJsonZipArchiveEntry
                                };
                                GetInfoText(extractionData);
                                GetFileData(extractionData);
                                if (extractionData.IsRevison)
                                {
                                    if (!_fileCacheService.AddRevision(revisionChangeSetKey, extractionData.FileData,
                                            extractionData.InfoText))
                                    {
                                        var logger = _logger;
                                        var revisionChangeSetKey1 = revisionChangeSetKey;
                                        var str = "Unable to add revision for " + (revisionChangeSetKey1 != null
                                            ? revisionChangeSetKey1.IdentifyingText()
                                            : null);
                                        _logger.LogErrorWithSource(str,
                                            "/sln/src/UpdateClientService.API/Services/FileSets/ZipDownloadHelper.cs");
                                        extractionData.IsSuccess = false;
                                    }
                                }
                                else if (extractionData.IsPatchFile)
                                {
                                    if (!_fileCacheService.AddFilePatch(revisionChangeSetKey.FileSetId,
                                            extractionData.FileId, extractionData.FileRevisionId,
                                            extractionData.PatchFileRevisionId, extractionData.FileData,
                                            extractionData.InfoText))
                                    {
                                        _logger.LogErrorWithSource(
                                            string.Format(
                                                "Unable to add file patch for FileSetId {0}, FileId {1}, FileRevisionId {2}, PatchFileRevision {3}",
                                                revisionChangeSetKey.FileSetId, extractionData.FileId,
                                                extractionData.FileRevisionId, extractionData.PatchFileRevisionId),
                                            "/sln/src/UpdateClientService.API/Services/FileSets/ZipDownloadHelper.cs");
                                        extractionData.IsSuccess = false;
                                    }
                                }
                                else if (!_fileCacheService.AddFile(revisionChangeSetKey.FileSetId,
                                             extractionData.FileId, extractionData.FileRevisionId,
                                             extractionData.FileData, extractionData.InfoText))
                                {
                                    _logger.LogErrorWithSource(
                                        string.Format(
                                            "Unable to add file for FileSetId {0}, FileId {1}, FileRevisionId {2}",
                                            revisionChangeSetKey.FileSetId, extractionData.FileId,
                                            extractionData.FileRevisionId),
                                        "/sln/src/UpdateClientService.API/Services/FileSets/ZipDownloadHelper.cs");
                                    extractionData.IsSuccess = false;
                                }

                                result &= extractionData.IsSuccess;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogErrorWithSource(ex,
                                    "Exception while extracting " + eachJsonZipArchiveEntry.Name,
                                    "/sln/src/UpdateClientService.API/Services/FileSets/ZipDownloadHelper.cs");
                                result = false;
                            }
                        });
                }
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var revisionChangeSetKey2 = revisionChangeSetKey;
                var str = "Exception while extracting files for " +
                          (revisionChangeSetKey2 != null ? revisionChangeSetKey2.IdentifyingText() : null) + ".";
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/FileSets/ZipDownloadHelper.cs");
                result = false;
            }

            return result;
        }

        private void GetInfoText(ExtractionData extractionData)
        {
            using (var stream = extractionData.ZipArchiveEntry.Open())
            {
                extractionData.InfoText = stream.ReadToEnd();
            }

            extractionData.InfoValues = extractionData.InfoText.ToObject<Dictionary<string, object>>();
        }

        private void GetFileData(ExtractionData extractionData)
        {
            var fileEntryName = Path.GetFileNameWithoutExtension(extractionData.ZipArchiveEntry.Name);
            var zipArchiveEntry = extractionData.ZipArchive.Entries.Where(e => e.Name.Equals(fileEntryName))
                .FirstOrDefault();
            if (zipArchiveEntry == null)
            {
                _logger.LogErrorWithSource(fileEntryName + " is missing from zip file.",
                    "/sln/src/UpdateClientService.API/Services/FileSets/ZipDownloadHelper.cs");
                extractionData.IsSuccess = false;
            }
            else
            {
                using (var stream = zipArchiveEntry.Open())
                {
                    extractionData.FileData = stream.GetBytes();
                }
            }
        }

        private class ExtractionData
        {
            public bool IsSuccess { get; set; } = true;

            public string InfoText { get; set; }

            public byte[] FileData { get; set; }

            public Dictionary<string, object> InfoValues { get; set; } = new Dictionary<string, object>();

            public ZipArchive ZipArchive { get; set; }

            public ZipArchiveEntry ZipArchiveEntry { get; set; }

            public bool IsRevison => InfoValues.ContainsKey("RevisionId");

            public bool IsPatchFile => InfoValues.ContainsKey("PatchFileRevisionId");

            public long FileId => Convert.ToInt64(InfoValues[nameof(FileId)]);

            public long FileRevisionId => Convert.ToInt64(InfoValues[nameof(FileRevisionId)]);

            public long PatchFileRevisionId => Convert.ToInt64(InfoValues[nameof(PatchFileRevisionId)]);
        }
    }
}