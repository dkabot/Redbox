using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UpdateClientService.API.Services;

namespace UpdateClientService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly IStatusService _status;
        private ILogger<StatusController> _logger;

        public StatusController(ILogger<StatusController> logger, IStatusService status)
        {
            _logger = logger;
            _status = status;
        }

        [HttpGet("versions")]
        [ProducesResponseType(200, Type = typeof(FileVersionDataResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public IActionResult FileVersions()
        {
            return _status.GetFileVersions().ToObjectResult();
        }

        [HttpPost("versions")]
        [ProducesResponseType(200, Type = typeof(FileVersionDataResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public IActionResult FileVersions(IEnumerable<string> filePaths)
        {
            return _status.GetFileVersions(filePaths).ToObjectResult();
        }
    }
}