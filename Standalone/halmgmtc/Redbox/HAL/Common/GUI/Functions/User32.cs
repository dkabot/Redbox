using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Redbox.HAL.Common.GUI.Functions
{
    public static class User32
    {
        public delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lparam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(
            HookType hookType,
            HookProc lpfn,
            IntPtr hMod,
            uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(
            IntPtr hhk,
            int nCode,
            IntPtr wparam,
            IntPtr lparam);

        public static IntPtr RegisterLowLevelHook(HookProc hook)
        {
            var num = IntPtr.Zero;
            using (var currentProcess = Process.GetCurrentProcess())
            {
                using (var mainModule = currentProcess.MainModule)
                {
                    var moduleHandle = Kernel32.GetModuleHandle(mainModule.ModuleName);
                    num = SetWindowsHookEx(HookType.WH_KEYBOARD_LL, hook, moduleHandle, 0U);
                }

                return num;
            }
        }
    }
}