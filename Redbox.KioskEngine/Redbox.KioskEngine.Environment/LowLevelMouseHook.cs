using System;
using System.Runtime.InteropServices;

namespace Redbox.KioskEngine.Environment
{
  public class LowLevelMouseHook : WindowsHook
  {
    private IntPtr m_handle;

    public LowLevelMouseHook(IntPtr handle)
      : base(HookType.WH_MOUSE_LL)
    {
      this.m_handle = handle;
    }

    public override bool SetHook() => this.SetHook(this.m_handle, 0U);

    protected override bool HookFunction(int code, IntPtr wParam, IntPtr lParam)
    {
      return ((WindowsAPIObjects.MSLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof (WindowsAPIObjects.MSLLHOOKSTRUCT))).flags > 0U;
    }

    protected override void Dispose(bool disposing)
    {
      int num = disposing ? 1 : 0;
      this.m_handle = IntPtr.Zero;
    }
  }
}
