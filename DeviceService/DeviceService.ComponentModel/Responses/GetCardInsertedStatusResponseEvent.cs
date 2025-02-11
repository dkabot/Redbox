using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses
{
    public class GetCardInsertedStatusResponseEvent : BaseResponseEvent
    {
        public GetCardInsertedStatusResponseEvent(BaseCommandRequest request)
            : base(request)
        {
            EventName = nameof(GetCardInsertedStatusResponseEvent);
        }

        public InsertedStatus CardInsertedStatus { get; set; }
    }
}