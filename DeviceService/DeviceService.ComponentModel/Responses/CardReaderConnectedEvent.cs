namespace DeviceService.ComponentModel.Responses
{
    public class CardReaderConnectedEvent : SimpleEvent
    {
        public CardReaderConnectedEvent()
        {
            EventName = nameof(CardReaderConnectedEvent);
        }
    }
}