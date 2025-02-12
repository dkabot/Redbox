using HALUtilities.KioskTest;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Threading;
using Redbox.HAL.Core;
using System;

namespace HALUtilities
{
  internal sealed class KioskTestRunner : IDisposable
  {
    private bool Disposed;
    private readonly double? RunTime;
    private readonly KioskTestLogger Logger;
    private readonly NamedLock m_instanceLock;
    private readonly Guid ProgramLock = new Guid("{7B780DBE-F37B-4f4a-83FA-C59F5B2C87D2}");
    private readonly AbstractKioskTest Test;

    public void Dispose()
    {
      if (this.Disposed)
        return;
      this.Disposed = true;
      this.Test.Dispose();
      if (this.m_instanceLock != null)
        this.m_instanceLock.Dispose();
      this.Logger.Dispose();
    }

    internal static int RunKioskTest(string[] args, HardwareService s)
    {
      int num = -1;
      using (GetMachineInfoExecutor machineInfoExecutor = new GetMachineInfoExecutor(s))
      {
        machineInfoExecutor.Run();
        if (HardwareJobStatus.Completed != machineInfoExecutor.EndStatus || machineInfoExecutor.Info.Configuration == KioskConfiguration.None)
        {
          Console.WriteLine("Unable to determine machine configuration.");
        }
        else
        {
          using (KioskTestRunner kioskTestRunner = new KioskTestRunner(machineInfoExecutor.Info, args, s))
            num = kioskTestRunner.Run(machineInfoExecutor.Info);
        }
      }
      return num;
    }

    private KioskTestRunner(MachineInformation config, string[] args, HardwareService service)
    {
      this.Logger = new KioskTestLogger();
      ServiceLocator.Instance.AddService<ILogger>((object) this.Logger);
      if (config.Configuration == KioskConfiguration.None)
      {
        LogHelper.Instance.Log("Unsupported test configuration {0}", (object) config);
        throw new ArgumentException();
      }
      switch (config.Configuration)
      {
        case KioskConfiguration.R504:
        case KioskConfiguration.R630:
          this.Test = (AbstractKioskTest) new QlmKioskTest(config.Configuration, service);
          break;
        case KioskConfiguration.R717:
          this.Test = (AbstractKioskTest) new KioskTest_R717(service);
          break;
      }
      if (args.Length != 0)
      {
        for (int index = 0; index < args.Length; ++index)
        {
          if (args[index].StartsWith("--runtime", StringComparison.CurrentCultureIgnoreCase))
            this.RunTime = CommandLineOption.GetOptionVal<double?>(args[index], this.RunTime);
          else if (args[index] == "-help")
          {
            Console.WriteLine("\nList of Commands:\n--------------------\n");
            Console.WriteLine("\t--runTime:{number}\tSet the number of hours for test to run");
            Console.WriteLine("\t--vend:{number}\tSet the number of moves between each vend attempt, 0 prevents vends from being attempted");
            Console.WriteLine("\t-excludeAroundDump\tIgnores slots 81 & 85 on deck 8 VMZ machines.");
            Console.WriteLine("\t--quickTest:{true|false}\tTests disk at slots 1, 30 and 60 on each deck. Assumes a disk at 1,1 on each deck.");
            Console.WriteLine("\t--rewriteDataOnSuccess:{true|false}\tOn successful test, updates GAMP data with new values.");
            Console.WriteLine("\t-oneDiskQuickTest\tPerforms a quick test at slots 1, 30 and 60 on each deck. Assumes single disk at 1,1.");
            Console.WriteLine("\t--testDisksAroundBin:{true|false}\tPerforms a test on slots adjacent to the dumpbin.");
            Console.WriteLine("\t-help\t\t\tPrints this screen");
            Environment.Exit(0);
          }
          else
            this.Test.AcceptArgument(args[index]);
        }
      }
      this.m_instanceLock = new NamedLock(this.ProgramLock.ToString());
    }

    private int Run(MachineInformation config)
    {
      if (!this.m_instanceLock.IsOwned)
      {
        Console.WriteLine("Only one instance of the Kiosk Test is allowed to run - bailing test.");
        return -2;
      }
      TestLocation testLocation = new TestLocation(1, 1);
      if (this.Test.SingleTestOnly())
        return this.Test.RunSingleTests(testLocation, config);
      TimeSpan? runTime = new TimeSpan?();
      if (this.RunTime.HasValue)
        runTime = new TimeSpan?(TimeSpan.FromHours(this.RunTime.Value));
      return this.Test.Run(runTime, testLocation, config);
    }
  }
}
