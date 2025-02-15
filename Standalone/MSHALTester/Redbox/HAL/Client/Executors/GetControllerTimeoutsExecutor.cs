using System;
using System.IO;

namespace Redbox.HAL.Client.Executors;

public sealed class GetControllerTimeoutsExecutor(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "get-controller-timeouts";

    protected override string Label => "Tester Counters";

    public void Log(StreamWriter log)
    {
        var streamWriter = log;
        var now = DateTime.Now;
        var shortTimeString = now.ToShortTimeString();
        now = DateTime.Now;
        var shortDateString = now.ToShortDateString();
        streamWriter.WriteLine("Counter sample on {0} {1}", shortTimeString, shortDateString);
        foreach (var result in Results)
            log.WriteLine(" {0}: {1}", result.Code, result.Message);
    }
}