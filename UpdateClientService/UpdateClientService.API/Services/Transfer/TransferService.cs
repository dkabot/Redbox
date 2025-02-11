using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.App;

namespace UpdateClientService.API.Services.Transfer
{
    public class TransferService : ITransferService
    {
        private const uint EnumAllUsers = 1;
        private const uint EnumCurrentUser = 0;
        private readonly ILogger<TransferService> _logger;

        public TransferService(ILogger<TransferService> logger)
        {
            _logger = logger;
        }

        public List<Error> AreJobsRunning(out bool isRunning)
        {
            isRunning = false;
            var source = new List<Error>();
            List<ITransferJob> jobs;
            source.AddRange(GetJobs(out jobs, false));
            if (source.Any() || jobs.Count <= 0 ||
                jobs.FindIndex(j => j.Name.StartsWith("<~") && j.Name.EndsWith("~>")) <= -1)
                return source;
            isRunning = true;
            return source;
        }

        public List<Error> CancelAll()
        {
            List<ITransferJob> jobs1;
            var jobs2 = GetJobs(out jobs1, false);
            if (!jobs2.Any())
                jobs1.ForEach(j =>
                {
                    if (j.Status == TransferStatus.Transferred)
                        j.Complete();
                    else
                        j.Cancel();
                });
            return jobs2;
        }

        public List<Error> GetRepositoriesInTransit(out HashSet<long> inTransit)
        {
            inTransit = new HashSet<long>();
            var source = new List<Error>();
            List<ITransferJob> jobs;
            source.AddRange(GetJobs(out jobs, false));
            if (source.Any())
                return source;
            foreach (var transferJob in jobs)
            {
                var status = (int)transferJob.Status;
            }

            return source;
        }

        public List<Error> GetJobs(out List<ITransferJob> jobs, bool allUsers)
        {
            jobs = new List<ITransferJob>();
            var errors = new List<Error>();
            var o = (IBackgroundCopyManager)new BackgroundCopyManager();
            var ppenum = (IEnumBackgroundCopyJobs)null;
            try
            {
                o.EnumJobs(allUsers ? 1U : 0U, out ppenum);
                uint puCount;
                ppenum.GetCount(out puCount);
                for (var index = 0; index < puCount; ++index)
                {
                    IBackgroundCopyJob rgelt;
                    ppenum.Next(1U, out rgelt, out var _);
                    Guid pVal;
                    rgelt.GetId(out pVal);
                    jobs.Add(new TransferJob(pVal, rgelt));
                    Marshal.ReleaseComObject(rgelt);
                }

                jobs.Sort((lhs, rhs) =>
                {
                    if (lhs.FinishTime.HasValue && rhs.FinishTime.HasValue)
                        return lhs.FinishTime.Value.CompareTo(rhs.FinishTime.Value);
                    if (lhs.FinishTime.HasValue && !rhs.FinishTime.HasValue)
                        return 1;
                    return !lhs.FinishTime.HasValue && rhs.FinishTime.HasValue
                        ? -1
                        : lhs.StartTime.CompareTo(rhs.StartTime);
                });
            }
            catch (COMException ex)
            {
                HandleCOMException("Error getting BITS job", errors, ex);
            }
            catch (Exception ex)
            {
                HandleGenericException(errors, ex);
            }
            finally
            {
                Marshal.ReleaseComObject(o);
                if (ppenum != null)
                    Marshal.ReleaseComObject(ppenum);
            }

            return errors;
        }

        public List<Error> CreateDownloadJob(string name, out ITransferJob job)
        {
            job = null;
            var errors = new List<Error>();
            var o = (IBackgroundCopyManager)new BackgroundCopyManager();
            var ppJob = (IBackgroundCopyJob)null;
            try
            {
                Guid pJobId;
                o.CreateJob(name, BG_JOB_TYPE.BG_JOB_TYPE_DOWNLOAD, out pJobId, out ppJob);
                job = new TransferJob(pJobId, ppJob);
            }
            catch (COMException ex)
            {
                HandleCOMException("Error creating BITS job", errors, ex);
            }
            catch (Exception ex)
            {
                HandleGenericException(errors, ex);
            }
            finally
            {
                Marshal.ReleaseComObject(o);
                if (ppJob != null)
                    Marshal.ReleaseComObject(ppJob);
            }

            return errors;
        }

        public List<Error> CreateUploadJob(string name, out ITransferJob job)
        {
            job = null;
            var errors = new List<Error>();
            var o = (IBackgroundCopyManager)new BackgroundCopyManager();
            var ppJob = (IBackgroundCopyJob)null;
            try
            {
                Guid pJobId;
                o.CreateJob(name, BG_JOB_TYPE.BG_JOB_TYPE_UPLOAD, out pJobId, out ppJob);
                job = new TransferJob(pJobId, ppJob);
            }
            catch (COMException ex)
            {
                HandleCOMException("Error creating BITS job", errors, ex);
            }
            catch (Exception ex)
            {
                HandleGenericException(errors, ex);
            }
            finally
            {
                Marshal.ReleaseComObject(o);
                if (ppJob != null)
                    Marshal.ReleaseComObject(ppJob);
            }

            return errors;
        }

        public List<Error> GetJob(Guid id, out ITransferJob job)
        {
            var errors = new List<Error>();
            job = null;
            try
            {
                job = new TransferJob(id);
            }
            catch (COMException ex)
            {
                HandleCOMException("Error accessing BITS job", errors, ex);
            }
            catch (Exception ex)
            {
                HandleGenericException(errors, ex);
            }

            return errors;
        }

        public bool SetMaxBandwidthWhileWithInSchedule(int i)
        {
            var flag = MaxBandwidthWhileWithInSchedule != i;
            MaxBandwidthWhileWithInSchedule = i;
            return flag;
        }

        public int MaxBandwidthWhileWithInSchedule
        {
            get => BandwidthUsageSettings.MaxBandwidthWhileWithInSchedule;
            private set => BandwidthUsageSettings.MaxBandwidthWhileWithInSchedule = value;
        }

        public bool SetMaxBandwidthWhileOutsideOfSchedule(int max)
        {
            var flag = false;
            try
            {
                flag = MaxBandwidthWhileOutsideOfSchedule != max;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Error Accessing BITS settings.",
                    "/sln/src/UpdateClientService.API/Services/TransferService/TransferService.cs");
            }

            MaxBandwidthWhileOutsideOfSchedule = max;
            return flag;
        }

        public int MaxBandwidthWhileOutsideOfSchedule
        {
            get => BandwidthUsageSettings.MaxBandwidthWhileWithInSchedule;
            private set => BandwidthUsageSettings.MaxBandwidthWhileWithInSchedule = value;
        }

        public bool SetStartOfScheduleInHoursFromMidnight(byte b)
        {
            var flag = false;
            try
            {
                flag = StartOfScheduleInHoursFromMidnight != b;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Error Accessing BITS settings.",
                    "/sln/src/UpdateClientService.API/Services/TransferService/TransferService.cs");
            }

            StartOfScheduleInHoursFromMidnight = b;
            return flag;
        }

        public byte StartOfScheduleInHoursFromMidnight
        {
            get => BandwidthUsageSettings.StartOfScheduleInHoursFromMidnight;
            private set => BandwidthUsageSettings.StartOfScheduleInHoursFromMidnight = value;
        }

        public bool SetEndOfScheduleInHoursFromMidnight(byte b)
        {
            var flag = false;
            try
            {
                flag = EndOfScheduleInHoursFromMidnight != b;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Error Accessing BITS settings.",
                    "/sln/src/UpdateClientService.API/Services/TransferService/TransferService.cs");
            }

            EndOfScheduleInHoursFromMidnight = b;
            return flag;
        }

        public byte EndOfScheduleInHoursFromMidnight
        {
            get => BandwidthUsageSettings.EndOfScheduleInHoursFromMidnight;
            private set => BandwidthUsageSettings.EndOfScheduleInHoursFromMidnight = value;
        }

        public bool SetUseSystemMaxOutsideOfSchedule(bool flag)
        {
            var flag1 = false;
            try
            {
                flag1 = UseSystemMaxOutsideOfSchedule != flag;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Error Accessing BITS settings.",
                    "/sln/src/UpdateClientService.API/Services/TransferService/TransferService.cs");
            }

            UseSystemMaxOutsideOfSchedule = flag;
            return flag1;
        }

        public bool UseSystemMaxOutsideOfSchedule
        {
            get => BandwidthUsageSettings.UseSystemMaxOutsideOfSchedule;
            private set => BandwidthUsageSettings.UseSystemMaxOutsideOfSchedule = value;
        }

        public bool SetEnableMaximumBandwitdthThrottle(bool flag)
        {
            var flag1 = false;
            try
            {
                flag1 = EnableMaximumBandwitdthThrottle != flag;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Error Accessing BITS settings.",
                    "/sln/src/UpdateClientService.API/Services/TransferService/TransferService.cs");
            }

            EnableMaximumBandwitdthThrottle = flag;
            return flag1;
        }

        public bool EnableMaximumBandwitdthThrottle
        {
            get => BandwidthUsageSettings.EnableMaximumBandwitdthThrottle;
            private set => BandwidthUsageSettings.EnableMaximumBandwitdthThrottle = value;
        }

        private void HandleCOMException(string baseMessage, List<Error> errors, COMException ex)
        {
            errors.AddRange(new List<Error>
            {
                new Error
                {
                    Code = "C999",
                    Message = baseMessage + ". Exception -> " + ex.GetFullMessage()
                },
                GetLastWin32Error()
            });
        }

        private void HandleGenericException(List<Error> errors, Exception e)
        {
            errors.Add(new Error
            {
                Code = "C998",
                Message = "Error accessing BITS job. Exception -> " + e.GetFullMessage()
            });
        }

        private static Error GetLastWin32Error()
        {
            return new Error
            {
                Code = "B999",
                Message = "Error accessing BITS job. Exception -> " +
                          new Win32Exception(Marshal.GetLastWin32Error()).GetFullMessage()
            };
        }
    }
}