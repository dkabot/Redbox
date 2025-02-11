namespace DeviceService.ComponentModel.Responses
{
    public class CardReaderDisconnectedEvent : SimpleEvent
    {
        public CardReaderDisconnectedEvent()
        {
            EventName = nameof(CardReaderDisconnectedEvent);
        }
    }
}