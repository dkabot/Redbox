using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses
{
    public class IsConnectedResponseEvent : BaseResponseEvent
    {
        public IsConnectedResponseEvent(BaseCommandRequest request)
            : base(request)
        {
            EventName = nameof(IsConnectedResponseEvent);
        }

        public bool IsConnected { get; set; }
    }
}