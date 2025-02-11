using System;
using DeviceService.ComponentModel.Requests;
using Newtonsoft.Json;

namespace DeviceService.ComponentModel.Commands
{
    public class CheckActivationCommand : BaseCommandRequest
    {
        public CheckActivationCommand(Version deviceServiceClientVersion) : base("CheckActivation",
            deviceServiceClientVersion)
        {
        }

        public BluefinActivationRequest Request { get; set; }

        public override string SignalRCommand => "ExecuteCheckActivationCommand";

        public override object Scrub()
        {
            return JsonConvert.SerializeObject(new
            {
                Request = Request.Scrub(),
                SignalRCommand,
                RequestId,
                CommandName,
                CreatedDate,
                CommandTimeout,
                DeviceServiceClientVersion
            });
        }
    }
}