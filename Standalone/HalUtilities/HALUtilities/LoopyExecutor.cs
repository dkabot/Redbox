using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace HALUtilities
{
    internal sealed class LoopyExecutor : JobExecutor
    {
        private readonly int Iterations;

        internal LoopyExecutor(HardwareService s, int iter)
            : base(s)
        {
            Iterations = iter;
        }

        protected override string JobName => "loopy";

        internal static void RunExecutor(int iterations, HardwareService s)
        {
            var consoleLogger = new ConsoleLogger(true);
            using (var loopyExecutor = new LoopyExecutor(s, iterations))
            {
                loopyExecutor.AddSink((job, eventTime, eventMessage) =>
                    LogHelper.Instance.Log("<EventReceived> Job = {0} Msg = {1}", job.ID, eventMessage));
                loopyExecutor.Run();
            }
        }

        protected override void SetupJob()
        {
            Job.Push(Iterations);
        }
    }
}