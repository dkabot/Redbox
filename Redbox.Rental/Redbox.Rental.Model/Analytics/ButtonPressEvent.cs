namespace Redbox.Rental.Model.Analytics
{
    public class ButtonPressEvent : AnalyticsEvent
    {
        public ButtonPressEvent(string name)
            : this(name, (string)null)
        {
        }

        public ButtonPressEvent(string name, string eventSubType)
        {
            EventType = "ButtonPress";
            Name = name;
            if (string.IsNullOrWhiteSpace(eventSubType))
                return;
            EventSubType = eventSubType;
        }

        public string Name { get; set; }
    }
}