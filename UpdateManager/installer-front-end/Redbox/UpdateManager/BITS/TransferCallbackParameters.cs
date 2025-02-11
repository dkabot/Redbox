using Redbox.UpdateManager.ComponentModel;
using System.Collections.Generic;

namespace Redbox.UpdateManager.BITS
{
    internal class TransferCallbackParameters : ITransferCallbackParameters
    {
        public string Path { get; set; }

        public string Executable { get; set; }

        public List<string> Arguments { get; set; }
    }
}
