using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses
{
    public class EncryptedCardReadResponseEvent : BaseResponseEvent, ICardReadResponseEvent
    {
        public EncryptedCardReadResponseEvent(BaseCommandRequest request)
            : base(request)
        {
            EventName = nameof(EncryptedCardReadResponseEvent);
        }

        public EncryptedCardReadModel Data { get; set; }

        public Base87CardReadModel GetBase87CardReadModel()
        {
            return Data;
        }
    }
}