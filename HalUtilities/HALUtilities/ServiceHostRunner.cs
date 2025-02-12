using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework.Server;
using Redbox.IPC.Framework;
using System;
using System.Threading;

namespace HALUtilities
{
  internal static class ServiceHostRunner
  {
    internal static void Run(string protocol, bool debug)
    {
      ConsoleLogger consoleLogger = new ConsoleLogger(debug);
      IpcServiceHostFactory serviceHostFactory = new IpcServiceHostFactory();
      IHostInfo info = serviceHostFactory.Create(typeof (Program).Assembly);
      IIpcServiceHost ipcServiceHost = serviceHostFactory.Create((IIpcProtocol) IPCProtocol.Parse(protocol), info);
      Thread thread = new Thread(new ThreadStart(ipcServiceHost.Start));
      thread.Start();
      Console.WriteLine("Press any key to halt.");
      Console.ReadLine();
      ipcServiceHost.Stop();
      thread.Join(5000);
      Console.WriteLine("Program finished.");
      Environment.Exit(0);
    }
  }
}
