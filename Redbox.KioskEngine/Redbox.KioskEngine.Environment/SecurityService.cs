using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.DirectoryServices.AccountManagement;

namespace Redbox.KioskEngine.Environment
{
  public class SecurityService : ISecurityService
  {
    public static SecurityService Instance => Singleton<SecurityService>.Instance;

    public bool CreateUser(string userName, string password, int numberOfDaysToExpire)
    {
      if (!string.IsNullOrEmpty(userName))
      {
        if (!string.IsNullOrEmpty(password))
        {
          try
          {
            using (PrincipalContext context = new PrincipalContext(ContextType.Machine))
            {
              UserPrincipal userPrincipal = new UserPrincipal(context);
              userPrincipal.Name = userName;
              userPrincipal.DisplayName = userName;
              userPrincipal.SetPassword(password);
              userPrincipal.UserCannotChangePassword = true;
              userPrincipal.AccountExpirationDate = new DateTime?(DateTime.Now.AddDays((double) numberOfDaysToExpire));
              userPrincipal.Save();
              return true;
            }
          }
          catch
          {
            return false;
          }
        }
      }
      return false;
    }

    public bool Authenticate(string userName, string password)
    {
      if (!string.IsNullOrEmpty(userName))
      {
        if (!string.IsNullOrEmpty(password))
        {
          try
          {
            using (PrincipalContext principalContext = new PrincipalContext(ContextType.Machine))
              return principalContext.ValidateCredentials(userName, password);
          }
          catch
          {
            return false;
          }
        }
      }
      return false;
    }

    public bool UserExists(string userName, out bool isAccountLocked)
    {
      isAccountLocked = false;
      if (string.IsNullOrEmpty(userName))
        return false;
      try
      {
        using (PrincipalContext context = new PrincipalContext(ContextType.Machine))
        {
          UserPrincipal byIdentity = UserPrincipal.FindByIdentity(context, IdentityType.Name, userName);
          if (byIdentity == null)
            return false;
          isAccountLocked = byIdentity.IsAccountLockedOut();
        }
      }
      catch
      {
        return false;
      }
      return true;
    }

    public bool RemoveUser(string userName)
    {
      if (string.IsNullOrEmpty(userName))
        return false;
      try
      {
        using (PrincipalContext context = new PrincipalContext(ContextType.Machine))
        {
          UserPrincipal byIdentity = UserPrincipal.FindByIdentity(context, IdentityType.Name, userName);
          if (byIdentity == null)
            return false;
          byIdentity.Delete();
          return true;
        }
      }
      catch
      {
        return false;
      }
    }

    public bool SlideExpirationDate(string userName, int numberOfDays)
    {
      if (string.IsNullOrEmpty(userName))
        return false;
      try
      {
        using (PrincipalContext context = new PrincipalContext(ContextType.Machine))
        {
          UserPrincipal byIdentity = UserPrincipal.FindByIdentity(context, IdentityType.Name, userName);
          if (byIdentity == null)
            return false;
          byIdentity.AccountExpirationDate = new DateTime?(DateTime.Now.AddDays((double) numberOfDays));
          byIdentity.Save();
          return true;
        }
      }
      catch
      {
        return false;
      }
    }

    private SecurityService()
    {
    }
  }
}
