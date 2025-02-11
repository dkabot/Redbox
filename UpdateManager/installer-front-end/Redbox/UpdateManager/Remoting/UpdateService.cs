using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Client;
using Redbox.UpdateService.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;

namespace Redbox.UpdateManager.Remoting
{
    internal class UpdateService : IUpdateService
    {
        private string m_url;
        private TimeSpan m_timeout = TimeSpan.FromSeconds(30.0);
        private const string ResultTableName = "Results";
        private const string InCompleteLabel = ".incomplete";
        private const string ScriptLabel = "script";
        private string m_storeNumber;
        private int m_minimumRetryDelay = 60;
        private const string WorkExtension = ".workdat";

        public static Redbox.UpdateManager.Remoting.UpdateService Instance
        {
            get => Singleton<Redbox.UpdateManager.Remoting.UpdateService>.Instance;
        }

        public void Initialize(
          string url,
          TimeSpan timeout,
          string storeNumber,
          int minimumRetryDelay)
        {
            this.m_url = url;
            this.m_storeNumber = storeNumber;
            this.m_timeout = timeout;
            this.m_minimumRetryDelay = minimumRetryDelay < 60 ? 60 : minimumRetryDelay;
            ServiceLocator.Instance.AddService(typeof(IUpdateService), (object)this);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ForcePublish(string name)
        {
            using (Redbox.UpdateService.Client.UpdateService service = this.GetService())
                return Redbox.UpdateManager.Remoting.UpdateService.IPC((IEnumerable<Redbox.IPC.Framework.Error>)service.ForcePublishRun(name).Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList GetSubscriptionState(
          string name,
          out DateTime lastRun,
          out Redbox.UpdateManager.ComponentModel.SubscriptionState state)
        {
            lastRun = DateTime.Now;
            state = Redbox.UpdateManager.ComponentModel.SubscriptionState.Idle;
            using (Redbox.UpdateService.Client.UpdateService service = this.GetService())
            {
                SubscriptionStatus state1;
                ClientCommandResult subscriptionStatus = service.GetSubscriptionStatus(name, out state1);
                if (subscriptionStatus.Success)
                {
                    lastRun = state1.ModifiedOn;
                    state = state1.State == Redbox.UpdateService.Client.SubscriptionState.Processing ? Redbox.UpdateManager.ComponentModel.SubscriptionState.Processing : Redbox.UpdateManager.ComponentModel.SubscriptionState.Idle;
                }
                return Redbox.UpdateManager.Remoting.UpdateService.IPC((IEnumerable<Redbox.IPC.Framework.Error>)subscriptionStatus.Errors);
            }
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList UploadFile(string path) => new Redbox.UpdateManager.ComponentModel.ErrorList();

        public Redbox.UpdateManager.ComponentModel.ErrorList AddGroupToRepository(
          string group,
          string name)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList repository = new Redbox.UpdateManager.ComponentModel.ErrorList();
            using (Redbox.UpdateService.Client.UpdateService service = this.GetService())
            {
                ClientCommandResult group1 = service.LinkRepositoryToGroup(group, name, "UpdateService.Kernel", "Update Service", out StoreGroup _);
                if (!group1.Success)
                    repository.AddRange(Redbox.UpdateManager.Remoting.UpdateService.IPC((IEnumerable<Redbox.IPC.Framework.Error>)group1.Errors));
                return repository;
            }
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList RemoveGroupFromRepository(
          string group,
          string name)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            using (Redbox.UpdateService.Client.UpdateService service = this.GetService())
            {
                ClientCommandResult group1 = service.UnlinkRepositoryToGroup(group, name, "UpdateService.Kernel", "Update Service", out StoreGroup _);
                if (!group1.Success)
                    errorList.AddRange(Redbox.UpdateManager.Remoting.UpdateService.IPC((IEnumerable<Redbox.IPC.Framework.Error>)group1.Errors));
                return errorList;
            }
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList AddStoreToRepository(
          string number,
          string name)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList repository = new Redbox.UpdateManager.ComponentModel.ErrorList();
            using (Redbox.UpdateService.Client.UpdateService service = this.GetService())
            {
                ClientCommandResult store = service.LinkRepositoryToStore(number, name, "UpdateService.Kernel", "Update Service", out Store _);
                if (!store.Success)
                    repository.AddRange(Redbox.UpdateManager.Remoting.UpdateService.IPC((IEnumerable<Redbox.IPC.Framework.Error>)store.Errors));
                return repository;
            }
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList RemoveStoreFromRepository(
          string number,
          string name)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            using (Redbox.UpdateService.Client.UpdateService service = this.GetService())
            {
                ClientCommandResult store = service.UnlinkRepositoryToStore(number, name, "UpdateService.Kernel", "Update Service", out Store _);
                if (!store.Success)
                    errorList.AddRange(Redbox.UpdateManager.Remoting.UpdateService.IPC((IEnumerable<Redbox.IPC.Framework.Error>)store.Errors));
                return errorList;
            }
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ProcessChangeSet(
          List<ChangeSet> changeSets,
          StoreScheduleInfo storeScheduleInfo)
        {
            LogHelper.Instance.Log(string.Format("Processing {0} changes in changeset from poll reply", (object)changeSets.Count), LogEntryType.Info);
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            try
            {
                ITransferService service1 = ServiceLocator.Instance.GetService<ITransferService>();
                IDataStoreService service2 = ServiceLocator.Instance.GetService<IDataStoreService>();
                IRepositoryService service3 = ServiceLocator.Instance.GetService<IRepositoryService>();
                if (!changeSets.Any<ChangeSet>())
                {
                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("PCS303", "There were no changes in the changeset", "No changeset to process, exiting ProcessChangeSet"));
                    return errorList;
                }
                if ((service1.SetEndOfScheduleInHoursFromMidnight(storeScheduleInfo.EndOfScheduleInHoursFromMidnight) || service1.SetStartOfScheduleInHoursFromMidnight(storeScheduleInfo.StartOfScheduleInHoursFromMidnight) || service1.SetMaxBandwidthWhileOutsideOfSchedule(storeScheduleInfo.MaxBandwidthWhileOutsideOfSchedule) ? 1 : (service1.SetMaxBandwidthWhileWithInSchedule(storeScheduleInfo.MaxBandwidthWhileWithInSchedule) ? 1 : 0)) != 0)
                {
                    using (ServiceController serviceController = new ServiceController("BITS"))
                    {
                        serviceController.Stop();
                        serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(10.0));
                        serviceController.Start();
                        serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(10.0));
                    }
                }
                HashSet<string> inTransit;
                service1.GetRepositoriesInTransit(out inTransit);
                foreach (ChangeSet changeSet in changeSets)
                {
                    if (!inTransit.Contains(changeSet.Name) && service3.Subscribed(changeSet.Name))
                    {
                        ITransferJob job = (ITransferJob)null;
                        try
                        {
                            if (!service3.ContainsRepository(changeSet.Name))
                                service3.AddRepository(changeSet.Name);
                            string name = string.Format("<~{0}-{1}~>", (object)changeSet.Name, (object)changeSet.Head.Substring(0, 8));
                            errorList.AddRange(service1.CreateDownloadJob(name, out job));
                            errorList.AddRange(job.SetMinimumRetryDelay((uint)this.m_minimumRetryDelay));
                            errorList.AddRange(job.SetNoProgressTimeout(storeScheduleInfo.NoProgressTimeout));
                            errorList.AddRange(job.AddItem(changeSet.Url, Path.GetTempFileName()));
                            if (errorList.ContainsError())
                            {
                                errorList.AddRange(job.Cancel());
                            }
                            else
                            {
                                errorList.AddRange(job.Resume());
                                if (!errorList.ContainsError())
                                {
                                    service2.Set(job.ID, (object)changeSet);
                                    LogHelper.Instance.Log("BITS job started with name: {0} and guid: {1}.", (object)name, (object)job.ID);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (job != null)
                                errorList.AddRange(job.Cancel());
                            errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception occurred in Updater.StartUpdate while creating job for a change set.", ex));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception occurred in Updater.StartUpdate. The Guid on the Error Report is not the BITS job id.", ex));
            }
            return errorList;
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList StartDownloads()
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            try
            {
                ITransferService service1 = ServiceLocator.Instance.GetService<ITransferService>();
                IDataStoreService service2 = ServiceLocator.Instance.GetService<IDataStoreService>();
                IRepositoryService service3 = ServiceLocator.Instance.GetService<IRepositoryService>();
                List<string> allRepositories = service3.GetAllRepositories();
                Dictionary<string, string> currentFiles = new Dictionary<string, string>();
                foreach (string str in allRepositories)
                {
                    if (!string.IsNullOrEmpty(service3.GetHeadRevision(str)))
                        currentFiles.Add(str, service3.GetHeadRevision(str));
                }
                Store store;
                List<ChangeSet> jobs;
                using (Redbox.UpdateService.Client.UpdateService service4 = this.GetService())
                {
                    ClientCommandResult changeSet = service4.GetChangeSet(this.m_storeNumber, currentFiles, out store, out jobs);
                    if (!changeSet.Success)
                    {
                        errorList.AddRange(Redbox.UpdateManager.Remoting.UpdateService.IPC((IEnumerable<Redbox.IPC.Framework.Error>)changeSet.Errors));
                        return errorList;
                    }
                }
                if (jobs.Count < 1 || errorList.ContainsError())
                    return errorList;
                if ((service1.SetEndOfScheduleInHoursFromMidnight(store.EndOfScheduleInHoursFromMidnight) || service1.SetEndOfScheduleInHoursFromMidnight(store.EndOfScheduleInHoursFromMidnight) || service1.SetStartOfScheduleInHoursFromMidnight(store.StartOfScheduleInHoursFromMidnight) || service1.SetMaxBandwidthWhileOutsideOfSchedule((int)store.MaxBandwidthWhileOutsideOfSchedule) ? 1 : (service1.SetMaxBandwidthWhileWithInSchedule((int)store.MaxBandwidthWhileWithInSchedule) ? 1 : 0)) != 0)
                {
                    using (ServiceController serviceController = new ServiceController("BITS"))
                    {
                        serviceController.Stop();
                        serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(10.0));
                        serviceController.Start();
                        serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(10.0));
                    }
                }
                HashSet<string> inTransit;
                service1.GetRepositoriesInTransit(out inTransit);
                foreach (ChangeSet o in jobs)
                {
                    if (!inTransit.Contains(o.Name) && service3.Subscribed(o.Name))
                    {
                        ITransferJob job = (ITransferJob)null;
                        try
                        {
                            if (!service3.ContainsRepository(o.Name))
                                service3.AddRepository(o.Name);
                            string name = string.Format("<~{0}-{1}~>", (object)o.Name, (object)o.Head.Substring(0, 8));
                            errorList.AddRange(service1.CreateDownloadJob(name, out job));
                            errorList.AddRange(job.SetMinimumRetryDelay((uint)this.m_minimumRetryDelay));
                            errorList.AddRange(job.SetNoProgressTimeout(store.NoProgressTimeout));
                            errorList.AddRange(job.AddItem(o.Url, Path.GetTempFileName()));
                            if (errorList.ContainsError())
                            {
                                errorList.AddRange(job.Cancel());
                            }
                            else
                            {
                                errorList.AddRange(job.Resume());
                                if (!errorList.ContainsError())
                                {
                                    service2.Set(job.ID, (object)o);
                                    LogHelper.Instance.Log("BITS job started with name: {0} and guid: {1}.", (object)name, (object)job.ID);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (job != null)
                                errorList.AddRange(job.Cancel());
                            errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception occurred in UpdateService.StartDownloads while creating job for a change set.", ex));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception occurred in UpdateService.StartDownloads.", ex));
            }
            return errorList;
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList FinishDownloads()
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            ITransferService service1 = ServiceLocator.Instance.GetService<ITransferService>();
            IDataStoreService service2 = ServiceLocator.Instance.GetService<IDataStoreService>();
            List<ITransferJob> jobs;
            errorList.AddRange(service1.GetJobs(out jobs, false));
            if (errorList.ContainsError())
                return errorList;
            foreach (ITransferJob transferJob in jobs)
            {
                if (transferJob.Name.StartsWith("<~") && transferJob.Name.EndsWith("~>"))
                {
                    switch (transferJob.Status)
                    {
                        case TransferStatus.Suspended:
                            errorList.AddRange(transferJob.Cancel());
                            continue;
                        case TransferStatus.Error:
                        case TransferStatus.Transferred:
                            errorList.AddRange(transferJob.JobType == TransferJobType.Download ? this.FinishDownload(transferJob.ID) : this.FinishUpload(transferJob.ID));
                            continue;
                        default:
                            if (service2.Get<ChangeSet>(transferJob.ID) == null)
                            {
                                errorList.AddRange(transferJob.Cancel());
                                continue;
                            }
                            continue;
                    }
                }
            }
            if (jobs.Count == 0)
                service2.CleanUp();
            return errorList;
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList FinishUpload(Guid jobId)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            ITransferService service1 = ServiceLocator.Instance.GetService<ITransferService>();
            IDataStoreService service2 = ServiceLocator.Instance.GetService<IDataStoreService>();
            IQueueService service3 = ServiceLocator.Instance.GetService<IQueueService>();
            try
            {
                Upload upload = service2.Get<Upload>(jobId);
                if (upload == null)
                {
                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("M001", string.Format("Job file for job: {0}", (object)jobId), "Job file is missing the bits jobs will be canceled."));
                    ITransferJob job;
                    errorList.AddRange(service1.GetJob(jobId, out job));
                    if (job != null && !errorList.ContainsError())
                        errorList.AddRange(job.Cancel());
                    return errorList;
                }
                ITransferJob job1;
                errorList.AddRange(service1.GetJob(jobId, out job1));
                if (errorList.ContainsError())
                    return errorList;
                UploadReport entry;
                if (job1.Status == TransferStatus.Error)
                {
                    entry = new UploadReport()
                    {
                        AverageSpeedInKPS = 0.0,
                        ID = upload.ID,
                        End = job1.FinishTime.HasValue ? job1.FinishTime.Value : DateTime.UtcNow,
                        StoreNumber = this.m_storeNumber,
                        Success = false,
                        TotalBytesTransfered = job1.TotalBytesTransfered
                    };
                    errorList.AddRange(job1.Cancel());
                }
                else
                {
                    DateTime dateTime = job1.FinishTime.HasValue ? job1.FinishTime.Value : DateTime.UtcNow;
                    double num = (double)job1.TotalBytesTransfered / (dateTime - job1.StartTime).TotalSeconds;
                    UploadReport uploadReport = new UploadReport();
                    uploadReport.AverageSpeedInKPS = num;
                    uploadReport.ID = upload.ID;
                    DateTime? finishTime = job1.FinishTime;
                    DateTime utcNow;
                    if (!finishTime.HasValue)
                    {
                        utcNow = DateTime.UtcNow;
                    }
                    else
                    {
                        finishTime = job1.FinishTime;
                        utcNow = finishTime.Value;
                    }
                    uploadReport.End = utcNow;
                    uploadReport.StoreNumber = this.m_storeNumber;
                    uploadReport.Success = true;
                    uploadReport.TotalBytesTransfered = job1.TotalBytesTransfered;
                    entry = uploadReport;
                    errorList.AddRange(job1.Complete());
                }
                service3.Enqueue("upload-report", (object)entry);
                service2.Delete(jobId);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception occurred in Updater.FinishUpdate.", ex));
                return errorList;
            }
            return errorList;
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList FinishDownload(Guid jobId)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            ITransferService service1 = ServiceLocator.Instance.GetService<ITransferService>();
            IRepositoryService service2 = ServiceLocator.Instance.GetService<IRepositoryService>();
            IDataStoreService service3 = ServiceLocator.Instance.GetService<IDataStoreService>();
            IQueueService service4 = ServiceLocator.Instance.GetService<IQueueService>();
            ChangeSet changeSet;
            ITransferJob job1;
            try
            {
                changeSet = service3.Get<ChangeSet>(jobId);
                if (changeSet == null)
                {
                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("M001", string.Format("Job file for job: {0}", (object)jobId), "Job file is missing the bits jobs will be canceled."));
                    ITransferJob job2;
                    errorList.AddRange(service1.GetJob(jobId, out job2));
                    if (job2 != null)
                        errorList.AddRange(job2.Status == TransferStatus.Transferred ? job2.Complete() : job2.Cancel());
                    return errorList;
                }
                errorList.AddRange(service1.GetJob(jobId, out job1));
                if (errorList.ContainsError())
                    return errorList;
                if (job1.Status == TransferStatus.Error)
                {
                    TransferStatisticReport entry = new TransferStatisticReport()
                    {
                        Repository = changeSet.Name,
                        AverageSpeedInKPS = 0.0,
                        ChangeSet = changeSet.Head,
                        End = job1.FinishTime.HasValue ? job1.FinishTime.Value : DateTime.UtcNow,
                        StoreNumber = this.m_storeNumber,
                        Success = false,
                        TotalBytesTransfered = job1.TotalBytesTransfered
                    };
                    service4.Enqueue("statistic", (object)entry);
                    errorList.AddRange(job1.Cancel());
                    return errorList;
                }
                List<ITransferItem> items;
                errorList.AddRange(job1.GetItems(out items));
                if (errorList.ContainsError())
                    return errorList;
                if (items.Count != 1)
                {
                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("D999", string.Format("Job {0} has {1} items. It have 1 item.", (object)job1.ID, (object)items.Count), "This download will be canceled."));
                    service3.Delete(jobId);
                    return errorList;
                }
                errorList.AddRange(job1.Complete());
                if (errorList.ContainsError())
                {
                    job1.Cancel();
                    service3.Delete(jobId);
                    return errorList;
                }
                ITransferItem transferItem = items[0];
                string archive = Path.Combine(transferItem.Path, transferItem.Name);
                if (!string.IsNullOrEmpty(changeSet.ContentHash))
                {
                    string path = archive;
                    using (FileStream inputStream = File.OpenRead(path))
                    {
                        string asciishA1Hash = inputStream.ToASCIISHA1Hash();
                        if (asciishA1Hash != changeSet.ContentHash)
                        {
                            errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("D999", string.Format("{0} should have hash {1} but it hash {2}", (object)path, (object)changeSet.ContentHash, (object)asciishA1Hash), "This download failed and will need to be restarted."));
                            return errorList;
                        }
                        LogHelper.Instance.Log("{0} matched the server hash of {1}.", (object)path, (object)changeSet.ContentHash);
                    }
                }
                service2.UnpackChangeSet(changeSet.Name, jobId, archive);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception occurred in Updater.FinishUpdate.", ex));
                service3.Delete(jobId);
                return errorList;
            }
            DateTime dateTime = job1.FinishTime.HasValue ? job1.FinishTime.Value : DateTime.UtcNow;
            double num = (double)job1.TotalBytesTransfered / (dateTime - job1.StartTime).TotalSeconds;
            try
            {
                service2.AddRevisions(changeSet.Name, changeSet.Revisions);
                if (!string.IsNullOrEmpty(changeSet.Label))
                    service2.SetLabel(changeSet.Name, changeSet.Revisions[changeSet.Revisions.Count - 1], changeSet.Label);
                List<DeltaItem> deltaItemList = new List<DeltaItem>();
                foreach (ChangeSetItem changeSetItem in changeSet.Items)
                {
                    DeltaItem deltaItem = new DeltaItem()
                    {
                        Revision = changeSetItem.Revision,
                        IsSeed = changeSetItem.IsFirst,
                        IsPlaceHolder = changeSetItem.Composite,
                        ContentHash = changeSetItem.ContentHash,
                        TargetName = changeSetItem.TargetName,
                        TargetPath = changeSetItem.TargetPath,
                        VersionHash = changeSetItem.VersionHash
                    };
                    deltaItemList.Add(deltaItem);
                }
                service2.AddDelta(changeSet.Name, deltaItemList);
                LogHelper.Instance.Log("Finished Add patch for ChangeSetName: {0} TransferJobId: {1}", (object)changeSet.Name, (object)job1.ID);
                TransferStatisticReport transferStatisticReport = new TransferStatisticReport();
                transferStatisticReport.Repository = changeSet.Name;
                transferStatisticReport.AverageSpeedInKPS = num;
                transferStatisticReport.ChangeSet = changeSet.Head;
                DateTime? finishTime = job1.FinishTime;
                DateTime utcNow;
                if (!finishTime.HasValue)
                {
                    utcNow = DateTime.UtcNow;
                }
                else
                {
                    finishTime = job1.FinishTime;
                    utcNow = finishTime.Value;
                }
                transferStatisticReport.End = utcNow;
                transferStatisticReport.StoreNumber = this.m_storeNumber;
                transferStatisticReport.Success = true;
                transferStatisticReport.TotalBytesTransfered = job1.TotalBytesTransfered;
                TransferStatisticReport entry = transferStatisticReport;
                service4.Enqueue("statistic", (object)entry);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "Error adding delta to repository.", ex));
                service2.RemoveRevisions(changeSet.Name, changeSet.Revisions);
                TransferStatisticReport transferStatisticReport = new TransferStatisticReport();
                transferStatisticReport.Repository = changeSet.Name;
                transferStatisticReport.AverageSpeedInKPS = num;
                transferStatisticReport.ChangeSet = changeSet.Head;
                DateTime? finishTime = job1.FinishTime;
                DateTime utcNow;
                if (!finishTime.HasValue)
                {
                    utcNow = DateTime.UtcNow;
                }
                else
                {
                    finishTime = job1.FinishTime;
                    utcNow = finishTime.Value;
                }
                transferStatisticReport.End = utcNow;
                transferStatisticReport.StoreNumber = this.m_storeNumber;
                transferStatisticReport.TotalBytesTransfered = job1.TotalBytesTransfered;
                TransferStatisticReport entry = transferStatisticReport;
                service4.Enqueue("statistic", (object)entry);
            }
            service3.Delete(jobId);
            return errorList;
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ServerPoll()
        {
            return ServiceLocator.Instance.GetService<IPollService>().ServerPoll();
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList Poll()
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            errorList.AddRange(this.FinishDownloads());
            errorList.AddRange(this.StartDownloads());
            return errorList;
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ClearWorkQueue()
        {
            ServiceLocator.Instance.GetService<IDataStoreService>().Delete(".incomplete");
            return new Redbox.UpdateManager.ComponentModel.ErrorList();
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList DeleteFromWorkQueue(string name)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
            try
            {
                List<Guid> o = service.Get<List<Guid>>(".incomplete");
                for (int index = 0; index < o.Count; ++index)
                {
                    if (service.Get<Work>(o[index].ToString() + ".workdat").ID == Convert.ToInt64(name))
                    {
                        o.RemoveAt(index);
                        service.Set(".incomplete", (object)o);
                        service.Delete(o[index].ToString() + ".workdat");
                        return errorList;
                    }
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception occurred in Updater.DoWork.", ex));
            }
            return errorList;
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList StartInstaller(
          string repositoryHash,
          string frontEndVersion,
          out Dictionary<string, string> response)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            response = new Dictionary<string, string>();
            using (Redbox.UpdateService.Client.UpdateService service = this.GetService())
            {
                ClientCommandResult clientCommandResult = service.StartStoreInstaller("installer", repositoryHash, frontEndVersion);
                if (clientCommandResult.Success)
                {
                    response = clientCommandResult.CommandMessages[0].ToDictionary().ToDictionary<KeyValuePair<string, object>, string, string>((Func<KeyValuePair<string, object>, string>)(k => k.Key), (Func<KeyValuePair<string, object>, string>)(v => v.Value as string));
                }
                else
                {
                    LogHelper.Instance.Log("The update service could not be contacted in StartInstaller.");
                    errorList.AddRange(clientCommandResult.Errors.Select<Redbox.IPC.Framework.Error, Redbox.UpdateManager.ComponentModel.Error>((Func<Redbox.IPC.Framework.Error, Redbox.UpdateManager.ComponentModel.Error>)(e => Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details))));
                }
            }
            return errorList;
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList FinishInstaller(
          string guid,
          Dictionary<string, string> data)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            using (Redbox.UpdateService.Client.UpdateService service = this.GetService())
            {
                foreach (KeyValuePair<string, string> keyValuePair in data)
                {
                    int num = 1;
                label_4:
                    LogHelper.Instance.Log(string.Format("Sending log to server try #{0}: {1}:{2} ", (object)num, (object)keyValuePair.Key, (object)keyValuePair.Value));
                    ClientCommandResult clientCommandResult = service.InstallerLog("installer", guid, keyValuePair.Key, keyValuePair.Value);
                    if (!clientCommandResult.Success)
                        errorList.AddRange(clientCommandResult.Errors.Select<Redbox.IPC.Framework.Error, Redbox.UpdateManager.ComponentModel.Error>((Func<Redbox.IPC.Framework.Error, Redbox.UpdateManager.ComponentModel.Error>)(e => Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details))));
                    ++num;
                    if (!clientCommandResult.Success && num <= 3)
                        goto label_4;
                }
            }
            return errorList;
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList FinishInstaller(
          string guid,
          string name,
          string value)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            using (Redbox.UpdateService.Client.UpdateService service = this.GetService())
            {
                LogHelper.Instance.Log(string.Format("Sending log to server: {0}:{1} ", (object)name, (object)value));
                ClientCommandResult clientCommandResult = service.InstallerLog("installer", guid, name, value);
                if (!clientCommandResult.Success)
                    errorList.AddRange(clientCommandResult.Errors.Select<Redbox.IPC.Framework.Error, Redbox.UpdateManager.ComponentModel.Error>((Func<Redbox.IPC.Framework.Error, Redbox.UpdateManager.ComponentModel.Error>)(e => Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details))));
            }
            return errorList;
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ExecuteScriptFile(
          string path,
          out string result)
        {
            return this.ExecuteScriptFile(path, out result, false);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ExecuteScriptFile(
          string path,
          out string result,
          bool reset)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            result = string.Empty;
            string path1 = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(path);
            if (File.Exists(path1))
                return this.ExecuteScript(File.ReadAllText(path1), out result, reset);
            errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("US9987", "Script path does not exist.", string.Format("Correct Path {0}", (object)path)));
            return errorList;
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ExecuteScript(
          string script,
          out string result)
        {
            return this.ExecuteScript(script, out result, false);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ExecuteScript(
          string script,
          out string result,
          bool reset)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            result = string.Empty;
            IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
            try
            {
                service.ExecuteChunk(script, reset);
                Dictionary<object, object> table = service.GetTable("Results");
                if (table != null)
                    result = table.ToJson();
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("US9986", "Error: unhandled exception in script.", ex));
                LogHelper.Instance.Log("(US9986), Error: unhandled exception in script.", ex);
            }
            return errorList;
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList DoInCompleteWork(IEnumerable<Guid> ids)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            List<Guid> o = new List<Guid>(ids);
            try
            {
                IDataStoreService service1 = ServiceLocator.Instance.GetService<IDataStoreService>();
                IQueueService service2 = ServiceLocator.Instance.GetService<IQueueService>();
                foreach (Guid id in ids)
                {
                    Work work = service1.Get<Work>(id.ToString() + ".workdat");
                    IKernelService service3 = ServiceLocator.Instance.GetService<IKernelService>();
                    Exception exception = (Exception)null;
                    try
                    {
                        LogHelper.Instance.Log("Executing incomplete work item with script id: {0}", (object)work.ID);
                        service3.ExecuteChunk(work.GetScriptText(), true);
                    }
                    catch (Exception ex)
                    {
                        errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception occurred in Updater.DoWork.ExecuteChunk", ex));
                        exception = ex;
                    }
                    if (exception != null)
                    {
                        LogHelper.Instance.Log("Script Id: {0} caused an exception.", (object)work.ID);
                        var instance = new
                        {
                            Success = false,
                            RequiresRetry = true,
                            exception.Message
                        };
                        service2.Enqueue("script", (object)new Redbox.UpdateManager.Remoting.UpdateService.ResultEntry()
                        {
                            ID = work.ID,
                            Entry = instance.ToJson()
                        });
                        o.Remove(id);
                        if (o.Count == 0)
                            service1.Delete(".incomplete");
                        else
                            service1.Set(".incomplete", (object)o);
                        service1.Delete(id.ToString() + ".workdat");
                        LogHelper.Instance.Log("Script id: {0} incomplete work removed", (object)work.ID);
                    }
                    else if (service3.ScriptCompleted)
                    {
                        o.Remove(id);
                        if (o.Count == 0)
                            service1.Delete(".incomplete");
                        else
                            service1.Set(".incomplete", (object)o);
                        service1.Delete(id.ToString() + ".workdat");
                        LogHelper.Instance.Log("Script id: {0} completed", (object)work.ID);
                        Dictionary<object, object> table = service3.GetTable("Results");
                        if (table != null)
                        {
                            string json = table.ToJson();
                            service2.Enqueue("script", (object)new Redbox.UpdateManager.Remoting.UpdateService.ResultEntry()
                            {
                                ID = work.ID,
                                Entry = json
                            });
                        }
                    }
                    else
                        LogHelper.Instance.Log("Script id: {0} is still incomplete.", (object)work.ID);
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception occurred in Updater.DoWork.", ex));
            }
            return errorList;
        }

        public string StoreNumber() => this.m_storeNumber;

        private UpdateService()
        {
        }

        private Redbox.UpdateService.Client.UpdateService GetService()
        {
            return Redbox.UpdateService.Client.UpdateService.GetService(this.m_url, Convert.ToInt32(this.m_timeout.TotalMilliseconds));
        }

        private static Redbox.UpdateManager.ComponentModel.ErrorList IPC(IEnumerable<Redbox.IPC.Framework.Error> errors)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            foreach (Redbox.IPC.Framework.Error error in errors)
                errorList.Add(error.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(error.Code, error.Details, error.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(error.Code, error.Details, error.Details));
            return errorList;
        }

        private class ResultEntry
        {
            public string Entry { get; set; }

            public long ID { get; set; }
        }
    }
}
