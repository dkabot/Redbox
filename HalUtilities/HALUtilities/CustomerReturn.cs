using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace HALUtilities
{
    internal sealed class CustomerReturn : JobExecutor
    {
        internal CustomerReturn(HardwareService s)
            : base(s)
        {
        }

        protected override string JobName => "return";

        internal static void RunExecutor(object o)
        {
            RunExecutor(o as HardwareService);
        }

        internal static void RunExecutor(HardwareService service)
        {
            var consoleLogger = new ConsoleLogger(true);
            using (var customerReturn = new CustomerReturn(service))
            {
                customerReturn.AddSink((job, eventTime, eventMessage) =>
                    LogHelper.Instance.Log("<EventReceived> Job = {0} Msg = {1}", job.ID, eventMessage));
                customerReturn.Run();
            }
        }
    }
}