using System;
using System.Runtime.InteropServices;

namespace Redbox.KioskEngine.Environment
{
  public abstract class WindowsHook : IDisposable
  {
    private readonly HookType m_type;
    private WindowsHook.HookCallback m_callback;
    private IntPtr m_handle = IntPtr.Zero;
    private bool m_disposed;

    protected WindowsHook(HookType hookType)
    {
      this.m_type = hookType;
      this.m_callback = new WindowsHook.HookCallback(this.InternalHookFunction);
    }

    ~WindowsHook() => this.Dispose(false);

    public bool IsHooked => this.m_handle != IntPtr.Zero;

    public abstract bool SetHook();

    public bool UnHook()
    {
      this.TestDisposed();
      if (!this.IsHooked)
        return false;
      int num = WindowsHook.UnhookWindowsHookEx(this.m_handle) ? 1 : 0;
      this.m_handle = IntPtr.Zero;
      return num != 0;
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected bool SetHook(IntPtr moduleHandle, uint threadId)
    {
      this.TestDisposed();
      if (this.IsHooked)
        return false;
      this.m_handle = WindowsHook.SetWindowsHookEx((int) this.m_type, this.m_callback, moduleHandle, threadId);
      return this.m_handle != IntPtr.Zero;
    }

    protected virtual void Dispose(bool disposing)
    {
      int num = disposing ? 1 : 0;
      WindowsHook.UnhookWindowsHookEx(this.m_handle);
      this.m_handle = IntPtr.Zero;
      this.m_disposed = true;
    }

    protected abstract bool HookFunction(int code, IntPtr wParam, IntPtr lParam);

    private IntPtr InternalHookFunction(int nCode, IntPtr wParam, IntPtr lParam)
    {
      return nCode < 0 || this.HookFunction(nCode, wParam, lParam) ? WindowsHook.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam) : new IntPtr(1);
    }

    private void TestDisposed()
    {
      if (this.m_disposed)
        throw new InvalidOperationException("This windows hook has been disposed and operations can no longer be performed on it.");
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(
      int idHook,
      WindowsHook.HookCallback lpfn,
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
