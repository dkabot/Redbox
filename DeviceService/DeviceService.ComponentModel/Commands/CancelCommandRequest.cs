using System;

namespace DeviceService.ComponentModel.Commands
{
    public class CancelCommandRequest : BaseCommandRequest
    {
        public CancelCommandRequest(Version deviceServiceClientVersion) : base("Cancel", deviceServiceClientVersion)
        {
        }

        public override string SignalRCommand => "ExecuteCancelCommand";

        public override bool IsQueuedCommand => false;

        public Guid CommandToCancelRequestId { get; set; }
    }
}