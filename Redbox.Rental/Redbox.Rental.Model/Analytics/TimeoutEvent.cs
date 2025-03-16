namespace Redbox.Rental.Model.Analytics
{
    public class TimeoutEvent : AnalyticsEvent
    {
        public TimeoutEvent(string viewName, int timeout, bool ttsEnabled = false)
        {
            EventType = nameof(Timeout);
            ViewName = viewName;
            Timeout = timeout;
            TTSEnabled = ttsEnabled;
        }

        public string ViewName { get; set; }

        public int Timeout { get; set; }

        public bool TTSEnabled { get; set; }
    }
}