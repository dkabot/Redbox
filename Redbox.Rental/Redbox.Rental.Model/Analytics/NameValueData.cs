namespace Redbox.Rental.Model.Analytics
{
    public class NameValueData : AnalyticsData
    {
        public NameValueData(string name, string value)
        {
            Name = name;
            Value = value;
            DataType = "NameValue";
        }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}