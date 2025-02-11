using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage
{
    internal sealed class CountersDescriptor : IDataTableDescriptor
    {
        internal CountersDescriptor(string dataPath, bool exclusive)
        {
            Source = Path.Combine(dataPath, "HALCounters.vdb3");
            ExclusiveReadWrite = exclusive;
            SupportsPooling = !exclusive;
        }

        public string Source { get; }

        public bool ExclusiveReadWrite { get; }

        public bool SupportsPooling { get; }

        public bool UseTransaction => false;
    }
}