using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Redbox.UpdateManager.BITS
{
    internal class TransferService : ITransferService
    {
        private const uint EnumAllUsers = 1;
        private const uint EnumCurrentUser = 0;

        public static TransferService Instance => Singleton<TransferService>.Instance;

        public void Initialize()
        {
            ServiceLocator.Instance.AddService(typeof(ITransferService), (object)this);
        }

        public ErrorList AreJobsRunning(out bool isRunning)
        {
            isRunning = false;
            ErrorList errorList = new ErrorList();
            List<ITransferJob> jobs;
            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.GetJobs(out jobs, false));
            if (errorList.ContainsError() || jobs.Count <= 0 || jobs.FindIndex((Predicate<ITransferJob>)(j => j.Name.StartsWith("<~") && j.Name.EndsWith("~>"))) <= -1)
                return errorList;
            isRunning = true;
            return errorList;
        }

        public ErrorList CancelAll()
        {
            List<ITransferJob> jobs1;
            ErrorList jobs2 = this.GetJobs(out jobs1, false);
            if (jobs2.ContainsError())
                return jobs2;
            jobs1.ForEach((Action<ITransferJob>)(j =>
            {
                if (j.Status == TransferStatus.Transferred)
                    j.Complete();
                else
                    j.Cancel();
            }));
            return jobs2;
        }

        public ErrorList GetRepositoriesInTransit(out HashSet<string> inTransit)
        {
            inTransit = new HashSet<string>();
            IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
            ErrorList repositoriesInTransit = new ErrorList();
            List<ITransferJob> jobs;
            repositoriesInTransit.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.GetJobs(out jobs, false));
            if (repositoriesInTransit.ContainsError())
                return repositoriesInTransit;
            foreach (ITransferJob transferJob in jobs)
            {
                if (transferJob.Status != TransferStatus.Error)
                {
                    ChangeSet changeSet = service.Get<ChangeSet>(transferJob.ID);
                    if (changeSet != null && !inTransit.Contains(changeSet.Name))
                        inTransit.Add(changeSet.Name);
                }
            }
            return repositoriesInTransit;
        }

        public ErrorList GetJobs(out List<ITransferJob> jobs, bool allUsers)
        {
            jobs = new List<ITransferJob>();
            ErrorList jobs1 = new ErrorList();
            IBackgroundCopyManager o = (IBackgroundCopyManager)new BackgroundCopyManager();
            IEnumBackgroundCopyJobs ppenum = (IEnumBackgroundCopyJobs)null;
            try
            {
                o.EnumJobs(allUsers ? 1U : 0U, out ppenum);
                uint puCount;
                ppenum.GetCount(out puCount);
                for (int index = 0; (long)index < (long)puCount; ++index)
                {
                    uint pceltFetched = 0;
                    IBackgroundCopyJob rgelt;
                    ppenum.Next(1U, out rgelt, out pceltFetched);
                    Guid pVal;
                    rgelt.GetId(out pVal);
                    jobs.Add((ITransferJob)new TransferJob(pVal, rgelt));
                    Marshal.ReleaseComObject((object)rgelt);
                }
                jobs.Sort((Comparison<ITransferJob>)((lhs, rhs) =>
                {
                    if (lhs.FinishTime.HasValue && rhs.FinishTime.HasValue)
                        return lhs.FinishTime.Value.CompareTo(rhs.FinishTime.Value);
                    if (lhs.FinishTime.HasValue && !rhs.FinishTime.HasValue)
                        return 1;
                    return !lhs.FinishTime.HasValue && rhs.FinishTime.HasValue ? -1 : lhs.StartTime.CompareTo(rhs.StartTime);
                }));
            }
            catch (COMException ex)
            {
                jobs1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("B999", "Error creating BITS job.", (Exception)ex));
                jobs1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("B999", "Error accesing BITS job.", (Exception)new Win32Exception(Marshal.GetLastWin32Error())));
            }
            finally
            {
                Marshal.ReleaseComObject((object)o);
                if (ppenum != null)
                    Marshal.ReleaseComObject((object)ppenum);
            }
            return jobs1;
        }

        public ErrorList CreateDownloadJob(string name, out ITransferJob job)
        {
            job = (ITransferJob)null;
            ErrorList downloadJob = new ErrorList();
            IBackgroundCopyManager o = (IBackgroundCopyManager)new BackgroundCopyManager();
            IBackgroundCopyJob ppJob = (IBackgroundCopyJob)null;
            try
            {
                Guid pJobId;
                o.CreateJob(name, BG_JOB_TYPE.BG_JOB_TYPE_DOWNLOAD, out pJobId, out ppJob);
                job = (ITransferJob)new TransferJob(pJobId, ppJob);
            }
            catch (COMException ex)
            {
                downloadJob.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("B999", "Error creating BITS job.", (Exception)ex));
            }
            finally
            {
                Marshal.ReleaseComObject((object)o);
                if (ppJob != null)
                    Marshal.ReleaseComObject((object)ppJob);
            }
            return downloadJob;
        }

        public ErrorList CreateUploadJob(string name, out ITransferJob job)
        {
            job = (ITransferJob)null;
            ErrorList uploadJob = new ErrorList();
            IBackgroundCopyManager o = (IBackgroundCopyManager)new BackgroundCopyManager();
            IBackgroundCopyJob ppJob = (IBackgroundCopyJob)null;
            try
            {
                Guid pJobId;
                o.CreateJob(name, BG_JOB_TYPE.BG_JOB_TYPE_UPLOAD, out pJobId, out ppJob);
                job = (ITransferJob)new TransferJob(pJobId, ppJob);
            }
            catch (COMException ex)
            {
                uploadJob.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("B999", "Error creating BITS job.", (Exception)ex));
            }
            finally
            {
                Marshal.ReleaseComObject((object)o);
                if (ppJob != null)
                    Marshal.ReleaseComObject((object)ppJob);
            }
            return uploadJob;
        }

        public ErrorList GetJob(Guid id, out ITransferJob job)
        {
            ErrorList job1 = new ErrorList();
            job = (ITransferJob)null;
            try
            {
                job = (ITransferJob)new TransferJob(id);
            }
            catch (COMException ex)
            {
                job1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("B999", "Error accesing BITS job.", (Exception)ex));
                job1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("B999", "Error accesing BITS job.", (Exception)new Win32Exception(Marshal.GetLastWin32Error())));
            }
            return job1;
        }

        public bool SetMaxBandwidthWhileWithInSchedule(int i)
        {
            int num = this.MaxBandwidthWhileWithInSchedule != i ? 1 : 0;
            this.MaxBandwidthWhileWithInSchedule = i;
            return num != 0;
        }

        public int MaxBandwidthWhileWithInSchedule
        {
            get => BandwidthUsageSettings.MaxBandwidthWhileWithInSchedule;
            private set => BandwidthUsageSettings.MaxBandwidthWhileWithInSchedule = value;
        }

        public bool SetMaxBandwidthWhileOutsideOfSchedule(int max)
        {
            bool flag = false;
            try
            {
                flag = this.MaxBandwidthWhileOutsideOfSchedule != max;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error Accessing BITS settings.", ex);
            }
            this.MaxBandwidthWhileOutsideOfSchedule = max;
            return flag;
        }

        public int MaxBandwidthWhileOutsideOfSchedule
        {
            get => BandwidthUsageSettings.MaxBandwidthWhileWithInSchedule;
            private set => BandwidthUsageSettings.MaxBandwidthWhileWithInSchedule = value;
        }

        public bool SetStartOfScheduleInHoursFromMidnight(byte b)
        {
            bool flag = false;
            try
            {
                flag = (int)this.StartOfScheduleInHoursFromMidnight != (int)b;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error Accessing BITS settings.", ex);
            }
            this.StartOfScheduleInHoursFromMidnight = b;
            return flag;
        }

        public byte StartOfScheduleInHoursFromMidnight
        {
            get => BandwidthUsageSettings.StartOfScheduleInHoursFromMidnight;
            private set => BandwidthUsageSettings.StartOfScheduleInHoursFromMidnight = value;
        }

        public bool SetEndOfScheduleInHoursFromMidnight(byte b)
        {
            bool flag = false;
            try
            {
                flag = (int)this.EndOfScheduleInHoursFromMidnight != (int)b;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error Accessing BITS settings.", ex);
            }
            this.EndOfScheduleInHoursFromMidnight = b;
            return flag;
        }

        public byte EndOfScheduleInHoursFromMidnight
        {
            get => BandwidthUsageSettings.EndOfScheduleInHoursFromMidnight;
            private set => BandwidthUsageSettings.EndOfScheduleInHoursFromMidnight = value;
        }

        public bool SetUseSystemMaxOutsideOfSchedule(bool flag)
        {
            bool flag1 = false;
            try
            {
                flag1 = this.UseSystemMaxOutsideOfSchedule != flag;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error Accessing BITS settings.", ex);
            }
            this.UseSystemMaxOutsideOfSchedule = flag;
            return flag1;
        }

        public bool UseSystemMaxOutsideOfSchedule
        {
            get => BandwidthUsageSettings.UseSystemMaxOutsideOfSchedule;
            private set => BandwidthUsageSettings.UseSystemMaxOutsideOfSchedule = value;
        }

        public bool SetEnableMaximumBandwitdthThrottle(bool flag)
        {
            bool flag1 = false;
            try
            {
                flag1 = this.EnableMaximumBandwitdthThrottle != flag;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error Accessing BITS settings.", ex);
            }
            this.EnableMaximumBandwitdthThrottle = flag;
            return flag1;
        }

        public bool EnableMaximumBandwitdthThrottle
        {
            get => BandwidthUsageSettings.EnableMaximumBandwitdthThrottle;
            private set => BandwidthUsageSettings.EnableMaximumBandwitdthThrottle = value;
        }

        private TransferService()
        {
        }
    }
}
