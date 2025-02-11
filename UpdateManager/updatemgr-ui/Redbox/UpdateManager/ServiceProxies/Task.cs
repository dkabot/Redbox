using Redbox.UpdateManager.ComponentModel;
using System;

namespace Redbox.UpdateManager.ServiceProxies
{
    internal class Task : ITask
    {
        public DateTime? EndTime { get; set; }

        public string CronExpression { get; set; }

        public TimeSpan? RepeatInterval { get; set; }

        public string Name { get; set; }

        public string Payload { get; set; }

        public PayloadType PayloadType { get; set; }

        public ScheduleType ScheduleType { get; set; }

        public TimeSpan? StartOffset { get; set; }

        public DateTime? StartTime { get; set; }

        public PayloadType Type { get; set; }
    }
}
