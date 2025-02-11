using System;
using Newtonsoft.Json;

namespace DeviceService.ComponentModel.Commands
{
    public class BaseCommandRequest : IMessageScrub
    {
        public BaseCommandRequest(string commandName, Version deviceServiceClientVersion)
        {
            CommandName = commandName;
            DeviceServiceClientVersion = deviceServiceClientVersion;
        }

        public Guid RequestId { get; set; } = Guid.NewGuid();

        public virtual string CommandName { get; set; }

        public virtual string SignalRCommand => "ExecuteBaseCommand";

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public virtual bool IsQueuedCommand => true;

        public int? CommandTimeout { get; set; }

        public Version DeviceServiceClientVersion { get; set; }

        public virtual object Scrub()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}