using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Client;
using Redbox.UpdateService.Model;
using System;
using System.Collections.Generic;

namespace Redbox.UpdateManager.Environment
{
    internal class TransferStatisticService : IPollRequestReply
    {
        public static TransferStatisticService Instance => Singleton<TransferStatisticService>.Instance;

        public ErrorList GetPollRequest(out List<PollRequest> pollRequestList)
        {
            pollRequestList = new List<PollRequest>();
            ErrorList pollRequest1 = new ErrorList();
            try
            {
                IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
                for (TransferStatisticReport instance = service.Peek<TransferStatisticReport>("statistic"); instance != null; instance = service.Peek<TransferStatisticReport>("statistic"))
                {
                    PollRequest pollRequest2 = new PollRequest()
                    {
                        PollRequestType = PollRequestType.TransferStatisticReport,
                        Data = instance.ToJson()
                    };
                    pollRequestList.Add(pollRequest2);
                    LogHelper.Instance.Log("Enqueued transfer statistics for revision: {0}", (object)instance.ChangeSet);
                    service.Dequeue("statistic");
                }
            }
            catch (Exception ex)
            {
                pollRequest1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StatusMessageService", "Unhandled exception occurred in GetPollRequest.", ex));
            }
            return pollRequest1;
        }

        public ErrorList ProcessPollReply(List<PollReply> pollReplyList) => new ErrorList();

        private TransferStatisticService()
        {
        }
    }
}
