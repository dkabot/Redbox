using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses
{
    public class CheckDeviceStatusResponseEvent : BaseResponseEvent
    {
        public CheckDeviceStatusResponseEvent(BaseCommandRequest baseCommandRequest)
            : base(baseCommandRequest)
        {
            EventName = nameof(CheckDeviceStatusResponseEvent);
        }
    }
}