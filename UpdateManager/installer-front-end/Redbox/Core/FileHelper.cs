using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Redbox.Core
{
    internal static class FileHelper
    {
        public static void CreateBackup(string path, string extension)
        {
            if (!File.Exists(path))
                return;
            if (extension != null && !extension.StartsWith("."))
                extension = "." + extension;
            string destFileName = string.Format("{0}.{1}{2}", (object)path, (object)DateTime.Now.ToString("yyyyMMddHHmmss"), (object)(extension ?? ".bak"));
            File.Copy(path, destFileName, true);
        }

        public static string GetShortName(string path)
        {
            StringBuilder stringBuilder = new StringBuilder(256);
            string lpszLongPath = path;
            StringBuilder lpszShortPath = stringBuilder;
            int capacity = lpszShortPath.Capacity;
            int shortPathName = (int)FileHelper.GetShortPathName(lpszLongPath, lpszShortPath, capacity);
            return stringBuilder.ToString();
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetShortPathName(
          [MarshalAs(UnmanagedType.LPTStr)] string lpszLongPath,
          [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszShortPath,
          int cchBuffer);
    }
}
