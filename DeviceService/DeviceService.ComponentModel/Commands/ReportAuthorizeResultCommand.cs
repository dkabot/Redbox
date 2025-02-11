using System;

namespace DeviceService.ComponentModel.Commands
{
    public class ReportAuthorizeResultCommand : BaseCommandRequest
    {
        public ReportAuthorizeResultCommand(Version deviceServiceClientVersion) : base("ReportAuthorizeResult",
            deviceServiceClientVersion)
        {
        }

        public override string SignalRCommand => "ExecuteReportAuthorizeResultCommand";

        public bool Success { get; set; }
    }
}