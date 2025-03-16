namespace Redbox.Rental.Model.Analytics
{
    public class LoginEvent : AnalyticsEvent
    {
        public LoginEvent()
        {
            EventType = "Login";
        }

        public string LoginType { get; set; }

        public string WalletType { get; set; }

        public bool Success { get; set; }

        public string Error { get; set; }
    }
}