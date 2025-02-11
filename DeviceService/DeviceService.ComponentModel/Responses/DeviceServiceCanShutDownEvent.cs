namespace DeviceService.ComponentModel.Responses
{
    public class DeviceServiceCanShutDownEvent : SimpleEvent
    {
        public DeviceServiceCanShutDownEvent()
        {
            EventName = nameof(DeviceServiceCanShutDownEvent);
        }
    }
}