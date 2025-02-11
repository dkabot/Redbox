using System;
using System.Diagnostics;

namespace Redbox.Core
{
    public class ExecutionTimer : IDisposable
    {
        public ExecutionTimer()
        {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }

        public TimeSpan Elapsed => Stopwatch.Elapsed;

        public long ElapsedTicks => Stopwatch.ElapsedTicks;

        public long ElapsedMilliseconds => Stopwatch.ElapsedMilliseconds;

        private Stopwatch Stopwatch { get; }

        public void Dispose()
        {
            if (!Stopwatch.IsRunning)
                return;
            Stopwatch.Stop();
        }

        public void Stop()
        {
            Stopwatch.Stop();
        }

        public void Start()
        {
            Stopwatch.Start();
        }

        public void StartNew()
        {
            Stopwatch.StartNew();
        }
    }
}