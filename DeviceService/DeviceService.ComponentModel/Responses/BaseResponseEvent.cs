using System;
using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses
{
    public abstract class BaseResponseEvent : BaseEvent
    {
        public BaseResponseEvent(BaseCommandRequest request)
        {
            if (request == null)
                return;
            RequestId = request.RequestId;
        }

        public Guid RequestId { get; set; }

        public bool Success { get; set; }
    }
}