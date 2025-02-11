using Microsoft.Win32;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Configuration;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.DataMatrix.Framework;
using Redbox.HAL.DataStorage;
using Redbox.HAL.Script.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;

namespace Redbox.HAL.PackageInstaller
{
  internal sealed class PackageInstaller : IDisposable
  {
    private readonly string[] DefaultDatabases = new string[2]
    {
      "HALCounters.vdb3",
      "HALData.vdb3"
    };
    private readonly string ServiceInstallerBatchFile;
    private readonly TextWriter Writer;
    private readonly string InstallerLog;
    private readonly string InstallLogsDirectory;
    private readonly string ConfigurationFullPath;
    private readonly IRuntimeService RuntimeService;
    private readonly List<IPackageInstallHandler> PackageHandlers = new List<IPackageInstallHandler>();
    private readonly ErrorList InstallErrors = new ErrorList();
    private const int BackupsToKeep = 2;
    private const string KioskLogsDirectory = "C:\\Program Files\\Redbox\\KioskLogs";
    private const string DefaultConfiguration = "HAL.xml.default";
    private const string ConfigurationFileName = "hal.xml";
    private bool Disposed;

    public void Dispose()
    {
      if (this.Disposed)
        return;
      this.Disposed = true;
      if (this.InstallErrors.ContainsError())
      {
        this.Writer.WriteLine("** Installation failed with the following errors **");
        this.InstallErrors.ForEach((Action<Error>) (error => this.Writer.WriteLine("{0}: {1}: {2} ", (object) error.Code, (object) error.Description, (object) error.Details)));
      }
      this.Writer.Dispose();
    }

    private PackageInstaller()
    {
      this.RuntimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
      this.ConfigurationFullPath = this.RuntimeService.RuntimePath("hal.xml");
      this.InstallLogsDirectory = this.RuntimeService.InstallPath("InstallLogs");
      if (!Directory.Exists(this.InstallLogsDirectory))
      {
        try
        {
          Directory.CreateDirectory(this.InstallLogsDirectory);
        }
        catch
        {
        }
      }
      this.InstallerLog = Path.Combine(this.InstallLogsDirectory, "Package-installer.log");
      try
      {
        this.Writer = (TextWriter) new StreamWriter(this.InstallerLog, true);
      }
      catch
      {
        this.Writer = (TextWriter) StreamWriter.Null;
      }
      this.ServiceInstallerBatchFile = Path.Combine(this.RuntimeService.AssemblyDirectory, "register-service.bat");
    }

    private void CreateDirectories()
    {
      List<string> list = new List<string>();
      using (new DisposeableList<string>((IList<string>) list))
      {
        list.Add("C:\\Program Files\\Redbox\\KioskLogs");
        list.Add(Path.Combine(this.RuntimeService.AssemblyDirectory, "data"));
        list.Add("c:\\Program Files\\Redbox\\HALService\\Video");
        list.ForEach((Action<string>) (directory =>
        {
          try
          {
            if (Directory.Exists(directory))
              return;
            Directory.CreateDirectory(directory);
          }
          catch (Exception ex)
          {
            this.Writer.WriteLine("Unable to create directory: {0}", (object) ex.Message);
          }
        }));
      }
    }

    private void InstallDatabases()
    {
      Array.ForEach<string>(this.DefaultDatabases, (Action<string>) (fileName =>
      {
        string str1 = this.RuntimeService.RuntimePath(string.Format("data\\{0}", (object) fileName));
        if (File.Exists(str1))
        {
          this.Writer.WriteLine("The database {0} already exists - take no further action.", (object) fileName);
        }
        else
        {
          string sourceFileName = this.RuntimeService.RuntimePath(string.Format("{0}.default", (object) fileName));
          string str2 = string.Format("copy {0} -> {1}", (object) sourceFileName, (object) str1);
          try
          {
            File.Copy(sourceFileName, str1);
            this.Writer.WriteLine("{0}: SUCCESS", (object) str2);
          }
          catch (Exception ex)
          {
            this.InstallErrors.Add(Error.NewError("I001", "Copy failure", string.Format("{0}: FAILURE reason = {1}", (object) str2, (object) ex.Message)));
          }
        }
      }));
    }

    private void BackupConfiguration()
    {
      this.RuntimeService.CreateBackup(this.ConfigurationFullPath, BackupAction.Copy);
      List<FileInfo> backups = new List<FileInfo>();
      using (new DisposeableList<FileInfo>((IList<FileInfo>) backups))
      {
        Array.ForEach<string>(Directory.GetFiles(this.RuntimeService.AssemblyDirectory, "hal*.xml"), (Action<string>) (bf =>
        {
          FileInfo fileInfo = new FileInfo(bf);
          if (fileInfo.Name.Equals("hal.xml", StringComparison.CurrentCultureIgnoreCase) || fileInfo.Name.Equals("HAL.xml.default", StringComparison.CurrentCultureIgnoreCase))
            return;
          backups.Add(fileInfo);
        }));
        backups.Sort((Comparison<FileInfo>) ((x, y) => x.CreationTime.CompareTo(y.CreationTime)));
        if (backups.Count <= 2)
          return;
        for (int index = 0; index < backups.Count - 2; ++index)
          this.RuntimeService.SafeDelete(backups[index].FullName);
      }
    }

    private bool ServiceIsInstalled()
    {
      foreach (ServiceController service in ServiceController.GetServices())
      {
        if (service.ServiceName.Equals("halsvc$default", StringComparison.CurrentCultureIgnoreCase))
          return true;
      }
      return false;
    }

    private void InstallService()
    {
      if (!File.Exists(this.ServiceInstallerBatchFile))
        this.InstallErrors.Add(Error.NewError("I003", "No service batch file.", "The service install batch file is missing."));
      else if (this.ServiceIsInstalled())
      {
        this.Writer.WriteLine("Service already installed - don't need to register.");
      }
      else
      {
        using (Process process = new Process())
        {
          process.StartInfo.FileName = this.ServiceInstallerBatchFile;
          process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
          process.Start();
          process.WaitForExit();
          int exitCode = process.ExitCode;
          if (exitCode == 0)
          {
            this.Writer.WriteLine("Register service was successful.");
          }
          else
          {
            this.Writer.WriteLine("Register service returned with code {0}", (object) exitCode);
            this.InstallErrors.Add(Error.NewError("I004", "Service install failed.", string.Format("The service install returned error code {0}", (object) exitCode)));
          }
        }
      }
    }

    private void RegisterHandlers()
    {
      this.Writer.WriteLine("Register package handlers");
      string[] files = Directory.GetFiles(this.RuntimeService.AssemblyDirectory, "*.dll");
      string[] array = new string[4]
      {
        "CortexDecoder.dll",
        "Hasp_windows_110892.dll",
        "CortexDecoderHasp.dll",
        "CDHelper.dll"
      };
      foreach (string str in files)
      {
        string file = Path.GetFileName(str);
        if (!string.IsNullOrEmpty(Array.Find<string>(array, (Predicate<string>) (a => a.Equals(file, StringComparison.CurrentCultureIgnoreCase)))))
        {
          this.Writer.WriteLine(" Exclude {0} ( '{1}' ) from handler search", (object) file, (object) str);
        }
        else
        {
          try
          {
            Assembly assembly = Assembly.LoadFrom(str);
            Type type1 = typeof (IPackageInstallHandler);
            foreach (Type type2 in assembly.GetTypes())
            {
              if (type1.IsAssignableFrom(type2) && !type2.IsInterface)
              {
                IPackageInstallHandler instance = (IPackageInstallHandler) Activator.CreateInstance(type2);
                this.PackageHandlers.Add(instance);
                instance.OnRegister();
              }
            }
          }
          catch (Exception ex)
          {
            this.Writer.WriteLine(" Unable to load assembly '{0}' to scan for installer handlers.", (object) str);
            this.Writer.WriteLine(" Exception: {0}", (object) ex.Message);
          }
        }
      }
      this.Writer.WriteLine("There are {0} handlers registered.", (object) this.PackageHandlers.Count);
    }

    private void OnUpgrade()
    {
      this.PackageHandlers.ForEach((Action<IPackageInstallHandler>) (handler => handler.OnUpgrade(this.InstallErrors, this.Writer)));
    }

    private void OnNewInstall()
    {
      this.PackageHandlers.ForEach((Action<IPackageInstallHandler>) (handler => handler.OnNewInstall(this.InstallErrors, this.Writer)));
    }

    private static void Main(string[] args)
    {
      ServiceLocator.Instance.AddService<ITableTypeFactory>((object) new TableTypeFactory());
      ServiceLocator.Instance.AddService<IFormattedLogFactoryService>((object) new FormattedLogFactory());
      ServiceLocator.Instance.AddService<IRuntimeService>((object) new Redbox.HAL.Component.Model.Services.RuntimeService());
      using (Redbox.HAL.PackageInstaller.PackageInstaller installer = new Redbox.HAL.PackageInstaller.PackageInstaller())
      {
        if (installer.ServiceIsInstalled())
        {
          ServiceController serviceController = new ServiceController("halsvc$default");
          try
          {
            serviceController.Stop();
            serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30.0));
          }
          catch
          {
          }
        }
        installer.CreateDirectories();
        Platform platform = installer.RuntimeService.GetPlatform();
        string str;
        try
        {
          str = typeof (Redbox.HAL.PackageInstaller.PackageInstaller).Assembly.GetName().Version.ToString();
        }
        catch
        {
          str = "1.0.0.x";
        }
        installer.Writer.WriteLine("HAL package installer version {0} running on {1} at {2} (platform = {3})", (object) str, (object) DateTime.Now.ToShortDateString(), (object) DateTime.Now.ToShortTimeString(), (object) platform.ToString());
        if (Platform.Windows7 != platform)
        {
          installer.Writer.WriteLine("** ERROR ** This version is not supported on this platform ( {0} )", (object) platform.ToString());
        }
        else
        {
          installer.InstallDatabases();
          if (installer.InstallErrors.ContainsError())
          {
            installer.Writer.WriteLine("** There were problems copying databases - ABORTING install: **");
            installer.InstallErrors.ForEach((Action<Error>) (error => installer.Writer.WriteLine(error.ToString())));
          }
          else
          {
            ServiceLocator.Instance.AddService<IDataTableService>((object) new DataTableService(false));
            ServiceLocator.Instance.AddService<IPersistentMapService>((object) new PersistentMapService());
            ConfigurationService instance = ConfigurationService.Make(installer.ConfigurationFullPath);
            ServiceLocator.Instance.AddService<IConfigurationService>((object) instance);
            instance.RegisterConfiguration(Configurations.Controller.ToString(), (IAttributeXmlConfiguration) ControllerConfiguration.Instance);
            instance.RegisterConfiguration(Configurations.Camera.ToString(), (IAttributeXmlConfiguration) BarcodeConfiguration.MakeNewInstance2());
            installer.RegisterHandlers();
            if (File.Exists(installer.ConfigurationFullPath))
            {
              installer.BackupConfiguration();
              installer.Writer.WriteLine("The configuration file is already present.");
              instance.LoadAndUpgrade(installer.InstallErrors);
              installer.OnUpgrade();
            }
            else
            {
              installer.Writer.WriteLine("Installing configuration from GAMP.");
              string sourceFileName = installer.RuntimeService.RuntimePath("HAL.xml.default");
              try
              {
                File.Copy(sourceFileName, installer.ConfigurationFullPath);
              }
              catch (Exception ex)
              {
                installer.InstallErrors.Add(Error.NewError("I004", "Configuration install failed.", string.Format("Unable to move file '{0}' - error: {1}", (object) sourceFileName, (object) ex.Message)));
                return;
              }
              instance.LoadAndImport(installer.InstallErrors);
              if (installer.InstallErrors.ErrorCount == 0)
                installer.OnNewInstall();
              else
                installer.Writer.WriteLine("** Gamp data import failed - the configuration file may be incomplete. **");
            }
            string assemblyFile = Path.Combine(installer.RuntimeService.AssemblyDirectory, "halsvc-win32.exe");
            try
            {
              using (RegistryKey subKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Redbox\\HAL"))
              {
                AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyFile);
                subKey.SetValue("Product Version", (object) assemblyName.Version.ToString());
              }
            }
            catch (Exception ex)
            {
              installer.Writer.WriteLine("Unable to set service registry key.", (object) ex);
              installer.InstallErrors.Add(Error.NewError("I001", "Service registration failure", ex));
            }
            installer.InstallService();
          }
        }
      }
    }
  }
}
