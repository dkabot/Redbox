namespace Redbox.Rental.Model.Analytics
{
    public class AuthorizationData : AnalyticsData
    {
        public AuthorizationData()
        {
            DataType = "Authorization";
        }

        public string AuthorizationType { get; set; }

        public bool Online { get; set; }

        public bool? Approved { get; set; }

        public bool? PickupValid { get; set; }
    }
}