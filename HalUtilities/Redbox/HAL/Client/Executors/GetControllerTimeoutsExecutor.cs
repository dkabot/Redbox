using System;
using System.IO;

namespace Redbox.HAL.Client.Executors
{
  public sealed class GetControllerTimeoutsExecutor : JobExecutor
  {
    public void Log(StreamWriter log)
    {
      StreamWriter streamWriter = log;
      DateTime now = DateTime.Now;
      string shortTimeString = now.ToShortTimeString();
      now = DateTime.Now;
      string shortDateString = now.ToShortDateString();
      streamWriter.WriteLine("Counter sample on {0} {1}", (object) shortTimeString, (object) shortDateString);
      foreach (ProgramResult result in this.Results)
        log.WriteLine(" {0}: {1}", (object) result.Code, (object) result.Message);
    }

    protected override string JobName => "get-controller-timeouts";

    protected override string Label => "Tester Counters";

        public GetControllerTimeoutsExecutor(HardwareService service) : base(service)
        {
        }
    }
}
