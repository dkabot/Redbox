using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Redbox.Core
{
    public class LibraryModule : IDisposable
    {
        public LibraryModule(string fileName)
        {
            Handle = LoadLibrary(fileName);
            if (Handle == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public IntPtr Handle { get; }

        public void Dispose()
        {
            if (!(Handle != IntPtr.Zero))
                return;
            FreeLibrary(Handle);
        }

        public IntPtr GetProcAddress(string procName)
        {
            return GetProcAddress(Handle, procName);
        }

        [DllImport("Kernel32.dll", EntryPoint = "LoadLibraryW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
    }
}