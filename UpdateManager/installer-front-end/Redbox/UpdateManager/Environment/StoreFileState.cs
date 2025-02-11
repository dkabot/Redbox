using System;

namespace Redbox.UpdateManager.Environment
{
    internal class StoreFileState
    {
        public StoreFilePollRequest StoreFilePollRequest { get; set; }

        public string Destination { get; set; }

        public FileState State { get; set; }

        public string StateText => Enum.GetName(typeof(FileState), (object)this.State);

        public DateTime Timestamp { get; set; }
    }
}
