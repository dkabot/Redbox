using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Redbox.Core;

namespace Redbox.KioskEngine.ComponentModel
{
    public static class KaseyaMethods
    {
        public static string GetKaseyaIniDPath()
        {
            var source = new List<string>();
            var from1 = "C:\\Program Files\\Kaseya";
            var from2 = "C:\\Program Files (x86)\\Kaseya";
            var stringList = new List<string>();
            stringList.AddRange(GetKaseyaInstallDirs(from1));
            if (stringList.Count == 0)
                stringList.AddRange(GetKaseyaInstallDirs(from2));
            try
            {
                foreach (var path1 in stringList)
                {
                    var path = Path.Combine(path1, "KaseyaD.ini");
                    if (File.Exists(path))
                        source.Add(path);
                }

                var kaseyaIniDpath = source.OrderByDescending(p => File.GetLastWriteTimeUtc(p)).FirstOrDefault();
                LogHelper.Instance.Log("KaseyaD.ini path: {0}", kaseyaIniDpath);
                return kaseyaIniDpath;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unexpected exception was raised in GetKaseyainiPath function.", ex.Message);
            }

            return null;
        }

        private static List<string> GetKaseyaInstallDirs(string from)
        {
            var kaseyaInstallDirs = new List<string>();
            if (Directory.Exists(from))
                try
                {
                    kaseyaInstallDirs.AddRange(Directory.GetDirectories(from, "RDBX*", SearchOption.TopDirectoryOnly));
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log("An unexpected exception was raised in GetKaseyaInstallDirs function.", ex);
                }

            return kaseyaInstallDirs;
        }
    }
}