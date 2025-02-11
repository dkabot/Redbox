using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses
{
    public class EMVCardReadResponseEvent : BaseResponseEvent, ICardReadResponseEvent
    {
        public EMVCardReadResponseEvent(BaseCommandRequest request)
            : base(request)
        {
            EventName = nameof(EMVCardReadResponseEvent);
        }

        public EMVCardReadModel Data { get; set; }

        public Base87CardReadModel GetBase87CardReadModel()
        {
            return Data;
        }
    }
}