namespace Redbox.Rental.Model.Analytics
{
    public class ViewShownEvent : AnalyticsEvent
    {
        public ViewShownEvent(string name)
        {
            EventType = "View";
            ViewName = name;
        }

        public string ViewName { get; set; }
    }
}