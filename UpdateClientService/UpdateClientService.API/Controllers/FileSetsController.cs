using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpdateClientService.API.Services.FileSets;
using UpdateClientService.API.Services.IoT.FileSets;

namespace UpdateClientService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileSetsController : ControllerBase
    {
        private readonly IFileSetCleanup _cleanup;
        private readonly IFileSetService _fileSetService;
        private readonly IKioskFileSetVersionsService _versionsService;

        public FileSetsController(
            IFileSetService fileSetService,
            IKioskFileSetVersionsService versionsService,
            IFileSetCleanup cleanup)
        {
            _fileSetService = fileSetService;
            _versionsService = versionsService;
            _cleanup = cleanup;
        }

        [HttpPost("invoke")]
        [ProducesResponseType(typeof(OkResult), 200)]
        public async Task<ActionResult> Invoke()
        {
            var fileSetsController = this;
            await fileSetsController._fileSetService.ProcessInProgressRevisionChangeSets();
            return fileSetsController.Ok();
        }

        [HttpPost("versions")]
        [ProducesResponseType(typeof(ReportFileSetVersionsResponse), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> ReportVersions([FromQuery] long? executionTimeFrameMs)
        {
            return (await _fileSetService.TriggerProcessPendingFileSetVersions(new TriggerReportFileSetVersionsRequest
            {
                ExecutionTimeFrameMs = executionTimeFrameMs
            })).ToObjectResult();
        }

        [HttpPost("changesets")]
        [ProducesResponseType(typeof(OkResult), 200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(typeof(ProcessChangeSetResponse), 200)]
        [ProducesResponseType(typeof(ProcessChangeSetResponse), 503)]
        [ProducesResponseType(typeof(ProcessChangeSetResponse), 500)]
        public async Task<ActionResult> ProcessChangeSet(ClientFileSetRevisionChangeSet changeSet)
        {
            return (await _fileSetService.ProcessChangeSet(changeSet)).ToObjectResult();
        }

        [HttpPost("cleanup")]
        [ProducesResponseType(typeof(OkResult), 200)]
        public async Task<ActionResult> Cleanup()
        {
            var fileSetsController = this;
            await fileSetsController._cleanup.Run();
            return fileSetsController.Ok();
        }
    }
}