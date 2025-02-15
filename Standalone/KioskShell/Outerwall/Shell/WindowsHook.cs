using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Outerwall.Shell
{
    internal abstract class WindowsHook : IDisposable
    {
        private readonly HookCallback _callback;
        private readonly object _disposeSync = new object();
        private readonly object _hookSync = new object();
        private readonly int _type;
        private bool _disposed;
        private IntPtr _handle = IntPtr.Zero;

        protected WindowsHook(int hookType)
        {
            _type = hookType;
            _callback = InternalHookFunction;
        }

        public bool IsHooked
        {
            get
            {
                Monitor.Enter(_hookSync);
                var num = _handle != IntPtr.Zero ? 1 : 0;
                Monitor.Exit(_hookSync);
                return num != 0;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~WindowsHook()
        {
            Dispose(false);
        }

        public virtual bool SetHook()
        {
            if (!Monitor.TryEnter(_hookSync))
                return false;
            try
            {
                using (var currentProcess = Process.GetCurrentProcess())
                {
                    using (var mainModule = currentProcess.MainModule)
                    {
                        var flag = false;
                        var moduleHandle = GetModuleHandle(mainModule.ModuleName);
                        if (_handle == IntPtr.Zero)
                        {
                            _handle = SetWindowsHookEx(_type, _callback, moduleHandle, 0U);
                            flag = _handle != IntPtr.Zero;
                        }

                        return flag;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to establish Windows hook.", ex);
            }
            finally
            {
                Monitor.Exit(_hookSync);
            }
        }

        public bool UnHook()
        {
            VerifyNotDisposed();
            Monitor.Enter(_hookSync);
            try
            {
                if (!(_handle != IntPtr.Zero))
                    return false;
                var num = UnhookWindowsHookEx(_handle) ? 1 : 0;
                _handle = IntPtr.Zero;
                return num != 0;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to unhook Windows hook.", ex);
            }
            finally
            {
                Monitor.Exit(_hookSync);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            Monitor.Enter(_hookSync);
            try
            {
                lock (_disposeSync)
                {
                    if (_disposed)
                        return;
                    var num = disposing ? 1 : 0;
                    UnhookWindowsHookEx(_handle);
                    _handle = IntPtr.Zero;
                    _disposed = true;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to dispose " + GetType().Name, ex);
            }
            finally
            {
                Monitor.Exit(_hookSync);
            }
        }

        protected abstract bool HookFunction(int code, IntPtr wParam, IntPtr lParam);

        private IntPtr InternalHookFunction(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var num = new IntPtr(1);
            try
            {
                if (nCode < 0)
                    num = CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
                if (HookFunction(nCode, wParam, lParam))
                    num = CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
                return num;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    string.Format(
                        "Unable to process this Windows message. nCode:{0}, wParam:{1}, lParam:{2}. The next hook will not be called.",
                        nCode, wParam, lParam), ex);
            }
        }

        private void VerifyNotDisposed()
        {
            lock (_disposeSync)
            {
                if (_disposed)
                    throw new InvalidOperationException(
                        "This Windows hook has been disposed and operations can no longer be performed.");
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(
            int idHook,
            HookCallback lpfn,
            IntPtr hMod,
            uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(
            IntPtr hhk,
            int nCode,
            IntPtr wParam,
            IntPtr lParam);

        private delegate IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam);
    }
}