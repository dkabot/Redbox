namespace DeviceService.ComponentModel.Responses
{
    public class CardReaderStateEvent : SimpleEvent
    {
        public CardReaderStateEvent()
        {
            EventName = nameof(CardReaderStateEvent);
        }

        public CardReaderState CardReaderState { get; set; }
    }
}