using System;

namespace DeviceService.ComponentModel.Commands
{
    public class IsConnectedCommandRequest : BaseCommandRequest
    {
        public IsConnectedCommandRequest(Version deviceServiceClientVersion) : base("IsConnected",
            deviceServiceClientVersion)
        {
        }

        public override bool IsQueuedCommand => false;
    }
}