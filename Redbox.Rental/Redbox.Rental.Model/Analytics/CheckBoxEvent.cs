namespace Redbox.Rental.Model.Analytics
{
    public class CheckBoxEvent : AnalyticsEvent
    {
        public CheckBoxEvent(string name, bool isChecked)
            : this((string)null, name)
        {
            IsChecked = isChecked;
        }

        public CheckBoxEvent(string eventSubType, string name)
        {
            EventType = "CheckBoxPressed";
            Name = name;
            if (string.IsNullOrWhiteSpace(eventSubType))
                return;
            EventSubType = eventSubType;
        }

        public string Name { get; set; }

        public bool IsChecked { get; set; }
    }
}