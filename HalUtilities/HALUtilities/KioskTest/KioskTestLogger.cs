using Redbox.HAL.Component.Model;
using System;
using System.IO;

namespace HALUtilities.KioskTest
{
  internal sealed class KioskTestLogger : ILogger, IDisposable
  {
    private readonly StreamWriter LogFile;

    public void Log(string message, Exception e)
    {
      this.printAndLog(message);
      this.printAndLog(e.Message);
    }

    public void Log(string message, LogEntryType type) => this.printAndLog(message);

    public void Log(string message, Exception e, LogEntryType type)
    {
      this.printAndLog(message);
      this.printAndLog(e.Message);
    }

    public bool IsLevelEnabled(LogEntryType entryLevel) => true;

    public void Dispose() => this.LogFile.Dispose();

    internal KioskTestLogger()
    {
      string path2 = string.Format("kioskTest-{0}", (object) ServiceLocator.Instance.GetService<IRuntimeService>().GenerateUniqueFile(".log"));
      string str = "c:\\Program Files\\Redbox\\KioskLogs\\KioskTest";
      bool flag = true;
      if (!Directory.Exists(str))
      {
        try
        {
          Directory.CreateDirectory(str);
        }
        catch (Exception ex)
        {
          flag = false;
          Console.WriteLine("Failed to create kiosk logs path", (object) ex.Message);
        }
      }
      if (flag)
        this.LogFile = new StreamWriter(Path.Combine(str, path2))
        {
          AutoFlush = true
        };
      else
        this.LogFile = StreamWriter.Null;
    }

    private void printAndLog(string msg)
    {
      string str = string.Format("{0} {1}", (object) DateTime.Now, (object) msg);
      Console.WriteLine(str);
      this.LogFile.WriteLine(str);
    }
  }
}
