using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace HALUtilities
{
  internal sealed class CustomerReturn : JobExecutor
  {
    internal static void RunExecutor(object o) => CustomerReturn.RunExecutor(o as HardwareService);

    internal static void RunExecutor(HardwareService service)
    {
      ConsoleLogger consoleLogger = new ConsoleLogger(true);
      using (CustomerReturn customerReturn = new CustomerReturn(service))
      {
        customerReturn.AddSink((HardwareEvent) ((job, eventTime, eventMessage) => LogHelper.Instance.Log("<EventReceived> Job = {0} Msg = {1}", (object) job.ID, (object) eventMessage)));
        customerReturn.Run();
      }
    }

    protected override string JobName => "return";

    internal CustomerReturn(HardwareService s)
      : base(s)
    {
    }
  }
}
