using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage
{
    internal sealed class DumpbinItem : IDumpBinInventoryItem
    {
        internal DumpbinItem(string id, DateTime dt)
        {
            ID = id;
            PutTime = dt;
        }

        public DateTime PutTime { get; }

        public string ID { get; }
    }
}