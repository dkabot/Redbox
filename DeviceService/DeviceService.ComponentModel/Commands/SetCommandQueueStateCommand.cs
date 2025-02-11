using System;

namespace DeviceService.ComponentModel.Commands
{
    public class SetCommandQueueStateCommand : BaseCommandRequest
    {
        public SetCommandQueueStateCommand(Version deviceServiceClientVersion) : base("SetCommandQueueState",
            deviceServiceClientVersion)
        {
        }

        public override string SignalRCommand => "SetCommandQueueState";

        public bool CommandQueuePaused { get; set; }

        public override bool IsQueuedCommand => false;
    }
}