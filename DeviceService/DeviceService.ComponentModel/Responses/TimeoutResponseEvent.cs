using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses
{
    public class TimeoutResponseEvent : BaseResponseEvent
    {
        public TimeoutResponseEvent(BaseCommandRequest request)
            : base(request)
        {
            EventName = nameof(TimeoutResponseEvent);
        }
    }
}