using System.Security.Principal;

namespace Redbox.Core
{
    public static class SecurityHelper
    {
        public static bool IsCurrentUserAnAdministrator()
        {
            var current = WindowsIdentity.GetCurrent();
            return current != null && new WindowsPrincipal(current).IsInRole("BUILTIN\\Administrators");
        }

        public static bool IsCurrentUserPowerUser()
        {
            var current = WindowsIdentity.GetCurrent();
            return current != null && new WindowsPrincipal(current).IsInRole("BUILTIN\\Power Users");
        }
    }
}