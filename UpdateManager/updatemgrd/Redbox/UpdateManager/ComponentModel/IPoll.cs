using Redbox.UpdateService.Model;
using System.Collections.Generic;

namespace Redbox.UpdateManager.ComponentModel
{
    internal interface IPoll
    {
        ErrorList Poll(List<PollRequest> pollRequestList, out List<PollReply> pollReplyList);
    }
}
