using Redbox.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Redbox.KioskEngine.ComponentModel
{
  public static class KaseyaMethods
  {
    public static string GetKaseyaIniDPath()
    {
      List<string> source = new List<string>();
      string from1 = "C:\\Program Files\\Kaseya";
      string from2 = "C:\\Program Files (x86)\\Kaseya";
      List<string> stringList = new List<string>();
      stringList.AddRange((IEnumerable<string>) KaseyaMethods.GetKaseyaInstallDirs(from1));
      if (stringList.Count == 0)
        stringList.AddRange((IEnumerable<string>) KaseyaMethods.GetKaseyaInstallDirs(from2));
      try
      {
        foreach (string path1 in stringList)
        {
          string path = Path.Combine(path1, "KaseyaD.ini");
          if (File.Exists(path))
            source.Add(path);
        }
        string kaseyaIniDpath = source.OrderByDescending<string, DateTime>((Func<string, DateTime>) (p => File.GetLastWriteTimeUtc(p))).FirstOrDefault<string>();
        LogHelper.Instance.Log("KaseyaD.ini path: {0}", (object) kaseyaIniDpath);
        return kaseyaIniDpath;
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unexpected exception was raised in GetKaseyainiPath function.", (object) ex.Message);
      }
      return (string) null;
    }

    private static List<string> GetKaseyaInstallDirs(string from)
    {
      List<string> kaseyaInstallDirs = new List<string>();
      if (Directory.Exists(from))
      {
        try
        {
          kaseyaInstallDirs.AddRange((IEnumerable<string>) Directory.GetDirectories(from, "RDBX*", SearchOption.TopDirectoryOnly));
        }
        catch (Exception ex)
        {
          LogHelper.Instance.Log("An unexpected exception was raised in GetKaseyaInstallDirs function.", ex);
        }
      }
      return kaseyaInstallDirs;
    }
  }
}
