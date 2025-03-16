namespace Redbox.Rental.Model.Analytics
{
    public class ViewEventData : AnalyticsEvent
    {
        public ViewEventData(string action, string name)
        {
            EventType = "ViewEvent";
            Action = action;
            Name = name;
        }

        public string Action { get; set; }

        public string Name { get; set; }
    }
}