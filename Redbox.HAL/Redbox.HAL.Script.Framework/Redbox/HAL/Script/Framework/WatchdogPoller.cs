using System;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    [Poller(Name = "Watchdog")]
    internal sealed class WatchdogPoller : ScriptPoller
    {
        private readonly string[] ignoreList = new string[8]
        {
            "quick-return",
            "qlm-unload",
            "thin",
            "unload-thin",
            "sync",
            "sync-locations",
            "clean-vmz",
            "thin-vmz"
        };

        private int m_gcCount;

        private WatchdogPoller()
        {
        }

        protected override int PollSleep => 1800000;

        protected override string ThreadName => "Watchdog Poller";

        protected override string JobName => "job-watchdog";

        protected override string JobLabel => "Watchdog process";

        protected override ProgramPriority Priority => ProgramPriority.Normal;

        protected override bool CoreExecute()
        {
            TrimLogs();
            var service = ServiceLocator.Instance.GetService<IExecutionService>();
            if (ControllerConfiguration.Instance.ResetTimeoutCountersWeekly)
            {
                var executionContext = service.ScheduleJob("reset-counters-job", string.Empty, new DateTime?(),
                    ProgramPriority.Low);
                executionContext.Pend();
                executionContext.WaitForCompletion();
                executionContext.Trash();
            }
            else
            {
                LogHelper.Instance.Log("Watchdog is not configured to reset timeout counters weekly.");
            }

            if (++m_gcCount > 2 && service.GetActiveContext() == null)
            {
                GC.Collect();
                m_gcCount = 0;
            }

            CleanErroredJobs();
            CleanupJobs();
            ExecutionEngine.Instance.CleanupJobs(false);
            return true;
        }

        private void ForeachJob(Action<IExecutionContext> action)
        {
            var now = DateTime.Now;
            var jobList = ExecutionEngine.Instance.GetJobList();
            try
            {
                jobList.ForEach(job =>
                {
                    if (job.IsImmediate)
                        return;
                    action(job);
                });
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Watchdog: Foreachjob caught an exception", ex);
            }
        }

        private void TrimLogs()
        {
            var num = 0;
            var now = DateTime.Now;
            if (now.DayOfWeek != DayOfWeek.Sunday || now.Hour < 2 || now.Hour > 4)
                return;
            foreach (var directory in Directory.GetDirectories(ServiceLocator.Instance
                         .GetService<IFormattedLogFactoryService>().LogsBasePath))
                if (!directory.Contains("Service") && !directory.Contains("ErrorLogs"))
                    num += TrimFilesInSubdirectory(directory, "*.log", ControllerConfiguration.Instance.ApplogDayAge);
            LogHelper.Instance.Log("Watchdog removed {0} files",
                num + TrimFilesInSubdirectory("c:\\Program Files\\Redbox\\HALService\\Video", "*.jpg",
                    ControllerConfiguration.Instance.WatchdogImageDayAge));
        }

        private int TrimFilesInSubdirectory(string directory, string filePattern, int ageInDays)
        {
            var num = 0;
            foreach (var file in Directory.GetFiles(directory, filePattern))
                if (DateTime.Now.Subtract(new FileInfo(file).LastWriteTime).Days >= ageInDays)
                    try
                    {
                        if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
                            LogHelper.Instance.Log(string.Format("Deleting file {0}", file), LogEntryType.Debug);
                        File.Delete(file);
                        ++num;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log(string.Format("Unable to delete file {0}", file), ex);
                    }

            if (num > 0)
                LogHelper.Instance.Log("AppLog Trimmer: deleted {0} files of type {1} from {2}", num,
                    filePattern == "*.log" ? "Application Log" : (object)"image", directory);
            return num;
        }

        private bool JobIsIgnored(IExecutionContext context)
        {
            if (ignoreList.Length == 0)
                return false;
            foreach (var ignore in ignoreList)
                if (ignore.Equals(context.ProgramName, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            return false;
        }

        private void CleanupJobs()
        {
            var now = DateTime.Now;
            var time = 1;
            var trashed = 0;
            ForeachJob(job =>
            {
                var status = job.GetStatus();
                if ((ExecutionContextStatus.Stopped != status && ExecutionContextStatus.Completed != status) ||
                    job.IsConnected || JobIsIgnored(job) || !job.ExecutionStart.HasValue)
                    return;
                var dateTime = job.ExecutionStart.Value;
                if (job.Result.ExecutionTime.HasValue)
                    dateTime = dateTime.Add(job.Result.ExecutionTime.Value);
                else
                    ++time;
                if (!(now >= dateTime.AddHours(time)))
                    return;
                ++trashed;
                job.Trash();
            });
            LogHelper.Instance.Log("Watchdog trashed {0} jobs.", trashed);
        }

        private void CleanErroredJobs()
        {
            var now = DateTime.Now;
            var cleaned = 0;
            ForeachJob(job =>
            {
                if (job.GetStatus() != ExecutionContextStatus.Errored || !(now >= job.CreatedOn.AddDays(5.0)))
                    return;
                ++cleaned;
                job.Trash();
            });
            LogHelper.Instance.Log("Watchdog trashed {0} errored jobs.", cleaned);
        }
    }
}