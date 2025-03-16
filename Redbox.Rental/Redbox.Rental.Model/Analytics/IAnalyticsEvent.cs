using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Analytics
{
    public interface IAnalyticsEvent
    {
        DateTime Timestamp { get; set; }

        string EventType { get; }

        List<IAnalyticsEvent> Events { get; }

        IAnalyticsEvent AddEvent(IAnalyticsEvent analyticsEvent);

        IAnalyticsEvent AddProperty(string name, object value);

        List<IAnalyticsData> Data { get; }

        IAnalyticsData AddData(IAnalyticsData analyticsData);
    }
}