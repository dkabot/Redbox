using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace HALUtilities.KioskTest
{
    internal sealed class KioskTestLogger : ILogger, IDisposable
    {
        private readonly StreamWriter LogFile;

        internal KioskTestLogger()
        {
            var path2 = string.Format("kioskTest-{0}",
                ServiceLocator.Instance.GetService<IRuntimeService>().GenerateUniqueFile(".log"));
            var str = "c:\\Program Files\\Redbox\\KioskLogs\\KioskTest";
            var flag = true;
            if (!Directory.Exists(str))
                try
                {
                    Directory.CreateDirectory(str);
                }
                catch (Exception ex)
                {
                    flag = false;
                    Console.WriteLine("Failed to create kiosk logs path", ex.Message);
                }

            if (flag)
                LogFile = new StreamWriter(Path.Combine(str, path2))
                {
                    AutoFlush = true
                };
            else
                LogFile = StreamWriter.Null;
        }

        public void Dispose()
        {
            LogFile.Dispose();
        }

        public void Log(string message, Exception e)
        {
            printAndLog(message);
            printAndLog(e.Message);
        }

        public void Log(string message, LogEntryType type)
        {
            printAndLog(message);
        }

        public void Log(string message, Exception e, LogEntryType type)
        {
            printAndLog(message);
            printAndLog(e.Message);
        }

        public bool IsLevelEnabled(LogEntryType entryLevel)
        {
            return true;
        }

        private void printAndLog(string msg)
        {
            var str = string.Format("{0} {1}", DateTime.Now, msg);
            Console.WriteLine(str);
            LogFile.WriteLine(str);
        }
    }
}