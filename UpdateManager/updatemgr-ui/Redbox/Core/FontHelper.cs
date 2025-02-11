using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Redbox.Core
{
    internal static class FontHelper
    {
        public static void AddFontResource(string path, string typeFaceName, bool isTrueType)
        {
            FontHelper.AddFontResourceA(path);
            Registry.SetValue("HKEY_LOCAL_MACHINE\\Software\\Microsoft\\Windows NT\\CurrentVersion\\Fonts", FontHelper.FormatTypeFaceName(typeFaceName, isTrueType), (object)Path.GetFileName(path));
            MessageHelper.SendNotifyMessage(MessageHelper.HWND_BROADCAST, 29U, IntPtr.Zero, IntPtr.Zero);
        }

        public static void RemoveFontResource(string path, string typeFaceName, bool isTrueType)
        {
            FontHelper.RemoveFontResourceA(path);
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion\\Fonts", RegistryKeyPermissionCheck.ReadWriteSubTree);
            if (registryKey == null)
                return;
            registryKey.DeleteValue(FontHelper.FormatTypeFaceName(typeFaceName, isTrueType));
            registryKey.Close();
            MessageHelper.SendNotifyMessage(MessageHelper.HWND_BROADCAST, 29U, IntPtr.Zero, IntPtr.Zero);
        }

        [DllImport("gdi32.dll", EntryPoint = "AddFontResource")]
        private static extern int AddFontResourceA(string lpszFilename);

        [DllImport("gdi32.dll", EntryPoint = "RemoveFontResource")]
        private static extern bool RemoveFontResourceA(string lpFileName);

        private static string FormatTypeFaceName(string typeFaceName, bool isTrueType)
        {
            return string.Format("{0}{1}", (object)typeFaceName, isTrueType ? (object)" (TrueType)" : (object)string.Empty);
        }
    }
}
