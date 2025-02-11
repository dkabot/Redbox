namespace DeviceService.ComponentModel.Responses
{
    public class DeviceServiceShutDownStartingEvent : SimpleEvent
    {
        public DeviceServiceShutDownStartingEvent()
        {
            EventName = nameof(DeviceServiceShutDownStartingEvent);
        }

        public ShutDownReason ShutDownReason { get; set; }
    }
}