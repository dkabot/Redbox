using Redbox.HAL.Client;
using Redbox.HAL.Client.Services;
using Redbox.HAL.Component.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace HALUtilities
{
  internal sealed class KioskFunctionCheckHelper : IDisposable
  {
    private const string OutputFile = "KFCSessions.txt";
    private readonly HardwareService Service;

    public void Dispose() => this.Sessions.Clear();

    internal IList<IKioskFunctionCheckData> Sessions { get; private set; }

    internal void DumpSessions(OutputModes mode)
    {
      IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
      TextWriter writer = Console.Out;
      if (mode == OutputModes.File)
      {
        service.SafeDelete("KFCSessions.txt");
        writer = (TextWriter) new StreamWriter("KFCSessions.txt");
      }
      this.DumpSessions(writer);
      if (mode != OutputModes.File)
        return;
      writer.Dispose();
    }

    internal void DumpSessions(TextWriter writer)
    {
      if (this.Sessions.Count == 0)
      {
        writer.WriteLine("There is no session data available.");
      }
      else
      {
        foreach (IKioskFunctionCheckData session in (IEnumerable<IKioskFunctionCheckData>) this.Sessions)
          this.DumpSession(session, writer);
      }
    }

    internal KioskFunctionCheckHelper(HardwareService s)
    {
      ConsoleLogger consoleLogger = new ConsoleLogger(true);
      this.Service = s;
      this.Sessions = (IList<IKioskFunctionCheckData>) new List<IKioskFunctionCheckData>();
      IList<IKioskFunctionCheckData> sessions;
      HardwareCommandResult functionCheckData1 = this.Service.GetKioskFunctionCheckData(out sessions);
      if (functionCheckData1.Success)
      {
        foreach (IKioskFunctionCheckData functionCheckData2 in (IEnumerable<IKioskFunctionCheckData>) sessions)
          this.Sessions.Add(functionCheckData2);
        sessions.Clear();
      }
      else
        functionCheckData1.Dump();
    }

    private void DumpSession(IKioskFunctionCheckData session, TextWriter stream)
    {
      stream.WriteLine("## Session User {0} on {1} ##", (object) session.UserIdentifier, (object) session.Timestamp);
      stream.WriteLine("   Vertical result = {0}", (object) session.VerticalSlotTestResult);
      stream.WriteLine("   Init result = {0}", (object) session.InitTestResult);
      stream.WriteLine("   Venddoor result = {0}", (object) session.VendDoorTestResult);
      stream.WriteLine("   Track result = {0}", (object) session.TrackTestResult);
      stream.WriteLine("   Snap and decode result = {0}", (object) session.SnapDecodeTestResult);
      stream.WriteLine("   Touchscreen driver result = {0}", (object) session.TouchscreenDriverTestResult);
      stream.WriteLine("   Camera driver result = {0}", (object) session.CameraDriverTestResult);
    }
  }
}
