using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace HALUtilities
{
  internal sealed class ServiceConfigurationHelper : IDisposable
  {
    private readonly HardwareService Service;

    public void Dispose()
    {
    }

    internal void BackupConfiguration(string kioskID)
    {
      IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
      using (StreamWriter streamWriter = new StreamWriter((Stream) File.Open(service.RuntimePath("backup-log.txt"), FileMode.Append, FileAccess.Write, FileShare.Read)))
      {
        string str = service.RuntimePath(kioskID);
        if (!Directory.Exists(str))
        {
          try
          {
            Directory.CreateDirectory(str);
          }
          catch
          {
            streamWriter.WriteLine("Unable to create backup directory.");
            return;
          }
        }
        try
        {
          File.Copy("c:\\gamp\\SystemData.dat", Path.Combine(str, "SystemData.dat"), true);
          File.Copy("c:\\gamp\\SlotData.dat", Path.Combine(str, "SlotData.dat"), true);
          File.Copy("c:\\Program Files\\Redbox\\HALService\\bin\\HAL.xml", Path.Combine(str, "HAL.xml"), true);
        }
        catch
        {
          streamWriter.WriteLine("Error backing up file.");
        }
      }
    }

    internal bool UpdateOption(string updateConfigurationString)
    {
      IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
      char ch = ':';
      if (-1 != updateConfigurationString.IndexOf('|'))
        ch = '|';
      string[] strArray = updateConfigurationString.Split(ch);
      if (strArray.Length < 3)
      {
        Console.WriteLine("Unable to parse option {0}", (object) updateConfigurationString);
        return false;
      }
      string path1 = service.RuntimePath("ConfigurationUpdateError.log");
      string str1 = strArray[0];
      string str2 = strArray[1];
      string str3 = strArray[2];
      bool flag1 = strArray.Length == 4;
      if (File.Exists(path1))
      {
        try
        {
          File.Delete(path1);
        }
        catch
        {
        }
      }
      if (this.Service == null)
      {
        using (StreamWriter streamWriter = new StreamWriter(path1))
          streamWriter.WriteLine("Unable to talk to HAL.");
        return false;
      }
      HardwareJob job;
      HardwareCommandResult hardwareCommandResult1 = this.Service.ExecuteImmediate(string.Format("SETCFG \"{0}\" \"{1}\" TYPE={2}", (object) str2, (object) str3, (object) str1), out job);
      if (!hardwareCommandResult1.Success)
      {
        using (StreamWriter log = new StreamWriter(path1))
        {
          log.WriteLine("The configuration update failed.");
          hardwareCommandResult1.Errors.ForEach((Action<Error>) (err => log.WriteLine(err.ToString())));
        }
        return false;
      }
      if (!flag1)
        return true;
      HardwareCommandResult hardwareCommandResult2 = this.Service.ExecuteImmediate(string.Format("GETCFG \"{0}\" TYPE={1}", (object) str2, (object) str1), out job);
      string path2 = service.RuntimePath("ConfigurationUpdateValidateError.log");
      service.SafeDelete(path2);
      bool flag2 = false;
      using (StreamWriter validateFile = new StreamWriter(path2))
      {
        if (!hardwareCommandResult2.Success)
        {
          validateFile.WriteLine("The configuration update failed.");
          hardwareCommandResult2.Errors.ForEach((Action<Error>) (err => validateFile.WriteLine(err.ToString())));
          return false;
        }
        Stack<string> stack;
        if (!job.GetStack(out stack).Success)
        {
          validateFile.WriteLine("Couldn't validate: unable to get stack.");
        }
        else
        {
          string str4 = stack.Pop();
          if (!str4.Equals(str3, StringComparison.CurrentCultureIgnoreCase))
            validateFile.WriteLine("The new value {0} doesn't match {1}", (object) str4, (object) str3);
          else
            flag2 = true;
        }
      }
      return flag2;
    }

    internal ServiceConfigurationHelper(HardwareService service) => this.Service = service;
  }
}
