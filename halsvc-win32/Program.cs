using System;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace Redbox.HAL.Service.Win32
{
    public static class Program
    {
        private static readonly string logFilePath = "UnhandledException.log";

        private static void CurrentDomain_UnhandledException(
            object sender,
            UnhandledExceptionEventArgs e)
        {
            try
            {
                var exceptionObject = (Exception)e.ExceptionObject;
                File.AppendAllText(Path.Combine("c:\\program files\\redbox\\kiosklogs", logFilePath),
                    string.Format("{0} - Unhandled Application Exception. Exiting Application {1}", DateTime.Now,
                        exceptionObject));
            }
            finally
            {
                Environment.Exit(-1);
            }
        }

        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Thread.CurrentThread.Name = "HAL Service";
            ServiceBase.Run(new ServiceBase[1]
            {
                new HALService()
            });
        }
    }
}