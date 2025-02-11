using System;
using System.Collections.Generic;

namespace UpdateClientService.API.Services.IoT
{
    public class IoTConnectionAttempt
    {
        public DateTime? StartConnectionAttempt { get; set; }

        public DateTime? LatestConnectionAttempt { get; set; }

        public DateTime? Connected { get; set; }

        public DateTime? Disconnected { get; set; }

        public TimeSpan? ConnectionAttemptsDuration
        {
            get
            {
                var nullable = Connected.HasValue ? Connected : LatestConnectionAttempt;
                return !StartConnectionAttempt.HasValue || !nullable.HasValue
                    ? new TimeSpan?()
                    : nullable.Value - StartConnectionAttempt.Value;
            }
        }

        public int ConnectionAttempts { get; set; }

        public Dictionary<string, int> ConnectionExceptions { get; set; }
    }
}