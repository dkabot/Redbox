using System;
using System.Collections.Generic;
using System.Linq;

namespace UpdateClientService.API.Services.Broker
{
    public class PingStatistics : List<PingStatistic>, IPersistentData
    {
        public PingStatistic GetLatest()
        {
            return this.FirstOrDefault(x => x.StartInterval == this.Max(y => y.StartInterval));
        }

        public PingStatistic GetLastSuccessful()
        {
            var lastSuccessful = (PingStatistic)null;
            foreach (var pingStatistic in this)
                if (pingStatistic.PingSuccess && (lastSuccessful == null ||
                                                  pingStatistic.EndIntervalUTC > lastSuccessful.EndIntervalUTC))
                    lastSuccessful = pingStatistic;
            return lastSuccessful;
        }

        public void Cleanup()
        {
            this.Where(x =>
            {
                var dateTime1 = x.StartInterval;
                var date = dateTime1.Date;
                dateTime1 = DateTime.Today;
                var dateTime2 = dateTime1.AddDays(-30.0);
                return date < dateTime2;
            }).ToList().ForEach(x => Remove(x));
        }
    }
}