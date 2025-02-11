using System;
using System.Collections.Generic;
using System.Linq;

namespace UpdateClientService.API.Services.IoT
{
    public class IoTStatistics : IPersistentData
    {
        public List<IoTConnectionAttempt> ConnectionAttempts { get; set; } = new List<IoTConnectionAttempt>();

        public void StartConnectionAttempt()
        {
            var tconnectionAttempt = GetLatest();
            if (tconnectionAttempt != null && tconnectionAttempt.Connected.HasValue)
                tconnectionAttempt = null;
            if (tconnectionAttempt == null)
                tconnectionAttempt = CreateIoTConnectionAttempt();
            tconnectionAttempt.LatestConnectionAttempt = tconnectionAttempt.LatestConnectionAttempt.HasValue
                ? DateTime.Now
                : tconnectionAttempt.StartConnectionAttempt;
            ++tconnectionAttempt.ConnectionAttempts;
        }

        public void Connected()
        {
            var latest = GetLatest();
            if (latest == null)
                return;
            latest.Connected = DateTime.Now;
        }

        public void Disconnected()
        {
            var latest = GetLatest();
            if (latest == null || !latest.Connected.HasValue)
                return;
            latest.Disconnected = DateTime.Now;
        }

        public void AddException(string exceptionMessage)
        {
            var latest = GetLatest();
            if (latest == null)
                return;
            if (latest.ConnectionExceptions == null)
                latest.ConnectionExceptions = new Dictionary<string, int>();
            var num1 = 0;
            latest.ConnectionExceptions.TryGetValue(exceptionMessage, out num1);
            var num2 = num1 + 1;
            latest.ConnectionExceptions[exceptionMessage] = num2;
        }

        private IoTConnectionAttempt GetLatest()
        {
            return ConnectionAttempts.FirstOrDefault(x =>
            {
                var connectionAttempt = x.StartConnectionAttempt;
                var nullable = ConnectionAttempts.Max(y => y.StartConnectionAttempt);
                if (connectionAttempt.HasValue != nullable.HasValue)
                    return false;
                return !connectionAttempt.HasValue ||
                       connectionAttempt.GetValueOrDefault() == nullable.GetValueOrDefault();
            });
        }

        private IoTConnectionAttempt CreateIoTConnectionAttempt()
        {
            var tconnectionAttempt = new IoTConnectionAttempt
            {
                StartConnectionAttempt = DateTime.Now
            };
            ConnectionAttempts.Add(tconnectionAttempt);
            return tconnectionAttempt;
        }

        public void Cleanup()
        {
            ConnectionAttempts
                .Where(x => x.StartConnectionAttempt.HasValue &&
                            x.StartConnectionAttempt.Value.Date < DateTime.Today.AddDays(-30.0)).ToList()
                .ForEach(x => ConnectionAttempts.Remove(x));
        }
    }
}