using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.FileSets
{
    public class StateFileService : IStateFileService
    {
        private readonly ILogger<StateFileService> _logger;
        private readonly IStateFileRepository _stateFileRepository;

        public StateFileService(
            ILogger<StateFileService> logger,
            IStateFileRepository stateFileRepository)
        {
            _logger = logger;
            _stateFileRepository = stateFileRepository;
        }

        public async Task<bool> Delete(long fileSetId)
        {
            try
            {
                return await _stateFileRepository.Delete(fileSetId);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    string.Format("Exception while deleting StateFile for FileSetId {0}", fileSetId),
                    "/sln/src/UpdateClientService.API/Services/FileSets/StateFileService.cs");
                return false;
            }
        }

        public async Task<bool> DeleteInProgress(long fileSetId)
        {
            try
            {
                return await _stateFileRepository.DeleteInProgress(fileSetId);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    string.Format("Exception while deleting StateFile for FileSetId {0}", fileSetId),
                    "/sln/src/UpdateClientService.API/Services/FileSets/StateFileService.cs");
                return false;
            }
        }

        public async Task<StateFileResponse> Save(StateFile stateFile)
        {
            try
            {
                var httpStatusCode = await _stateFileRepository.Save(stateFile)
                    ? HttpStatusCode.OK
                    : HttpStatusCode.InternalServerError;
                var stateFileResponse = new StateFileResponse();
                stateFileResponse.StatusCode = httpStatusCode;
                stateFileResponse.StateFile = stateFile;
                return stateFileResponse;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    string.Format("Exception while saving StateFile for FileSetId {0}", stateFile?.FileSetId),
                    "/sln/src/UpdateClientService.API/Services/FileSets/StateFileService.cs");
                var stateFileResponse = new StateFileResponse();
                stateFileResponse.StatusCode = HttpStatusCode.InternalServerError;
                return stateFileResponse;
            }
        }

        public async Task<StateFileResponse> Get(long fileSetId)
        {
            try
            {
                var (flag, stateFile) = await _stateFileRepository.Get(fileSetId);
                var httpStatusCode = !flag || stateFile == null
                    ? !flag || stateFile != null ? HttpStatusCode.InternalServerError : HttpStatusCode.NotFound
                    : HttpStatusCode.OK;
                var stateFileResponse = new StateFileResponse();
                stateFileResponse.StatusCode = httpStatusCode;
                stateFileResponse.StateFile = stateFile;
                return stateFileResponse;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    string.Format("Exception while getting StateFile for FileSetId {0}", fileSetId),
                    "/sln/src/UpdateClientService.API/Services/FileSets/StateFileService.cs");
                var stateFileResponse = new StateFileResponse();
                stateFileResponse.StatusCode = HttpStatusCode.InternalServerError;
                return stateFileResponse;
            }
        }

        public async Task<StateFilesResponse> GetAll()
        {
            try
            {
                var (flag, stateFileList) = await _stateFileRepository.GetAll();
                var httpStatusCode = !flag || stateFileList.Count <= 0
                    ? !flag || stateFileList.Count != 0 ? HttpStatusCode.InternalServerError : HttpStatusCode.NotFound
                    : HttpStatusCode.OK;
                var all = new StateFilesResponse();
                all.StatusCode = httpStatusCode;
                all.StateFiles = stateFileList;
                return all;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while getting StateFiles",
                    "/sln/src/UpdateClientService.API/Services/FileSets/StateFileService.cs");
                var all = new StateFilesResponse();
                all.StatusCode = HttpStatusCode.InternalServerError;
                return all;
            }
        }

        public async Task<StateFilesResponse> GetAllInProgress()
        {
            try
            {
                var all = await _stateFileRepository.GetAll();
                var flag = all.Item1;
                var list = all.Item2.Where(x => x.IsRevisionDownloadInProgress).ToList();
                var httpStatusCode = !flag || list.Count <= 0
                    ? !flag || list.Count != 0 ? HttpStatusCode.InternalServerError : HttpStatusCode.NotFound
                    : HttpStatusCode.OK;
                var allInProgress = new StateFilesResponse();
                allInProgress.StatusCode = httpStatusCode;
                allInProgress.StateFiles = list;
                return allInProgress;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while getting in progress StateFiles",
                    "/sln/src/UpdateClientService.API/Services/FileSets/StateFileService.cs");
                var allInProgress = new StateFilesResponse();
                allInProgress.StatusCode = HttpStatusCode.InternalServerError;
                return allInProgress;
            }
        }
    }
}