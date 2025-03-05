using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Redbox.KioskEngine.ComponentModel
{
  public static class SecureStringExtensions
  {
    public static string SecureStringToString(this SecureString value)
    {
      if (value == null)
        return (string) null;
      IntPtr num = IntPtr.Zero;
      try
      {
        num = Marshal.SecureStringToGlobalAllocUnicode(value);
        return Marshal.PtrToStringUni(num);
      }
      finally
      {
        Marshal.ZeroFreeGlobalAllocUnicode(num);
      }
    }
  }
}
