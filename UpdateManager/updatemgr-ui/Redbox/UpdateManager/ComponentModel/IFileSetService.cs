using Redbox.UpdateService.Model;
using System.Collections.Generic;

namespace Redbox.UpdateManager.ComponentModel
{
    internal interface IFileSetService
    {
        ErrorList ProcessPollReply(List<PollReply> pollReplyList);

        void MarkRebootRequired();

        bool RebootRequired { get; }
    }
}
