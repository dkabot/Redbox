using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Redbox.Core
{
    public static class ResourceTool
    {
        public static byte[] GetResource(LibraryModule module, string group, string name)
        {
            if (group == null || name == null)
                return null;
            var destination = (byte[])null;
            var resource = FindResource(module.Handle, name.ToUpper(), group.ToUpper());
            if (resource == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            var length = SizeofResource(module.Handle, resource);
            if (length > 0)
            {
                var hResData = LoadResource(module.Handle, resource);
                var source = !(hResData == IntPtr.Zero)
                    ? LockResource(hResData)
                    : throw new Win32Exception(Marshal.GetLastWin32Error());
                if (source != IntPtr.Zero)
                {
                    destination = new byte[length];
                    Marshal.Copy(source, destination, 0, length);
                }
            }

            return destination;
        }

        public static int ForEachResourceDo(LibraryModule module, string group, Action<string> action)
        {
            return EnumResourceNames(module.Handle, group.ToUpper(), (hModule, pType, pName, param) =>
            {
                action(pName);
                return true;
            }, IntPtr.Zero);
        }

        [DllImport("Kernel32.dll", EntryPoint = "FindResourceW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr FindResource(IntPtr hModule, string lpName, string lpType);

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResource);

        [DllImport("Kernel32.dll")]
        private static extern IntPtr LockResource(IntPtr hResData);

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern int SizeofResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int EnumResourceNames(
            IntPtr hModule,
            string pType,
            EnumResNameProc callback,
            IntPtr param);

        internal delegate bool EnumResNameProc(
            IntPtr hModule,
            string pType,
            string pName,
            IntPtr param);
    }
}