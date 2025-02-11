using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.FileSets
{
    public class RevisionChangeSetRepository : IRevisionChangeSetRepository
    {
        private const string FileSetChangeSetExt = ".changeSet";
        private const string FileSetChangeSetPath = "changesets";
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly int _lockWait = 2000;
        private readonly ILogger<RevisionChangeSetRepository> _logger;

        public RevisionChangeSetRepository(ILogger<RevisionChangeSetRepository> logger)
        {
            _logger = logger;
            CreateChangeSetDirectoryIfNeeded();
        }

        private string ChangeSetDirectory => Path.Combine(Constants.FileSetsRoot, "changesets");

        public async Task<List<RevisionChangeSet>> GetAll()
        {
            var result = new List<RevisionChangeSet>();
            if (!Directory.Exists(ChangeSetDirectory))
                return result;
            if (await _lock.WaitAsync(_lockWait))
                try
                {
                    var strArray = Directory.GetFiles(ChangeSetDirectory, "*.changeSet", SearchOption.TopDirectoryOnly);
                    for (var index = 0; index < strArray.Length; ++index)
                    {
                        var eachFile = strArray[index];
                        try
                        {
                            result.Add(File.ReadAllText(eachFile).ToObject<RevisionChangeSet>());
                        }
                        catch (Exception ex)
                        {
                            _logger.LogErrorWithSource(ex,
                                "Exception while getting RevisionChangeSet from file " + eachFile,
                                "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");
                            Shared.SafeDelete(eachFile);
                        }

                        eachFile = null;
                    }

                    strArray = null;
                }
                catch (Exception ex)
                {
                    _logger.LogErrorWithSource(ex, "Exception while getting RevisionChangeSets",
                        "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");
                }
                finally
                {
                    _lock.Release();
                }
            else
                _logger.LogErrorWithSource("Lock failed.",
                    "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");

            return result;
        }

        public async Task<bool> Save(RevisionChangeSet revisionChangeSet)
        {
            var result = false;
            if (await _lock.WaitAsync(_lockWait))
                try
                {
                    File.WriteAllText(GetChangeSetPath(revisionChangeSet), revisionChangeSet.ToJson());
                    result = true;
                }
                catch (Exception ex)
                {
                    _logger.LogErrorWithSource(ex, "Exception while saving RevisionChangeSet",
                        "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");
                }
                finally
                {
                    _lock.Release();
                }
            else
                _logger.LogErrorWithSource("Lock failed.",
                    "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");

            return result;
        }

        public async Task<bool> Delete(RevisionChangeSet revisionChangeSet)
        {
            var result = false;
            if (await _lock.WaitAsync(_lockWait))
                try
                {
                    var changeSetPath = GetChangeSetPath(revisionChangeSet);
                    if (File.Exists(changeSetPath))
                        File.Delete(changeSetPath);
                    result = true;
                }
                catch (Exception ex)
                {
                    var logger = _logger;
                    var exception = ex;
                    var revisionChangeSet1 = revisionChangeSet;
                    var str = "Exception while deleting RevisionChangeSet " +
                              (revisionChangeSet1 != null ? revisionChangeSet1.ToJson() : null);
                    _logger.LogErrorWithSource(exception, str,
                        "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");
                }
                finally
                {
                    _lock.Release();
                }
            else
                _logger.LogErrorWithSource("Lock failed.",
                    "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");

            return result;
        }

        public async Task<bool> Cleanup(DateTime deleteDate)
        {
            if (!Directory.Exists(ChangeSetDirectory))
                return true;
            var result = false;
            if (await _lock.WaitAsync(_lockWait))
                try
                {
                    var files = Directory.GetFiles(ChangeSetDirectory, "*.changeSet", SearchOption.TopDirectoryOnly);
                    result = true;
                    foreach (var path in files)
                        try
                        {
                            if (File.GetCreationTime(path) <= deleteDate)
                            {
                                File.Delete(path);
                                _logger.LogInfoWithSource("Deleting file " + path,
                                    "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogErrorWithSource(ex,
                                "Exception while cleaning up RevisionChangeSet from file " + path,
                                "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");
                            result = false;
                        }
                }
                catch (Exception ex)
                {
                    _logger.LogErrorWithSource(ex, "Exception while cleaning up RevisionChangeSets",
                        "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");
                    result = false;
                }
                finally
                {
                    _lock.Release();
                }
            else
                _logger.LogErrorWithSource("Lock failed.",
                    "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");

            return result;
        }

        private string GetChangeSetPath(RevisionChangeSet revisionChangeSet)
        {
            return GetChangeSetPath(revisionChangeSet != null ? revisionChangeSet.FileSetId : 0L,
                revisionChangeSet != null ? revisionChangeSet.RevisionId : 0L);
        }

        private string GetChangeSetPath(long fileSetId, long revisionId)
        {
            return Path.ChangeExtension(
                Path.Combine(ChangeSetDirectory, string.Format("{0}-{1}", fileSetId, revisionId)), ".changeSet");
        }

        private void CreateChangeSetDirectoryIfNeeded()
        {
            try
            {
                if (Directory.Exists(ChangeSetDirectory))
                    return;
                Directory.CreateDirectory(ChangeSetDirectory);
            }
            catch (Exception ex)
            {
                var logger = _logger;
                if (logger == null)
                    return;
                _logger.LogErrorWithSource(ex, "Exception creating directory " + ChangeSetDirectory,
                    "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");
            }
        }
    }
}