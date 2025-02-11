using System;

namespace Redbox.HAL.Script.Framework
{
    internal sealed class QuickReturnCounters
    {
        private readonly string[] Counters;

        internal QuickReturnCounters()
        {
            Counters = new string[3]
            {
                "QuickReturnAttention",
                "QuickReturnNoInsertDisk",
                "QuickReturnSuccessful"
            };
        }

        internal void DoForeach(Action<string> action)
        {
            foreach (var counter in Counters)
                action(counter);
        }
    }
}