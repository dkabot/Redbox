namespace DeviceService.ComponentModel.Responses
{
    public class DeviceTamperedEvent : SimpleEvent
    {
        public DeviceTamperedEvent()
        {
            EventName = nameof(DeviceTamperedEvent);
        }
    }
}