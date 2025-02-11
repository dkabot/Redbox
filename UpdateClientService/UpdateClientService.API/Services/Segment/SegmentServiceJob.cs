using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.Segment
{
    public class SegmentServiceJob : ISegmentServiceJob, IInvocable
    {
        private readonly ILogger<SegmentServiceJob> _logger;
        private readonly ISegmentService _segmentService;

        public SegmentServiceJob(ILogger<SegmentServiceJob> logger, ISegmentService segmentService)
        {
            _logger = logger;
            _segmentService = segmentService;
        }

        public async Task Invoke()
        {
            _logger.LogInfoWithSource("Invoking SegmentService.UpdateKioskSegmentsIfNeeded",
                "/sln/src/UpdateClientService.API/Services/Segment/SegmentServiceJob.cs");
            var num = await _segmentService.UpdateKioskSegmentsIfNeeded() ? 1 : 0;
        }
    }
}