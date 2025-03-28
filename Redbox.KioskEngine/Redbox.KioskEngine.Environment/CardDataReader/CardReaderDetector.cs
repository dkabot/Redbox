using Microsoft.Win32;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.USB;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Redbox.KioskEngine.Environment.CardDataReader
{
  public class CardReaderDetector
  {
    public static bool IsDeviceServiceCardReaderAttached()
    {
      IDeviceServiceClientHelper service = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
      bool flag = service != null && service.IsCardReaderConnected;
      LogHelper.Instance.Log(string.Format("CardReaderDetector.IsDeviceServiceCardReaderAttached: {0}", (object) flag));
      return flag;
    }

    public static bool IsUSBCardReaderAttached()
    {
      using (HidDeviceManager hidDeviceManager = new HidDeviceManager())
      {
        List<string> usbCardReaderKeys = CardReaderDetector.GetUSBCardReaderKeys();
        foreach (HidDevice enumerateDevice in hidDeviceManager.EnumerateDevices())
        {
          string lower = enumerateDevice.DevicePath.ToLower();
          string s = string.Format("{0},{1}", (object) CardReaderDetector.GetIdFromPath(lower, "vid_"), (object) CardReaderDetector.GetIdFromPath(lower, "pid_"));
          SHA256Managed shA256Managed = new SHA256Managed();
          string base64String = Convert.ToBase64String(shA256Managed.ComputeHash(shA256Managed.ComputeHash(Encoding.UTF8.GetBytes(s))));
          if (usbCardReaderKeys.Contains(base64String))
            return true;
        }
      }
      return false;
    }

    private static string GetIdFromPath(string devicePath, string name)
    {
      return devicePath.Substring(devicePath.IndexOf(name) + 4, 4);
    }

    private static List<string> GetUSBCardReaderKeys()
    {
      List<string> usbCardReaderKeys = new List<string>();
      using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Redbox\\CRList"))
      {
        if (registryKey != null)
        {
          object obj = registryKey.GetValue("List");
          if (obj != null)
          {
            string str = obj.ToString();
            char[] chArray = new char[1]{ ',' };
            foreach (string name in str.Split(chArray))
              usbCardReaderKeys.Add((string) registryKey.GetValue(name));
          }
          registryKey.Close();
        }
      }
      if (usbCardReaderKeys.Count == 0)
        LogHelper.Instance.Log("...No card readers have been defined for this kiosk.", LogEntryType.Info);
      return usbCardReaderKeys;
    }
  }
}
