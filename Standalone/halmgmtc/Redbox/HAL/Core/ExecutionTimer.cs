using System;
using System.Diagnostics;

namespace Redbox.HAL.Core
{
    public sealed class ExecutionTimer : IDisposable
    {
        private readonly Stopwatch Stopwatch;

        public ExecutionTimer()
        {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }

        public TimeSpan Elapsed => Stopwatch.Elapsed;

        public long ElapsedTicks => Stopwatch.ElapsedTicks;

        public long ElapsedMilliseconds => Stopwatch.ElapsedMilliseconds;

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