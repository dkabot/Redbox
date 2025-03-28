using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE;
using Redbox.REDS.Framework;
using Redbox.Rental.Model.Health;
using Redbox.Rental.Model.Reservation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;
using System.Xml;

namespace Redbox.KioskEngine.Environment
{
  public class ResourceBundleService : IResourceBundleService
  {
    private string _devBundlePath;
    private string m_applicationPath;
    private BundleCollection m_innerBundles;
    private Dictionary<string, Assembly> _asseblies = new Dictionary<string, Assembly>();
    private string[] _removedBundles = new string[7]
    {
      "content-banner-marketing.bundle",
      "content-incart-rundata.bundle",
      "content-pricing.bundle",
      "content-promo.bundle",
      "content-vend-screen.bundle",
      "content-marketing-rundata.bundle",
      "content-email-opt-in.bundle"
    };

    public static ResourceBundleService Instance => Singleton<ResourceBundleService>.Instance;

    public bool IsRentalRunning
    {
      get
      {
        return this.ActiveBundle != null && ServiceLocator.Instance.GetService<IMacroService>()["ProductName"] == "Rental Application";
      }
    }

    public bool IsPendingBundleSwitch() => this.SwitchToBundle != null;

    public bool SetSwitchToBundle(IResourceBundle bundle)
    {
      if (bundle == this.ActiveBundle && MachineSettingsStore.Instance.GetValue<string>("Store", "Environment", "MISSING") != "Integration")
        return false;
      this.SwitchToBundle = bundle;
      return true;
    }

    public Redbox.KioskEngine.ComponentModel.ErrorList Switch(out IResourceBundle previousBundle)
    {
      LogHelper.Instance.Log("Beginning ResourceBundleService.Switch");
      previousBundle = this.ActiveBundle;
      Redbox.KioskEngine.ComponentModel.ErrorList target = new Redbox.KioskEngine.ComponentModel.ErrorList();
      if (this.SwitchToBundle == null)
        return target;
      SwitchStatusForm switchStatusForm = new SwitchStatusForm();
      if (!string.IsNullOrEmpty(this.SwitchStatusMessage))
        switchStatusForm.StatusLabel = this.SwitchStatusMessage;
      switchStatusForm.Show();
      Application.DoEvents();
      ManualResetEvent manualResetEvent = new ManualResetEvent(false);
      IKernelExtensionService service1 = ServiceLocator.Instance.GetService<IKernelExtensionService>();
      try
      {
        LogHelper.Instance.Log("Checking kernelExtensionService.CanSwitch before bundle switch");
        DateTime now = DateTime.Now;
        Redbox.KioskEngine.ComponentModel.ErrorList errors;
        while (!service1.CanSwitch(out errors))
        {
          LogHelper.Instance.Log("Unable to switch resource bundles due to switch error from kernel extension(s):", LogEntryType.Error);
          foreach (Redbox.KioskEngine.ComponentModel.Error error in (List<Redbox.KioskEngine.ComponentModel.Error>) errors)
            LogHelper.Instance.Log("...{0}", (object) error);
          LogHelper.Instance.Log("Wait for 15 seconds before next check.");
          manualResetEvent.WaitOne(15000);
          if ((DateTime.Now - now).TotalMinutes >= 5.0)
            return target;
        }
        ServiceLocator.Instance.GetService<IHealthServices>()?.ResetMinutesInMaintenanceMode();
      }
      finally
      {
        manualResetEvent.Close();
      }
      try
      {
        using (ExecutionTimer executionTimer = new ExecutionTimer())
        {
          target.AddRange((IEnumerable<Redbox.KioskEngine.ComponentModel.Error>) this.Deactivate());
          new ManualResetEvent(false).WaitOne(5000);
          LogHelper.Instance.Log("Activate resource bundle: {0}", (object) this.SwitchToBundle.Storage.BundlePath);
          this.ActiveBundle = this.SwitchToBundle;
          this.Filter = this.ActiveBundle.CreateFilter();
          this.Filter["runtime"] = "Client";
          this.Filter["platform"] = ".NET/Windows";
          this.Filter["environment"] = this.Environment;
          IConfigurationService service2 = ServiceLocator.Instance.GetService<IConfigurationService>();
          List<string> list = new List<string>();
          service2.GetSettingNames("system", "Filters", out list);
          foreach (string str in list)
          {
            ConfigurationData configurationData = new ConfigurationData();
            if (service2.TryGetConfigurationData("system", "Filters", str, out configurationData) && configurationData.Value.GetType() == typeof (string))
            {
              this.Filter[str] = configurationData.Value;
              LogHelper.Instance.Log("Added filter: {0} = {1}", (object) str, (object) configurationData.Value);
            }
          }
          this.SwitchToBundle = (IResourceBundle) null;
          IManifestInfo manifestInfo1;
          this.ActiveBundle.Activate(this.Filter, out manifestInfo1).CopyToLocalCollection(target);
          List<IResourceBundle> otherBundles = new List<IResourceBundle>();
          List<string> stringList = new List<string>()
          {
            manifestInfo1.StartupScript
          };
          foreach (IBundleSpecifier require in manifestInfo1.Requires)
          {
            IResourceBundle bundle;
            IManifestInfo manifestInfo2;
            this.InnerBundles.GetBundle(require.Name, require.Version.ToString(), out bundle, out manifestInfo2).CopyToLocalCollection(target);
            if (manifestInfo2 != null)
            {
              if (manifestInfo2.BundleType == BundleType.Application)
              {
                target.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("B122", "Only Library and Content bundles may be referenced in the Requires property.", "Specify a bundle that has a type of Library or Content."));
              }
              else
              {
                if (!string.IsNullOrEmpty(manifestInfo2.StartupScript) && !stringList.Contains(manifestInfo2.StartupScript))
                  stringList.Add(manifestInfo2.StartupScript);
                otherBundles.Add(bundle);
              }
            }
          }
          if (!target.ContainsError())
          {
            this.ActiveBundleSet = (IResourceBundleSet) new ResourceBundleSet(string.Format("Bundle Set: {0}, {1}", (object) manifestInfo1.ProductName, (object) manifestInfo1.ProductVersion), this.ActiveBundle, (IEnumerable<IResourceBundle>) otherBundles);
            this.CurrentBundleName = manifestInfo1.ProductName;
            IMacroService service3 = ServiceLocator.Instance.GetService<IMacroService>();
            service3["ProductName"] = manifestInfo1.ProductName;
            service3["ProductVersion"] = manifestInfo1.ProductVersion;
            target.AddRange((IEnumerable<Redbox.KioskEngine.ComponentModel.Error>) service1.ResetAllExtensions());
            ServiceLocator.Instance.GetService<IReservationService>().UnRegisterFromAWSBrokerService(UnRegisterFromBrokerServiceReason.BundleSwitch);
            IKernelService service4 = ServiceLocator.Instance.GetService<IKernelService>();
            foreach (string resourceName in stringList)
            {
              if (!service4.LoadStartupScript(resourceName))
                target.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("B033", string.Format("Unable to load the startup script resource: '{0}'", (object) resourceName), "Review the startup script resource for syntax errors."));
            }
            if (!target.ContainsError())
            {
              IKernelFunctionRegistryService kernelFunctionRegistryService = ServiceLocator.Instance.GetService<IKernelFunctionRegistryService>();
              if (kernelFunctionRegistryService != null)
                this._asseblies.ForEach<KeyValuePair<string, Assembly>>((Action<KeyValuePair<string, Assembly>>) (pair => kernelFunctionRegistryService.RegisterKernelFunctions(pair.Value, pair.Key)));
              service4.RegisterFunctions(kernelFunctionRegistryService.GetKernelFunctions());
              ServiceLocator.Instance.GetService<ICallbackService>().Resume();
              if (this.ShowDebugger)
              {
                this.ShowDebugger = false;
                IDebugService service5 = ServiceLocator.Instance.GetService<IDebugService>();
                if (service5 != null)
                {
                  service5.IsApplicationRunning = false;
                  service5.ActivateDebugger();
                }
              }
              else
              {
                string testControl = manifestInfo1.TestControl;
                if (testControl != null && MachineSettingsStore.Instance.GetValue<string>("Store", "Environment") == "Integration")
                {
                  if (!service4.IsTestPlanExecuting)
                    service4.ExecuteTestPlan(testControl);
                }
                else
                  service4.ExecuteApplication();
              }
            }
          }
          LogHelper.Instance.Log("Time to switch bundle: {0}", (object) executionTimer.Elapsed);
          return target;
        }
      }
      finally
      {
        ServiceLocator.Instance.GetService<IEngineApplication>().AppStarting = false;
        switchStatusForm.Close();
      }
    }

    public Redbox.KioskEngine.ComponentModel.ErrorList LoadBundles()
    {
      Redbox.KioskEngine.ComponentModel.ErrorList target = new Redbox.KioskEngine.ComponentModel.ErrorList();
      this.InnerBundles.Clear();
      List<string> adjustedPaths = new List<string>();
      if (Debugger.IsAttached)
      {
        string devBundleFullPath = this.DevBundleFullPath;
        if (!string.IsNullOrWhiteSpace(devBundleFullPath))
        {
          if (!Directory.Exists(devBundleFullPath))
          {
            LogHelper.Instance.Log("Application Property DevBundlePath {0} doesn't exist", (object) devBundleFullPath);
          }
          else
          {
            string[] files = Directory.GetFiles(devBundleFullPath, "manifest.resource", SearchOption.AllDirectories);
            if (files.Length > 1)
              ((IEnumerable<string>) files).ForEach<string>((Action<string>) (manifestPath => adjustedPaths.Add(Path.GetDirectoryName(manifestPath))));
          }
        }
      }
      if (adjustedPaths.Count == 0)
      {
        foreach (string str in this.SearchPath)
        {
          if (!Path.IsPathRooted(str))
            adjustedPaths.Add(Path.Combine(Path.GetDirectoryName(this.m_applicationPath), str));
          else
            adjustedPaths.Add(str);
        }
      }
      foreach (string str in adjustedPaths)
      {
        string eachPath = str;
        LogHelper.Instance.Log("...Search path: {0}", (object) eachPath);
        if (!Directory.Exists(eachPath))
        {
          target.Add(Redbox.KioskEngine.ComponentModel.Error.NewWarning("B001", string.Format("Bundle directory does not exist: {0}", (object) eachPath), "Correct the bundle search path and try again."));
          return target;
        }
        string[] removedBundlesFullPath = ((IEnumerable<string>) this._removedBundles).Select<string, string>((Func<string, string>) (removedBundle => Path.Combine(eachPath, removedBundle))).ToArray<string>();
        string[] array = ((IEnumerable<string>) Directory.GetFiles(eachPath, "*.bundle")).Where<string>((Func<string, bool>) (bundleFile => !((IEnumerable<string>) removedBundlesFullPath).Contains<string>(bundleFile))).ToArray<string>();
        if (array.Length != 0)
        {
          foreach (string fileName in array)
          {
            ResourceBundle bundle = new ResourceBundle(fileName, ResourceBundleStorageType.Zipped);
            Redbox.REDS.Framework.ErrorList errorList = bundle.LoadResources();
            if (errorList.ContainsError())
            {
              foreach (Redbox.REDS.Framework.Error error in (List<Redbox.REDS.Framework.Error>) errorList)
                target.Add(error.IsWarning ? Redbox.KioskEngine.ComponentModel.Error.NewWarning(error.Code, error.Description, error.Details) : Redbox.KioskEngine.ComponentModel.Error.NewError(error.Code, error.Description, error.Details));
            }
            else
            {
              this.InnerBundles.AddBundleVersion((IResourceBundle) bundle).CopyToLocalCollection(target);
              if (!target.ContainsError())
                LogHelper.Instance.Log("......Loaded bundle '{0}'.", (object) fileName);
            }
          }
        }
        else if (Directory.GetFiles(eachPath, "manifest.resource").Length == 1)
        {
          ResourceBundle bundle = new ResourceBundle(eachPath, ResourceBundleStorageType.FileSystem);
          bundle.LoadResources();
          this.InnerBundles.AddBundleVersion((IResourceBundle) bundle).CopyToLocalCollection(target);
          if (!target.ContainsError())
            LogHelper.Instance.Log("......Loaded loose bundle '{0}'.", (object) eachPath);
        }
      }
      return target;
    }

    public Redbox.KioskEngine.ComponentModel.ErrorList Initialize(
      string applicationPath,
      string devBundlePath = null)
    {
      Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
      this._devBundlePath = devBundlePath;
      LogHelper.Instance.Log("Initialize resource bundle service:");
      ServiceLocator.Instance.AddService(typeof (IResourceBundleService), (object) ResourceBundleService.Instance);
      this.m_applicationPath = applicationPath;
      return errorList;
    }

    public string DevBundleFullPath
    {
      get
      {
        string devBundleFullPath = this._devBundlePath;
        if (string.IsNullOrWhiteSpace(devBundleFullPath))
          return string.Empty;
        if (!Path.IsPathRooted(devBundleFullPath))
          devBundleFullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(this.m_applicationPath), devBundleFullPath));
        return devBundleFullPath;
      }
    }

    public Redbox.KioskEngine.ComponentModel.ErrorList ActivateDefaultBundle()
    {
      BundleSpecifier bundleSpecifier = new BundleSpecifier(this.DefaultBundleName);
      return this.Activate(bundleSpecifier.Name, bundleSpecifier.Version.ToString());
    }

    public Redbox.KioskEngine.ComponentModel.ErrorList Activate(
      string productName,
      string productVersion)
    {
      Redbox.KioskEngine.ComponentModel.ErrorList target = new Redbox.KioskEngine.ComponentModel.ErrorList();
      IResourceBundle bundle;
      IManifestInfo manifestInfo;
      this.InnerBundles.GetBundle(productName, productVersion, out bundle, out manifestInfo).CopyToLocalCollection(target);
      if (manifestInfo != null && manifestInfo.BundleType != BundleType.Application)
      {
        target.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("B122", "Only Application bundles may be activated.", "Specify a bundle that has a type of Application."));
        return target;
      }
      if (!target.ContainsError())
        this.SwitchToBundle = bundle;
      return target;
    }

    public Redbox.KioskEngine.ComponentModel.ErrorList Deactivate()
    {
      Redbox.KioskEngine.ComponentModel.ErrorList errors = new Redbox.KioskEngine.ComponentModel.ErrorList();
      if (this.ActiveBundle != null)
        LogHelper.Instance.Log("Deactivate resource bundle: {0}", (object) this.ActiveBundle.Storage.BundlePath);
      ITweenService service1 = ServiceLocator.Instance.GetService<ITweenService>();
      if (service1 != null)
      {
        LogHelper.Instance.Log("Resetting tween service.");
        service1.Reset();
      }
      ICallbackService service2 = ServiceLocator.Instance.GetService<ICallbackService>();
      service2.Flush();
      service2.Suspend();
      this.Filter = (IResourceBundleFilter) null;
      IKernelExtensionService service3 = ServiceLocator.Instance.GetService<IKernelExtensionService>();
      errors.AddRange((IEnumerable<Redbox.KioskEngine.ComponentModel.Error>) service3.DeactivateAllExtensions());
      LogHelper.Instance.Log("Deactivate all kernel extensions", LogEntryType.Info);
      ServiceLocator.Instance.GetService<IMacroService>().Clear((ICollection<string>) new string[2]
      {
        "RunningPath",
        "EngineVersion"
      });
      ResourceBundleService.ResetServices((List<Redbox.KioskEngine.ComponentModel.Error>) errors);
      this.ActiveBundle = (IResourceBundle) null;
      this.ActiveBundleSet = (IResourceBundleSet) null;
      return errors;
    }

    public void Restart()
    {
      IManifestInfo manifestInfo;
      if (this.GetManifest(out manifestInfo) == null)
        return;
      this.Activate(manifestInfo.ProductName, manifestInfo.ProductVersion);
    }

    public bool HasActiveBundle() => this.ActiveBundle != null;

    public IResourceBundle GetBundle(IBundleSpecifier bundleSpecifier)
    {
      IResourceBundle bundle;
      this.InnerBundles.GetBundle(bundleSpecifier.Name, bundleSpecifier.Version.ToString(), out bundle, out IManifestInfo _);
      return bundle;
    }

    public IResource GetResource(string resourceName)
    {
      return this.ActiveBundleSet == null ? (IResource) null : this.ActiveBundleSet.GetResource(resourceName, this.Filter);
    }

    public ReadOnlyCollection<IResource> GetResources(string typeName)
    {
      return this.ActiveBundleSet == null ? new List<IResource>().AsReadOnly() : this.ActiveBundleSet.GetResources(typeName, this.Filter);
    }

    public ReadOnlyCollection<IResource> GetResourcesUnfiltered(string typeName)
    {
      return this.ActiveBundleSet == null ? new List<IResource>().AsReadOnly() : this.ActiveBundleSet.GetResourcesUnfiltered(typeName);
    }

    public IDictionary<string, IResourceType> GetResourceTypes()
    {
      return this.ActiveBundleSet == null ? (IDictionary<string, IResourceType>) new Dictionary<string, IResourceType>() : this.ActiveBundleSet.GetResourceTypes();
    }

    public Font GetFont(string resourceName, out bool resourceFound)
    {
      resourceFound = false;
      IFontCacheService service = ServiceLocator.Instance.GetService<IFontCacheService>();
      IResource resource = this.GetResource(resourceName);
      if (resource != null)
      {
        float size = 10f;
        FontStyle style = FontStyle.Regular;
        if (resource["point_size"] != null)
          size = Convert.ToSingle(resource["point_size"]);
        if (resource["font_style"] != null)
          style = (FontStyle) resource["font_style"];
        IAspect aspect = resource.GetAspect("content");
        if (aspect != null)
        {
          object content = aspect.GetContent();
          if (content != null)
          {
            resourceFound = true;
            return service.RegisterFont(resource.GetFullName(), (string) resource["font_family"], size, style, (byte[]) content);
          }
        }
        if (resource["font_family"] != null)
        {
          resourceFound = true;
          return service.RegisterFont(resource.GetFullName(), (string) resource["font_family"], size, style);
        }
      }
      return service.RegisterFont("default", "Arial", 12f, FontStyle.Bold);
    }

    public IResource GetManifest(out IManifestInfo manifestInfo)
    {
      manifestInfo = (IManifestInfo) null;
      return this.ActiveBundle == null ? (IResource) null : this.ActiveBundle.GetManifest(this.Filter, out manifestInfo);
    }

    public XmlNode GetXml(string resourceName)
    {
      IResource resource = this.GetResource(resourceName);
      return resource == null ? (XmlNode) null : (XmlNode) resource.GetAspect("content").GetContent();
    }

    public byte[] GetSound(string resourceName)
    {
      IResource resource = this.GetResource(resourceName);
      return resource == null ? (byte[]) null : (byte[]) resource.GetAspect("content").GetContent();
    }

    public XpsDocument GetXpsDocument(string resourceName)
    {
      XpsDocument xpsDocument = (XpsDocument) null;
      IResource resource = this.GetResource(resourceName);
      if (resource != null)
      {
        IAspect aspect = resource.GetAspect("content");
        byte[] content = (byte[]) resource.GetAspect("content").GetContent();
        if (content != null)
        {
          string str = string.Format("memorystream://{0}", (object) aspect.Properties["file"]);
          Package package = PackageStore.GetPackage(new Uri(str));
          if (package == null)
          {
            package = Package.Open((Stream) new MemoryStream(content));
            PackageStore.AddPackage(new Uri(str), package);
          }
          xpsDocument = new XpsDocument(package, CompressionOption.Fast, str);
        }
      }
      return xpsDocument;
    }

    public string GetScript(string resourceName)
    {
      IResource resource = this.GetResource(resourceName);
      if (resource == null)
        return (string) null;
      object content = resource.GetAspect("content").GetContent();
      return content is byte[] ? Encoding.ASCII.GetString((byte[]) content) : (string) content;
    }

    public string GetJsonFile(string resourceName)
    {
      IResource resource = this.GetResource(resourceName);
      if (resource == null)
        return (string) null;
      object content = resource.GetAspect("content").GetContent();
      return content is byte[] ? Encoding.ASCII.GetString((byte[]) content) : (string) content;
    }

    public Image GetBitmap(string resourceName)
    {
      IBitmapCacheService service = ServiceLocator.Instance.GetService<IBitmapCacheService>();
      IResource resource = this.GetResource(resourceName);
      return resource == null ? (Image) null : service.RegisterImage(resource.GetFullName(), (byte[]) resource.GetAspect("content").GetContent());
    }

    public BitmapImage GetBitmapImage(string resourceName)
    {
      IBitmapCacheService service = ServiceLocator.Instance.GetService<IBitmapCacheService>();
      BitmapImage bitmapImage = service.GetBitmapImage(resourceName);
      if (bitmapImage == null)
      {
        this.GetBitmap(resourceName);
        bitmapImage = service.GetBitmapImage(resourceName);
      }
      return bitmapImage;
    }

    public ReadOnlyCollection<object> ExecuteScript(string resourceName)
    {
      return ServiceLocator.Instance.GetService<IKernelService>().ExecuteScript(resourceName);
    }

    public IEnumerable<IBundleSpecifier> Bundles => this.InnerBundles.GetAllBundleVersions();

    public IResourceBundleFilter Filter { get; internal set; }

    public IResourceBundleSet ActiveBundleSet { get; internal set; }

    public IResourceBundle ActiveBundle { get; internal set; }

    public string SwitchStatusMessage { get; set; }

    public bool ShowDebugger { get; set; }

    public string[] SearchPath
    {
      get
      {
        IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
        return service != null ? service.GetValue<string[]>("Core", "BundleSearchPath", new string[1]
        {
          "..\\bundles"
        }) : new string[1]{ "..\\bundles" };
      }
      set
      {
        ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue<string[]>("Core", "BundleSearchPath", value);
      }
    }

    public string Environment
    {
      get
      {
        return ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.GetValue<string>("Store", nameof (Environment), "Production");
      }
      set
      {
        ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue<string>("Store", nameof (Environment), value);
      }
    }

    public string CurrentBundleName { get; set; }

    public string DefaultBundleName
    {
      get
      {
        IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
        object obj;
        return service != null && service.TryGetObject("system", "Store", "DefaultBundle", out obj) ? obj.ToString() : "Rental Application,*";
      }
      set
      {
        ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue<string>("Core", "DefaultBundle", value);
      }
    }

    internal BundleCollection InnerBundles
    {
      get
      {
        if (this.m_innerBundles == null)
          this.m_innerBundles = new BundleCollection();
        return this.m_innerBundles;
      }
    }

    internal IResourceBundle SwitchToBundle { get; set; }

    private ResourceBundleService()
    {
    }

    private static void ResetServices(List<Redbox.KioskEngine.ComponentModel.Error> errors)
    {
      ICallbackService service1 = ServiceLocator.Instance.GetService<ICallbackService>();
      IViewService service2 = ServiceLocator.Instance.GetService<IViewService>();
      ITimerService service3 = ServiceLocator.Instance.GetService<ITimerService>();
      IInputService service4 = ServiceLocator.Instance.GetService<IInputService>();
      ServiceLocator.Instance.GetService<ISoundService>();
      IKernelService service5 = ServiceLocator.Instance.GetService<IKernelService>();
      ISchedulerService service6 = ServiceLocator.Instance.GetService<ISchedulerService>();
      IRenderingService service7 = ServiceLocator.Instance.GetService<IRenderingService>();
      IDataStoreService service8 = ServiceLocator.Instance.GetService<IDataStoreService>();
      IFontCacheService service9 = ServiceLocator.Instance.GetService<IFontCacheService>();
      IStyleSheetService service10 = ServiceLocator.Instance.GetService<IStyleSheetService>();
      IBitmapCacheService service11 = ServiceLocator.Instance.GetService<IBitmapCacheService>();
      IShoppingSessionService service12 = ServiceLocator.Instance.GetService<IShoppingSessionService>();
      IEnvironmentNotificationService service13 = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
      IKernelFunctionRegistryService service14 = ServiceLocator.Instance.GetService<IKernelFunctionRegistryService>();
      service6?.Reset();
      service3?.Reset();
      if (service7.ActiveScene != null)
        service7.ActiveScene.Clear();
      service2.Reset();
      service10?.Reset();
      service11.DropCache();
      service9.DropCache();
      service5.Reset();
      service14.Reset();
      service4.Reset();
      service13.Reset();
      service12.Reset();
      service1.Reset();
      service8.CloseAllStores();
    }

    public void RegisterAssemblyFunctions(string name, Assembly assembly)
    {
      this._asseblies[name] = assembly;
    }
  }
}
