using Redbox.UpdateService.Model;
using System.Collections.Generic;

namespace Redbox.UpdateManager.ComponentModel
{
    internal interface IPollRequestReply
    {
        ErrorList GetPollRequest(out List<PollRequest> pollRequestList);

        ErrorList ProcessPollReply(List<PollReply> pollReplyList);
    }
}
