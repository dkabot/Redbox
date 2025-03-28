using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Redbox.KioskEngine.Environment
{
  public class GenericTouchScreen : IDisposable
  {
    private readonly LowLevelMouseHook m_hook;
    private bool m_disposed;

    public GenericTouchScreen()
    {
      using (Process currentProcess = Process.GetCurrentProcess())
      {
        using (ProcessModule mainModule = currentProcess.MainModule)
          this.m_hook = new LowLevelMouseHook(GenericTouchScreen.GetModuleHandle(mainModule.ModuleName));
      }
    }

    ~GenericTouchScreen() => this.Dispose(false);

    public void Toggle()
    {
      try
      {
        this.ToggleScreen();
      }
      catch (InvalidOperationException ex)
      {
        LogHelper.Instance.Log("Touch screen control is no longer available.", (Exception) ex);
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("Error Toggling touch screen.", ex);
      }
    }

    public virtual string Driver => string.Empty;

    public TouchScreenState State
    {
      get
      {
        TouchScreenState state = this.m_disposed ? TouchScreenState.Unavailable : (this.m_hook.IsHooked ? TouchScreenState.Disabled : TouchScreenState.Enabled);
        LogHelper.Instance.Log("Touchscreen is currently {0}", (object) state);
        return state;
      }
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void ToggleScreen()
    {
      if (this.m_hook.IsHooked)
      {
        this.m_hook.UnHook();
        Windows10TouchScreen.Enable();
      }
      else
      {
        this.m_hook.SetHook();
        Windows10TouchScreen.Disable();
      }
    }

    protected virtual bool Enabled() => !this.m_hook.IsHooked;

    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
        this.m_hook.Dispose();
      this.m_disposed = true;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
  }
}
