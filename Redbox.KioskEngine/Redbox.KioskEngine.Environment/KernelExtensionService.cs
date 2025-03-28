using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

namespace Redbox.KioskEngine.Environment
{
  public class KernelExtensionService : IKernelExtensionService
  {
    private readonly object m_syncObject = new object();
    private IDictionary<Guid, IKernelExtension> m_innerExtensions;

    public static IKernelExtensionService Instance
    {
      get => (IKernelExtensionService) Singleton<KernelExtensionService>.Instance;
    }

    public IKernelExtension GetExtension(Guid id)
    {
      lock (this.m_syncObject)
        return this.InnerExtensions.ContainsKey(id) ? this.InnerExtensions[id] : (IKernelExtension) null;
    }

    public ErrorList Initialize(List<Type> kernelExtensionHostTypes)
    {
      LogHelper.Instance.Log("Initialize kernel extension service:");
      ErrorList errorList = new ErrorList();
      ServiceLocator.Instance.AddService(typeof (IKernelExtensionService), (object) KernelExtensionService.Instance);
      foreach (Type extensionHostType in kernelExtensionHostTypes)
      {
        errorList.AddRange((IEnumerable<Redbox.KioskEngine.ComponentModel.Error>) this.LoadExtension(extensionHostType));
        LogHelper.Instance.Log("...Loaded kernel extension '{0}'", (object) extensionHostType);
      }
      return errorList;
    }

    public ErrorList LoadExtension(Type extensionHostType)
    {
      ErrorList errorList = new ErrorList();
      try
      {
        if (extensionHostType != (Type) null)
        {
          string str = "0.0.0.0";
          Assembly assembly = extensionHostType?.Assembly;
          if (assembly != (Assembly) null)
          {
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            if (versionInfo != null)
              str = versionInfo.FileVersion;
          }
          if (Activator.CreateInstance(extensionHostType) is IKernelExtensionHost instance)
          {
            Guid? nullable = new Guid?(Guid.NewGuid());
            KernelExtension kernelExtension = new KernelExtension()
            {
              Host = instance,
              ID = nullable.Value,
              IsUnloadSupported = false,
              Name = extensionHostType.ToString(),
              Version = str
            };
            instance.Extension = (IKernelExtension) kernelExtension;
            errorList.AddRange((IEnumerable<Redbox.KioskEngine.ComponentModel.Error>) instance.PreLoad());
            this.InnerExtensions[nullable.Value] = (IKernelExtension) kernelExtension;
          }
        }
      }
      catch (Exception ex)
      {
        errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("P999", string.Format("An unhandled exception was raised while loading kernel extension: {0}.   See the details of this error for more information on the exception.", (object) extensionHostType), ex));
      }
      return errorList;
    }

    public void NotifyOfHostCrash(Exception e)
    {
      lock (this.m_syncObject)
      {
        foreach (IKernelExtension kernelExtension in (IEnumerable<IKernelExtension>) this.InnerExtensions.Values)
        {
          if (kernelExtension.Host != null)
            kernelExtension.Host.HandleHostCrash(e);
        }
      }
    }

    public ErrorList LoadAllExtensions()
    {
      lock (this.m_syncObject)
      {
        ErrorList errorList = new ErrorList();
        foreach (IKernelExtension kernelExtension in (IEnumerable<IKernelExtension>) this.InnerExtensions.Values)
        {
          if (kernelExtension.Host != null)
            errorList.AddRange((IEnumerable<Redbox.KioskEngine.ComponentModel.Error>) kernelExtension.Host.Load());
        }
        return errorList;
      }
    }

    public ErrorList ResetAllExtensions()
    {
      lock (this.m_syncObject)
      {
        ErrorList errorList = new ErrorList();
        foreach (IKernelExtension kernelExtension in (IEnumerable<IKernelExtension>) this.InnerExtensions.Values)
        {
          if (kernelExtension.Host != null)
            errorList.AddRange((IEnumerable<Redbox.KioskEngine.ComponentModel.Error>) kernelExtension.Host.Reset());
        }
        return errorList;
      }
    }

    public ErrorList ActivateAllExtensions()
    {
      lock (this.m_syncObject)
      {
        ErrorList errorList = new ErrorList();
        foreach (IKernelExtension kernelExtension in (IEnumerable<IKernelExtension>) this.InnerExtensions.Values)
        {
          if (kernelExtension.Host != null)
            errorList.AddRange((IEnumerable<Redbox.KioskEngine.ComponentModel.Error>) kernelExtension.Host.Activate());
        }
        return errorList;
      }
    }

    public ErrorList DeactivateAllExtensions()
    {
      lock (this.m_syncObject)
      {
        ErrorList errorList = new ErrorList();
        foreach (IKernelExtension kernelExtension in (IEnumerable<IKernelExtension>) this.InnerExtensions.Values)
        {
          if (kernelExtension.Host != null)
            errorList.AddRange((IEnumerable<Redbox.KioskEngine.ComponentModel.Error>) kernelExtension.Host.Deactivate());
        }
        return errorList;
      }
    }

    public bool CanSwitch(out ErrorList errors)
    {
      errors = new ErrorList();
      bool flag = true;
      lock (this.m_syncObject)
      {
        foreach (IKernelExtension kernelExtension in (IEnumerable<IKernelExtension>) this.InnerExtensions.Values)
        {
          ErrorList errors1;
          if (kernelExtension.Host != null && !kernelExtension.Host.CanSwitch(out errors1))
          {
            flag = false;
            errors.AddRange((IEnumerable<Redbox.KioskEngine.ComponentModel.Error>) errors1);
          }
        }
      }
      return flag;
    }

    public ReadOnlyCollection<IKernelExtension> Extensions
    {
      get
      {
        return new List<IKernelExtension>((IEnumerable<IKernelExtension>) this.InnerExtensions.Values).AsReadOnly();
      }
    }

    public string[] SearchPath
    {
      get
      {
        IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
        return service != null ? service.GetValue<string[]>("Core", "KernelExtensionSearchPath", new string[1]
        {
          "..\\kext"
        }) : new string[1]{ "..\\kext" };
      }
      set
      {
        ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue<string[]>("Core", "KernelExtensionSearchPath", value);
      }
    }

    internal IDictionary<Guid, IKernelExtension> InnerExtensions
    {
      get
      {
        if (this.m_innerExtensions == null)
          this.m_innerExtensions = (IDictionary<Guid, IKernelExtension>) new Dictionary<Guid, IKernelExtension>();
        return this.m_innerExtensions;
      }
    }

    private KernelExtensionService()
    {
    }
  }
}
