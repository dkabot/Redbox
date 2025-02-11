using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Redbox.Core
{
    internal class NTUserImpersonator : IDisposable
    {
        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private WindowsImpersonationContext impersonationContext;

        public NTUserImpersonator(string userName, string domainName, string password)
        {
            this.ImpersonateValidUser(userName, domainName, password);
        }

        public void Dispose() => this.UndoImpersonation();

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int LogonUser(
          string lpszUserName,
          string lpszDomain,
          string lpszPassword,
          int dwLogonType,
          int dwLogonProvider,
          ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int DuplicateToken(
          IntPtr hToken,
          int impersonationLevel,
          ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool RevertToSelf();

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool CloseHandle(IntPtr handle);

        private void ImpersonateValidUser(string userName, string domain, string password)
        {
            IntPtr zero1 = IntPtr.Zero;
            IntPtr zero2 = IntPtr.Zero;
            try
            {
                if (!NTUserImpersonator.RevertToSelf())
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                if (NTUserImpersonator.LogonUser(userName, domain, password, 2, 0, ref zero1) == 0)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                this.impersonationContext = NTUserImpersonator.DuplicateToken(zero1, 2, ref zero2) != 0 ? new WindowsIdentity(zero2).Impersonate() : throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            finally
            {
                if (zero1 != IntPtr.Zero)
                    NTUserImpersonator.CloseHandle(zero1);
                if (zero2 != IntPtr.Zero)
                    NTUserImpersonator.CloseHandle(zero2);
            }
        }

        private void UndoImpersonation()
        {
            if (this.impersonationContext == null)
                return;
            this.impersonationContext.Undo();
        }
    }
}
