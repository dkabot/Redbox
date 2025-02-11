using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses
{
    public class UnencryptedCardReadResponseEvent : BaseResponseEvent, ICardReadResponseEvent
    {
        public UnencryptedCardReadResponseEvent(BaseCommandRequest request)
            : base(request)
        {
            EventName = nameof(UnencryptedCardReadResponseEvent);
        }

        public UnencryptedCardReadModel Data { get; set; }

        public Base87CardReadModel GetBase87CardReadModel()
        {
            return Data;
        }
    }
}