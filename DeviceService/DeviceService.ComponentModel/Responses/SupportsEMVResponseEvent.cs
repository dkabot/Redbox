using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses
{
    public class SupportsEMVResponseEvent : BaseResponseEvent
    {
        public SupportsEMVResponseEvent(BaseCommandRequest request)
            : base(request)
        {
            EventName = nameof(SupportsEMVResponseEvent);
        }

        public bool SuportsEMV { get; set; }
    }
}