using System;

namespace DeviceService.ComponentModel.Commands
{
    public class ValidateVersionCommand : BaseCommandRequest
    {
        public ValidateVersionCommand(Version deviceServiceClientVersion) : base("ValidateVersion",
            deviceServiceClientVersion)
        {
        }

        public override string SignalRCommand => "ExecuteValidateVersionCommand";

        public override bool IsQueuedCommand => false;
    }
}