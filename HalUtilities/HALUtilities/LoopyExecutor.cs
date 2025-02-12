using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace HALUtilities
{
  internal sealed class LoopyExecutor : JobExecutor
  {
    private readonly int Iterations;

    internal static void RunExecutor(int iterations, HardwareService s)
    {
      ConsoleLogger consoleLogger = new ConsoleLogger(true);
      using (LoopyExecutor loopyExecutor = new LoopyExecutor(s, iterations))
      {
        loopyExecutor.AddSink((HardwareEvent) ((job, eventTime, eventMessage) => LogHelper.Instance.Log("<EventReceived> Job = {0} Msg = {1}", (object) job.ID, (object) eventMessage)));
        loopyExecutor.Run();
      }
    }

    protected override void SetupJob() => this.Job.Push((object) this.Iterations);

    protected override string JobName => "loopy";

    internal LoopyExecutor(HardwareService s, int iter)
      : base(s)
    {
      this.Iterations = iter;
    }
  }
}
