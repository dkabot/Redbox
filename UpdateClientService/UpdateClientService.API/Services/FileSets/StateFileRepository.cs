using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.FileSets
{
    public class StateFileRepository : IStateFileRepository
    {
        private const string FileSetStateActiveExt = ".active";
        private const string FileSetStateInProgressExt = ".inprogress";
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly int _lockWait = 2000;
        private readonly ILogger<StateFileRepository> _logger;

        public StateFileRepository(ILogger<StateFileRepository> logger)
        {
            _logger = logger;
        }

        public async Task<bool> Delete(long fileSetId)
        {
            return await Delete(fileSetId, new List<string>
            {
                ".active",
                ".inprogress"
            });
        }

        public async Task<bool> DeleteInProgress(long fileSetId)
        {
            return await Delete(fileSetId, new List<string>
            {
                ".inprogress"
            });
        }

        public async Task<bool> Save(StateFile stateFile)
        {
            var stateFileRepository = this;
            if (stateFile == null)
            {
                _logger.LogErrorWithSource("StateFile is null",
                    "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
                return false;
            }

            if (await _lock.WaitAsync(stateFileRepository._lockWait))
                try
                {
                    var flag = await stateFileRepository.SaveClientFileSetState(new ClientFileSetState
                    {
                        FileSetId = stateFile.FileSetId,
                        RevisionId = stateFile.ActiveRevisionId,
                        FileSetState = FileSetState.Active
                    });
                    if (flag)
                        flag = await stateFileRepository.SaveClientFileSetState(new ClientFileSetState
                        {
                            FileSetId = stateFile.FileSetId,
                            RevisionId = stateFile.InProgressRevisionId,
                            FileSetState = stateFile.InProgressRevisionId != 0L
                                ? stateFile.InProgressFileSetState
                                : FileSetState.Error
                        });
                    return flag;
                }
                catch (Exception ex)
                {
                    _logger.LogErrorWithSource(ex,
                        string.Format("Exception while saving StateFile for FileSetId {0}.", stateFile?.FileSetId),
                        "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
                    return false;
                }
                finally
                {
                    _lock.Release();
                }

            _logger.LogErrorWithSource("Lock failed.",
                "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
            return false;
        }

        public async Task<(bool, StateFile)> Get(long fileSetId)
        {
            var (flag, clientFileSetStates) = await GetClientFileSetStates(fileSetId);
            var stateFile = flag ? CreateStateFile(clientFileSetStates, fileSetId) : null;
            return (flag, stateFile);
        }

        public async Task<(bool, List<StateFile>)> GetAll()
        {
            var stateFiles = new List<StateFile>();
            var (flag, clientFileSetStateList) = await GetClientFileSetStates();
            foreach (var fileSetId in clientFileSetStateList.Select(x => x.FileSetId).Distinct())
            {
                var stateFile = CreateStateFile(clientFileSetStateList, fileSetId);
                if (stateFile != null)
                    stateFiles.Add(stateFile);
            }

            return (flag, stateFiles);
        }

        private async Task<bool> Delete(long fileSetId, List<string> fileExtensions)
        {
            if (await _lock.WaitAsync(_lockWait))
                try
                {
                    if (fileExtensions == null)
                        return false;
                    foreach (var fileExtension in fileExtensions)
                        Shared.SafeDelete(GetStateFilePath(fileSetId, fileExtension));
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogErrorWithSource(ex,
                        string.Format("Exception while deleting StateFile for FileSetId {0}.", fileSetId),
                        "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
                    return false;
                }
                finally
                {
                    _lock.Release();
                }

            _logger.LogErrorWithSource("Lock failed.",
                "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
            return false;
        }

        private async Task<bool> SaveClientFileSetState(ClientFileSetState clientFileSetState)
        {
            if (clientFileSetState == null)
                return false;
            var stateFilePath = GetStateFilePath(clientFileSetState.FileSetId,
                clientFileSetState.FileSetState == FileSetState.Active ? ".active" : ".inprogress");
            if (clientFileSetState.RevisionId != 0L)
                try
                {
                    File.WriteAllText(stateFilePath, clientFileSetState.ToJson());
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogErrorWithSource(ex, "Exception saving StateFile to " + stateFilePath,
                        "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
                    return false;
                }

            Shared.SafeDelete(stateFilePath);
            return true;
        }

        private StateFile CreateStateFile(List<ClientFileSetState> clientFileSetStates, long fileSetId)
        {
            var clientFileSetState1 = clientFileSetStates != null
                ? clientFileSetStates.FirstOrDefault(x =>
                    x.FileSetId == fileSetId && x.FileSetState == FileSetState.Active)
                : null;
            var clientFileSetState2 = clientFileSetStates != null
                ? clientFileSetStates.FirstOrDefault(x => x.FileSetId == fileSetId && x.FileSetState != 0)
                : null;
            return clientFileSetState1 == null && clientFileSetState2 == null
                ? null
                : new StateFile(fileSetId, clientFileSetState1 != null ? clientFileSetState1.RevisionId : 0L,
                    clientFileSetState2 != null ? clientFileSetState2.RevisionId : 0L,
                    clientFileSetState2 != null ? clientFileSetState2.FileSetState : FileSetState.Active);
        }

        private async Task<(bool, List<ClientFileSetState>)> GetClientFileSetStates(long? fileSetId = null)
        {
            var stateFiles = new List<ClientFileSetState>();
            if (await _lock.WaitAsync(_lockWait))
                try
                {
                    if (!Directory.Exists(Constants.FileSetsRoot))
                    {
                        _logger.LogErrorWithSource("StateFile directory " + Constants.FileSetsRoot + " does not exist.",
                            "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
                        return (false, stateFiles);
                    }

                    var stringList = new List<string>();
                    var str = fileSetId.HasValue ? fileSetId.ToString() : "*";
                    stringList.AddRange(Directory
                        .GetFiles(Constants.FileSetsRoot, str + ".active", SearchOption.TopDirectoryOnly).ToList());
                    stringList.AddRange(Directory.GetFiles(Constants.FileSetsRoot, str + ".inprogress",
                        SearchOption.TopDirectoryOnly).ToList());
                    foreach (var filePath in stringList)
                    {
                        var clientFileSetState = await Load(filePath);
                        if (clientFileSetState != null)
                            stateFiles.Add(clientFileSetState);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogErrorWithSource(ex, "Exception while getting StateFiles.",
                        "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
                    return (false, stateFiles);
                }
                finally
                {
                    _lock.Release();
                }
            else
                _logger.LogErrorWithSource("Lock failed.",
                    "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");

            return (true, stateFiles);
        }

        private async Task<ClientFileSetState> Load(string filePath)
        {
            var result = (ClientFileSetState)null;
            try
            {
                if (!string.IsNullOrEmpty(File.ReadAllText(filePath)))
                    try
                    {
                        result = File.ReadAllText(filePath).ToObject<ClientFileSetState>();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogErrorWithSource(ex, "Exception while deserializing StateFile " + filePath,
                            "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
                    }
                else
                    _logger.LogErrorWithSource("StateFile " + filePath + " was null",
                        "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while reading file " + filePath,
                    "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
            }

            if (result == null)
                Shared.SafeDelete(filePath);
            return result;
        }

        private static string GetStateFilePath(long fileSetId, string fileExtension)
        {
            return Path.ChangeExtension(Path.Combine(Constants.FileSetsRoot, fileSetId.ToString()), fileExtension);
        }
    }
}