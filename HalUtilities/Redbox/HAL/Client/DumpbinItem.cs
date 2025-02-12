using System;

namespace Redbox.HAL.Client
{
    internal sealed class DumpbinItem : IDumpbinItem
    {
        internal DumpbinItem(string matrix, DateTime putTime)
        {
            Matrix = matrix;
            PutTime = putTime;
        }

        public string Matrix { get; }

        public DateTime PutTime { get; }
    }
}