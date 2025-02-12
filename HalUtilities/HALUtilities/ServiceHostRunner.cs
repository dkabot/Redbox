using System;
using System.Threading;
using Redbox.HAL.IPC.Framework.Server;
using Redbox.IPC.Framework;

namespace HALUtilities
{
    internal static class ServiceHostRunner
    {
        internal static void Run(string protocol, bool debug)
        {
            var consoleLogger = new ConsoleLogger(debug);
            var serviceHostFactory = new IpcServiceHostFactory();
            var info = serviceHostFactory.Create(typeof(Program).Assembly);
            var ipcServiceHost = serviceHostFactory.Create(IPCProtocol.Parse(protocol), info);
            var thread = new Thread(ipcServiceHost.Start);
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