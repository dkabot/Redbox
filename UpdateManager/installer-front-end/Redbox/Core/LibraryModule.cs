using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Redbox.Core
{
    internal class LibraryModule : IDisposable
    {
        private readonly IntPtr m_hModule;

        public LibraryModule(string fileName)
        {
            this.m_hModule = LibraryModule.LoadLibrary(fileName);
            if (this.m_hModule == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public IntPtr GetProcAddress(string procName)
        {
            return LibraryModule.GetProcAddress(this.m_hModule, procName);
        }

        public void Dispose()
        {
            if (!(this.m_hModule != IntPtr.Zero))
                return;
            LibraryModule.FreeLibrary(this.m_hModule);
        }

        public IntPtr Handle => this.m_hModule;

        [DllImport("Kernel32.dll", EntryPoint = "LoadLibraryW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("Kernel32.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
    }
}
