using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Redbox.UpdateManager.Environment
{
    internal class PollService : IPollService
    {
        private IPoll m_poll;

        public static PollService Instance => Singleton<PollService>.Instance;

        public void Initialize(IPoll poll)
        {
            this.m_poll = poll;
            if (ServiceLocator.Instance.GetService<IPollService>() != null)
                return;
            ServiceLocator.Instance.AddService(typeof(IPollService), (object)this);
        }

        public ErrorList ServerPoll() => this.ServerPoll(false);

        public ErrorList ServerPoll(bool suppressHealthUpdate)
        {
            return this.ServerPoll(this.m_poll, suppressHealthUpdate);
        }

        public ErrorList ServerPoll(IPoll poll) => this.ServerPoll(poll, false);

        public ErrorList ServerPoll(IPoll poll, bool suppressHealthUpdate)
        {
            ErrorList errorList = new ErrorList();
            LogHelper.Instance.Log("Entering PollService.ServerPoll, using poll type " + (object)poll.GetType());
            try
            {
                List<PollRequest> pollRequestList1;
                ErrorList pollRequest1 = ConfigFileService.Instance.GetPollRequest(out pollRequestList1);
                if (errorList.ContainsError())
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)pollRequest1);
                List<PollRequest> pollRequestList2;
                ErrorList pollRequest2 = StoreInfoService.Instance.GetPollRequest(out pollRequestList2);
                if (pollRequest2.ContainsError())
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)pollRequest2);
                List<PollRequest> pollRequests;
                ErrorList pollRequest3 = StatusMessageService.Instance.GetPollRequest(out pollRequests);
                if (pollRequest3.ContainsError())
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)pollRequest3);
                List<PollRequest> pollRequestList3;
                ErrorList pollRequest4 = TransferStatisticService.Instance.GetPollRequest(out pollRequestList3);
                if (pollRequest4.ContainsError())
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)pollRequest4);
                List<PollRequest> pollRequestList4;
                ErrorList pollRequest5 = StoreFileService.Instance.GetPollRequest(out pollRequestList4);
                if (pollRequest5.ContainsError())
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)pollRequest5);
                IFileSetService service1 = ServiceLocator.Instance.GetService<IFileSetService>();
                List<PollRequest> pollRequestList5;
                ErrorList pollRequest6 = ((IPollRequestReply)service1).GetPollRequest(out pollRequestList5);
                if (pollRequest6.ContainsError())
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)pollRequest6);
                pollRequestList1.AddRange((IEnumerable<PollRequest>)pollRequestList5);
                pollRequestList1.AddRange((IEnumerable<PollRequest>)pollRequestList4);
                pollRequestList1.AddRange((IEnumerable<PollRequest>)pollRequestList3);
                pollRequestList1.AddRange((IEnumerable<PollRequest>)pollRequests);
                pollRequestList1.AddRange((IEnumerable<PollRequest>)pollRequestList2);
                List<PollReply> pollReplyList;
                ErrorList collection1 = poll.Poll(pollRequestList1, out pollReplyList);
                if (collection1.ContainsError())
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)collection1);
                if (!suppressHealthUpdate)
                    ServiceLocator.Instance.GetService<IHealthService>().Update("UPDATE_SERVICE_SERVERPOLL");
                if (pollReplyList != null)
                {
                    if (pollReplyList.Count > 0)
                    {
                        ErrorList collection2 = ConfigFileService.Instance.ProcessPollReply(pollReplyList.ToList<PollReply>().Where<PollReply>((Func<PollReply, bool>)(pr => pr.PollReplyType == PollReplyType.UpdateConfigFiles || pr.PollReplyType == PollReplyType.ConfigFileChangeSet)).ToList<PollReply>());
                        if (collection2.ContainsError())
                            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)collection2);
                        ErrorList collection3 = StoreInfoService.Instance.ProcessPollReply(pollReplyList.ToList<PollReply>().Where<PollReply>((Func<PollReply, bool>)(pr => pr.PollReplyType == PollReplyType.UpdateStoreInfo || pr.PollReplyType == PollReplyType.StoreInfoChangeSet)).ToList<PollReply>());
                        if (collection3.ContainsError())
                            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)collection3);
                        IDownloadFileService service2 = ServiceLocator.Instance.GetService<IDownloadFileService>();
                        if (service2 != null)
                            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)service2.ProcessPollReply(pollReplyList.Where<PollReply>((Func<PollReply, bool>)(pr => pr.PollReplyType == PollReplyType.DownloadFile)).ToList<PollReply>()));
                        IEnumerable<PollReply> source1 = pollReplyList.Where<PollReply>((Func<PollReply, bool>)(pr => pr.PollReplyType == PollReplyType.StoreFileChangeSet));
                        errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)StoreFileService.Instance.ProcessPollReply(source1.ToList<PollReply>()));
                        IEnumerable<PollReply> source2 = pollReplyList.Where<PollReply>((Func<PollReply, bool>)(pr => pr.PollReplyType == PollReplyType.FileSetChangeSet));
                        errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)service1.ProcessPollReply(source2.ToList<PollReply>()));
                    }
                }
            }
            finally
            {
                LogHelper.Instance.Log("Exiting PollService.ServerPoll");
            }
            return errorList;
        }

        private PollService()
        {
        }
    }
}
