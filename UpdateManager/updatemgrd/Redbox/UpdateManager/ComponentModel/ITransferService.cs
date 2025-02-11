using System;
using System.Collections.Generic;

namespace Redbox.UpdateManager.ComponentModel
{
    internal interface ITransferService
    {
        ErrorList CancelAll();

        ErrorList GetRepositoriesInTransit(out HashSet<string> inTransit);

        ErrorList GetJobs(out List<ITransferJob> jobs, bool allUsers);

        ErrorList CreateDownloadJob(string name, out ITransferJob job);

        ErrorList CreateUploadJob(string name, out ITransferJob job);

        ErrorList GetJob(Guid id, out ITransferJob job);

        ErrorList AreJobsRunning(out bool isRunning);

        bool SetMaxBandwidthWhileWithInSchedule(int i);

        int MaxBandwidthWhileWithInSchedule { get; }

        bool SetMaxBandwidthWhileOutsideOfSchedule(int max);

        int MaxBandwidthWhileOutsideOfSchedule { get; }

        bool SetStartOfScheduleInHoursFromMidnight(byte b);

        byte StartOfScheduleInHoursFromMidnight { get; }

        bool SetEndOfScheduleInHoursFromMidnight(byte b);

        byte EndOfScheduleInHoursFromMidnight { get; }

        bool SetUseSystemMaxOutsideOfSchedule(bool flag);

        bool UseSystemMaxOutsideOfSchedule { get; }

        bool SetEnableMaximumBandwitdthThrottle(bool flag);

        bool EnableMaximumBandwitdthThrottle { get; }
    }
}
