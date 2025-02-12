using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Redbox.HAL.Client;
using Redbox.HAL.Core;

namespace Redbox.HAL.Common.GUI.Functions
{
    public static class CommonCameraFunctions
    {
        public static bool IRCameraConfigured(HardwareService service)
        {
            var job = CommonFunctions.ExecuteInstruction(service, "CAMERA IRCAMERA");
            return job != null && bool.Parse(job.GetTopOfStack());
        }

        public static void LaunchAndWaitForTuner(HardwareService service)
        {
            if (service == null)
                return;
            var flag = IRCameraConfigured(service);
            var str = "c:\\Program Files\\Redbox\\halservice\\bin\\CameraTuner.exe";
            if (!File.Exists(str))
                return;
            var stringBuilder1 = new StringBuilder();
            stringBuilder1.Append("CAMERA STOP FORCE=TRUE" + Environment.NewLine);
            stringBuilder1.Append("RINGLIGHT ON" + Environment.NewLine);
            HardwareJob job;
            service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder1.ToString()), out job);
            PerfFunctions.SpinWait(5000);
            Process.Start(str, flag ? "-secure" : string.Empty).WaitForExit();
            var stringBuilder2 = new StringBuilder();
            stringBuilder2.Append("RINGLIGHT OFF" + Environment.NewLine);
            stringBuilder2.Append("CAMERA START" + Environment.NewLine);
            service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder2.ToString()), out job);
        }
    }
}