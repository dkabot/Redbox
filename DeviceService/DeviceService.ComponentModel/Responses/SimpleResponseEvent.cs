using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses
{
    public class SimpleResponseEvent : BaseResponseEvent
    {
        public SimpleResponseEvent(
            BaseCommandRequest request,
            string eventName,
            string data = null,
            bool success = true)
            : base(request)
        {
            EventName = eventName;
            Data = data;
            Success = success;
        }

        public string Data { get; set; }
    }
}