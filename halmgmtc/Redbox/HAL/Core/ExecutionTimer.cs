using System;
using System.Diagnostics;

namespace Redbox.HAL.Core
{
    public sealed class ExecutionTimer : IDisposable
    {
        private readonly Stopwatch Stopwatch;

        public ExecutionTimer()
        {
            this.Stopwatch = new Stopwatch();
            this.Stopwatch.Start();
        }

        public void Stop() => this.Stopwatch.Stop();

        public void Start() => this.Stopwatch.Start();

        public void StartNew() => Stopwatch.StartNew();

        public void Dispose()
        {
            if (!this.Stopwatch.IsRunning)
                return;
            this.Stopwatch.Stop();
        }

        public TimeSpan Elapsed => this.Stopwatch.Elapsed;

        public long ElapsedTicks => this.Stopwatch.ElapsedTicks;

        public long ElapsedMilliseconds => this.Stopwatch.ElapsedMilliseconds;
    }
}