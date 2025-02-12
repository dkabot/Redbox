using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Services;
using Redbox.HAL.Component.Model;

namespace HALUtilities
{
    internal sealed class KioskFunctionCheckHelper : IDisposable
    {
        private const string OutputFile = "KFCSessions.txt";
        private readonly HardwareService Service;

        internal KioskFunctionCheckHelper(HardwareService s)
        {
            var consoleLogger = new ConsoleLogger(true);
            Service = s;
            Sessions = new List<IKioskFunctionCheckData>();
            IList<IKioskFunctionCheckData> sessions;
            var functionCheckData1 = Service.GetKioskFunctionCheckData(out sessions);
            if (functionCheckData1.Success)
            {
                foreach (var functionCheckData2 in sessions)
                    Sessions.Add(functionCheckData2);
                sessions.Clear();
            }
            else
            {
                functionCheckData1.Dump();
            }
        }

        internal IList<IKioskFunctionCheckData> Sessions { get; }

        public void Dispose()
        {
            Sessions.Clear();
        }

        internal void DumpSessions(OutputModes mode)
        {
            var service = ServiceLocator.Instance.GetService<IRuntimeService>();
            var writer = Console.Out;
            if (mode == OutputModes.File)
            {
                service.SafeDelete("KFCSessions.txt");
                writer = new StreamWriter("KFCSessions.txt");
            }

            DumpSessions(writer);
            if (mode != OutputModes.File)
                return;
            writer.Dispose();
        }

        internal void DumpSessions(TextWriter writer)
        {
            if (Sessions.Count == 0)
                writer.WriteLine("There is no session data available.");
            else
                foreach (var session in Sessions)
                    DumpSession(session, writer);
        }

        private void DumpSession(IKioskFunctionCheckData session, TextWriter stream)
        {
            stream.WriteLine("## Session User {0} on {1} ##", session.UserIdentifier, session.Timestamp);
            stream.WriteLine("   Vertical result = {0}", session.VerticalSlotTestResult);
            stream.WriteLine("   Init result = {0}", session.InitTestResult);
            stream.WriteLine("   Venddoor result = {0}", session.VendDoorTestResult);
            stream.WriteLine("   Track result = {0}", session.TrackTestResult);
            stream.WriteLine("   Snap and decode result = {0}", session.SnapDecodeTestResult);
            stream.WriteLine("   Touchscreen driver result = {0}", session.TouchscreenDriverTestResult);
            stream.WriteLine("   Camera driver result = {0}", session.CameraDriverTestResult);
        }
    }
}