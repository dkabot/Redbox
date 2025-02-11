using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Redbox.UpdateManager.Environment
{
    internal class WorkService : IPollRequestReply
    {
        private Timer m_workTimer;
        private bool m_isRunning;
        private int m_inDoWork;
        private const string WorkExtension = ".workdat";
        private const string ScriptLabel = "script";
        private const string InCompleteLabel = ".incomplete";

        public static WorkService Instance => Singleton<WorkService>.Instance;

        public ErrorList Initialize()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                this.m_workTimer = new Timer((TimerCallback)(o => this.DoWork()));
                this.m_isRunning = false;
                LogHelper.Instance.Log("Initialized the work service", LogEntryType.Info);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HS999", "An unhandled exception occurred while initializing the work service.", ex));
            }
            return errorList;
        }

        public ErrorList Start()
        {
            ErrorList errorList = new ErrorList();
            if (this.m_isRunning)
            {
                LogHelper.Instance.Log("The work service is already running.", LogEntryType.Info);
                return errorList;
            }
            try
            {
                this.m_workTimer.Change(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0));
                this.m_isRunning = true;
                LogHelper.Instance.Log("Starting the work service.", LogEntryType.Info);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HS998", "An unhandled exception occurred while starting the work service.", ex));
            }
            return errorList;
        }

        public ErrorList Stop()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                this.m_workTimer.Change(-1, -1);
                this.m_isRunning = false;
                LogHelper.Instance.Log("Stopping the work service.", LogEntryType.Info);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HS998", "An unhandled exception occurred while stopping the work service.", ex));
            }
            return errorList;
        }

        public ErrorList GetPollRequest(out List<PollRequest> pollRequestList)
        {
            ErrorList pollRequest1 = new ErrorList();
            pollRequestList = new List<PollRequest>();
            IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
            string str = ServiceLocator.Instance.GetService<IUpdateService>().StoreNumber();
            for (WorkService.ResultEntry resultEntry = service.Peek<WorkService.ResultEntry>("script"); resultEntry != null; resultEntry = service.Peek<WorkService.ResultEntry>("script"))
            {
                Dictionary<object, object> dictionary = resultEntry.Entry.ToObject<Dictionary<object, object>>();
                WorkResultPollRequestDTO instance = new WorkResultPollRequestDTO()
                {
                    StoreNumber = str,
                    ScriptID = resultEntry.ID,
                    Results = dictionary
                };
                PollRequest pollRequest2 = new PollRequest()
                {
                    PollRequestType = PollRequestType.WorkResult,
                    Data = instance.ToJson()
                };
                pollRequestList.Add(pollRequest2);
                service.Dequeue("script");
                LogHelper.Instance.Log("Sent result for script {0} to server.", (object)resultEntry.ID);
            }
            return pollRequest1;
        }

        public ErrorList ProcessPollReply(List<PollReply> pollReplyList)
        {
            ErrorList errorList = new ErrorList();
            if (!errorList.ContainsError() && pollReplyList != null)
            {
                if (pollReplyList.Any<PollReply>())
                {
                    try
                    {
                        IEnumerable<ClientWorkScript> clientWorkScripts = pollReplyList.Select<PollReply, ClientWorkScript>((Func<PollReply, ClientWorkScript>)(pollReply => pollReply.Data.ToObject<ClientWorkScript>()));
                        IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
                        List<Guid> o1 = new List<Guid>();
                        foreach (ClientWorkScript o2 in clientWorkScripts)
                        {
                            Guid guid = Guid.NewGuid();
                            o1.Add(guid);
                            service.Set(guid.ToString() + ".workdat", (object)o2);
                            LogHelper.Instance.Log("Script Id: {0} has been put on the work queue.", (object)o2.ID);
                        }
                        service.Set(".incomplete", (object)o1);
                    }
                    catch (Exception ex)
                    {
                        errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception occurred in Updater.DoWork.", ex));
                    }
                    return errorList;
                }
            }
            return errorList;
        }

        private WorkService()
        {
        }

        private void DoWork()
        {
            try
            {
                if (!this.m_isRunning)
                    LogHelper.Instance.Log("The work service is not running.", LogEntryType.Info);
                else if (Interlocked.CompareExchange(ref this.m_inDoWork, 1, 0) == 1)
                {
                    LogHelper.Instance.Log("Already in DoWork", LogEntryType.Info);
                }
                else
                {
                    try
                    {
                        WorkService.DoIncompleteAndQueuedWork().ToLogHelper();
                    }
                    finally
                    {
                        this.m_inDoWork = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("There was an exception in DoWork()", ex);
            }
        }

        private static ErrorList DoIncompleteAndQueuedWork()
        {
            ErrorList errorList = new ErrorList();
            IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
            List<Guid> ids = ServiceLocator.Instance.GetService<IDataStoreService>().Get<List<Guid>>(".incomplete");
            if (ids != null && ids.Count > 0)
            {
                LogHelper.Instance.Log("Doing {0} incomplete and queued work items.", (object)ids.Count);
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)service.DoInCompleteWork((IEnumerable<Guid>)ids));
            }
            else
                LogHelper.Instance.Log("No incomplete or queued work items found.");
            return errorList;
        }

        private class ResultEntry
        {
            public string Entry { get; set; }

            public long ID { get; set; }
        }
    }
}
