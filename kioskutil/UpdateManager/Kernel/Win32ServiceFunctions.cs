using Microsoft.Win32;
using Redbox.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;

namespace Redbox.UpdateManager.Kernel
{
  internal static class Win32ServiceFunctions
  {
    internal static bool ServiceExists(string name)
    {
      foreach (ServiceController service in ServiceController.GetServices())
      {
        if (service.ServiceName == name)
          return true;
      }
      return false;
    }

    internal static void StopService(string name)
    {
      try
      {
        using (ServiceController serviceController = new ServiceController(name))
        {
          serviceController.Stop();
          serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(5.0));
        }
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was rasied in Win32ServiceFunctions.StopService.", ex);
      }
    }

    internal static string GetServiceStatus(string name)
    {
      try
      {
        using (ServiceController serviceController = new ServiceController(name))
          return serviceController.Status.ToString();
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was rasied in Win32ServiceFunctions.GetServiceStatus.", ex);
      }
      return "Unknown";
    }

    internal static void SetStartupType(string name, string type)
    {
      try
      {
        if (new List<ServiceController>((IEnumerable<ServiceController>) ServiceController.GetServices()).FindIndex((Predicate<ServiceController>) (p => p.ServiceName == name)) <= -1)
          return;
        string keyName = string.Format("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\{0}", (object) name);
        switch (type)
        {
          case "automatic":
            Registry.SetValue(keyName, "Start", (object) 2);
            break;
          case "disabled":
            Registry.SetValue(keyName, "Start", (object) 4);
            break;
          case "manual":
            Registry.SetValue(keyName, "Start", (object) 3);
            break;
        }
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was rasied in Win32ServiceFunctions.SetStartupType.", ex);
      }
    }

    internal static void UninstallService(string name)
    {
      try
      {
        Process process = new Process()
        {
          StartInfo = new ProcessStartInfo("SC.exe", string.Format("delete {0}", (object) name))
          {
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
          }
        };
        process.Start();
        process.WaitForExit(120000);
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was rasied in Win32ServiceFunctions.UninstallService.", ex);
      }
    }

    internal static void InstallService(
      string name,
      string displayName,
      string file,
      string type,
      string startType,
      string dependencies)
    {
      try
      {
        string arguments = string.Format("create {0} displayName= {1} binPath= {2} type= {3} start= {4}", (object) name, (object) displayName, (object) file, (object) (type ?? "own"), (object) (startType ?? "auto"));
        if (!string.IsNullOrEmpty(dependencies))
          arguments += string.Format(" depend= {0}", (object) dependencies);
        Process process = new Process()
        {
          StartInfo = new ProcessStartInfo("SC.exe", arguments)
          {
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
          }
        };
        process.Start();
        process.WaitForExit(120000);
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was rasied in Win32ServiceFunctions.InstallService.", ex);
      }
    }

    internal static void PauseService(string name)
    {
      try
      {
        using (ServiceController serviceController = new ServiceController(name))
        {
          serviceController.Pause();
          serviceController.WaitForStatus(ServiceControllerStatus.Paused, TimeSpan.FromMinutes(5.0));
        }
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was rasied in Win32ServiceFunctions.PauseService.", ex);
      }
    }

    internal static void ResumeService(string name)
    {
      try
      {
        using (ServiceController serviceController = new ServiceController(name))
        {
          serviceController.Continue();
          serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(5.0));
        }
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was rasied in Win32ServiceFunctions.ResumeService.", ex);
      }
    }

    internal static void StartService(string name)
    {
      try
      {
        using (ServiceController serviceController = new ServiceController(name))
        {
          serviceController.Start();
          serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(5.0));
        }
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was rasied in Win32ServiceFunctions.StartService.", ex);
      }
    }
  }
}
