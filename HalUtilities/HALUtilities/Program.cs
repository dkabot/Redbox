using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Component.Model.Services;
using Redbox.HAL.Core;
using Redbox.IPC.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Timers;

namespace HALUtilities
{
  internal sealed class Program
  {
    private static readonly ManualResetEvent GenericWaiter = new ManualResetEvent(false);
    private const string CSVDirectory = "C:\\Program Files\\Redbox\\inventory-snapshots";
    private const int ServiceTimeout = 30000;
    private const string DuplicateCounterFile = "DuplicateCounter.log";

    private static void PrintUsageAndExit() => Program.PrintUsageAndExit(new string[0]);

    private static void PrintUsageAndExit(string[] secureOptions)
    {
      Console.WriteLine("HALUtilities.exe version {0}", (object) typeof (Program).Assembly.GetName().Version);
      Console.WriteLine("## Options ## ");
      Console.WriteLine("  Where: ");
      Console.WriteLine("  \t\t--cameraGeneration: returns which camera generation is in use.");
      Console.WriteLine("  \t\t--decoderType: returns which barcode decoder is in use.");
      Console.WriteLine("  \t\t--findTripplite:<true|false>  Probes for any tripplite devices attached to the computer.");
      Console.WriteLine("  \t\t-findABEDevice  Probes for an attached ABE device via HAL service.");
      Console.WriteLine("  \t\t-findABEDeviceThroughScan  Probes for an attached ABE device via USB query.");
      Console.WriteLine("  \t\t-findKFCDevices  Probes for active touchscreen & camera attached to computer. Logs to KFC.log.");
      Console.WriteLine("  \t\t-findActiveTouchScreen  Probes for active touchscreen attached to computer. Logs to TSLocator.log.");
      Console.WriteLine("  \t\t-findActiveCamera  Probes for active camera attached to computer. Logs to CameraLocator.log.");
      Console.WriteLine("  \t\t-resetControlSystem  Resets the kiosk control system.");
      Console.WriteLine("  \t\t-resetMotionControl  Resets the motion control system.");
      Console.WriteLine("  \t\t-resetPDCMCounter  Resets the PDCM failure count.");
      Console.WriteLine("  \t\t-interactive  Starts interactive mode.");
      Console.WriteLine("  \t\t-dumpTouchScreens  Dumps monitor class devices to Monitor.log");
      Console.WriteLine("  \t\t-dumpDevices  Dumps all device classes to AllDevices.log");
      Console.WriteLine("  \t\t-interactive  Starts interactive mode.");
      Console.WriteLine("  \t\t-e syncs empties ");
      Console.WriteLine("  \t\t-unknown syncs unknowns ");
      Console.WriteLine("  \t\t-fullSync schedules a low-priority sync");
      Console.WriteLine("  \t\t-dumpInventory writes the inventory to the file \"inventory.xml\"");
      Console.WriteLine("  \t\t-inventorysnapshot   if specified, generates an inventory snapshot to a csv file in \"C:\\Program Files\\Redbox\\inventory-snapshot\"");
      Console.WriteLine("  \t\t-importInventory takes inventory from an XML file and copies it into inventory");
      Console.WriteLine("  \t\t-updateConfiguration CONFIG:OPTION:NEWVALUE:VALIDATE Update OPTION in configuration CONFIG to value NEWVALUE.");
      Console.WriteLine("  \t\t-restartQR Tries to restart the QR device.");
      Console.WriteLine("  \t\t-stopQR Tries to stop the QR device (does not remove it from configuration).");
      Console.WriteLine("  \t\t-checkQRStatus If the QR device is not configured and active, will generate a 'qr-status.log' file with any errors.");
      Console.WriteLine("  \t\t-version Report the tool's verison.");
      Console.WriteLine("  \t\t-checkDupCounter Generate a log file if the machine has counters.");
      Console.WriteLine("  \t\t-resetDupCounter Reset the duplicate barcode counter.");
      Console.WriteLine("  \t\t-updateReboot={time} Updates the reboot time and cycles the service.");
      Console.WriteLine("  \t\t-checkInventory={path} Checks HAL inventory against file specified.");
      Console.WriteLine("  \t\t-assemblyVersions Dumps the HAL assembly versions to the console.");
      Console.WriteLine("  \t\t-bmpconverter Specifies to run as bitmap converter; passes remaining arguments to converter.");
      Array.ForEach<string>(secureOptions, (Action<string>) (secMsg => Console.WriteLine(secMsg)));
      Console.WriteLine("  \t\t-help print this message and exit.");
      Environment.Exit(1);
    }

    private static void PrintSecureHelp()
    {
      Program.PrintUsageAndExit(new string[10]
      {
        "  \t\t-randomSync  Runs a random sync of the kiosk.",
        "  \t\t-installDefaultInventoryDB Backs up old inventory DB, and copies default.",
        "  \t\t-installDefaultCountersDB Backs up old counters DB, and copies default.",
        "  \t\t-updateQREEPROM:\"<c:\asdf>\") Updates the images on the QR device, path is optional, if not provided it will use the assembly/qr-chamfer.",
        "  \t\t-configureDoorSensors:on|off Updates HAL to include/exclude door sensors.",
        "  \t\t-configureVMZ Configures the HAL instance for a VMZ machine.",
        "  \t\t-dumpTouchScreens Dumps touchscreen monitor info to TouchScreen.txt.",
        "  \t\t-backupConfig Backs up the hal.xml & gamp files.",
        "  \t\t-analyzeEmptyStuck:fullDump Analyzes empty/stuck in vend logs. If file is specified, will analyze just that file.",
        "  \t\t-fraudStressTest [--scanPause:pause --iterations:iter --readPause:rp] Runs a stress test on the fraud sensor."
      });
    }

    private static void GenericimerFired(object source, ElapsedEventArgs e)
    {
      Program.GenericWaiter.Set();
    }

    private static string[] GetProgramArgs(string[] allArgs, int start)
    {
      string[] destinationArray = new string[allArgs.Length - (start + 1)];
      Array.Copy((Array) allArgs, start + 1, (Array) destinationArray, 0, destinationArray.Length);
      return destinationArray;
    }

    private static void OnSearchAndFix(
      Program.GetResult getResult,
      IUsbDeviceService usb,
      Program.CorrectionOptions options)
    {
      ConsoleLogger consoleLogger = new ConsoleLogger(true);
      IQueryUsbDeviceResult queryUsbDeviceResult = getResult(usb);
      if (!queryUsbDeviceResult.Found)
      {
        LogHelper.Instance.Log("Unable to find device.");
        Environment.Exit(256);
      }
      int exitCode = 0;
      LogHelper.Instance.Log("Found device {0}", (object) queryUsbDeviceResult);
      if (queryUsbDeviceResult.Running)
        LogHelper.Instance.Log("The device appears to be working.");
      else if ((Program.CorrectionOptions.RestartDevice & options) != Program.CorrectionOptions.None)
      {
        if (queryUsbDeviceResult.IsNotStarted)
          LogHelper.Instance.Log(" Device not started; RESET returned {0}", (object) queryUsbDeviceResult.Descriptor.ResetDriver());
        else if (queryUsbDeviceResult.IsDisabled)
          LogHelper.Instance.Log(" Device not enabled; change to enable returned {0}", (object) usb.ChangeByHWID(queryUsbDeviceResult.Descriptor, DeviceState.Enable));
        DeviceStatus deviceStatus = usb.FindDeviceStatus(queryUsbDeviceResult.Descriptor);
        LogHelper.Instance.Log(" Device status AFTER fix = {0}", (object) deviceStatus);
        if ((deviceStatus & DeviceStatus.Found) != DeviceStatus.None && (deviceStatus & DeviceStatus.Enabled) != DeviceStatus.None)
        {
          LogHelper.Instance.Log("  ** Successfully reset the device **");
        }
        else
        {
          LogHelper.Instance.Log("  !!Failed to reset the device!!");
          exitCode = 258;
        }
      }
      Environment.Exit(exitCode);
    }

    private static void Main(string[] args)
    {
      if (args.Length == 0)
        Program.PrintUsageAndExit();
      IRuntimeService runtimeService = (IRuntimeService) new RuntimeService();
      ServiceLocator.Instance.AddService<IRuntimeService>((object) runtimeService);
      ServiceLocator.Instance.AddService<IDeviceSetupClassFactory>((object) new DeviceSetupClassFactory());
      UsbDeviceService usbDeviceService = new UsbDeviceService(true);
      ServiceLocator.Instance.AddService<IUsbDeviceService>((object) usbDeviceService);
      HardwareService hardwareService1 = new HardwareService(IPCProtocol.Parse(Constants.HALIPCStrings.TcpServer));
      ClientHelper clientHelper = new ClientHelper(hardwareService1);
      bool flag = true;
      for (int start = 0; start < args.Length; ++start)
      {
        if (args[start].StartsWith("--protocol"))
        {
          string optionValue = CommandLineOption.GetOptionValue(args[start], "=");
          try
          {
            hardwareService1 = new HardwareService(IPCProtocol.Parse(optionValue));
            clientHelper.Dispose();
            clientHelper = new ClientHelper(hardwareService1);
          }
          catch
          {
          }
        }
        else if (args[start] == "--cameraGeneration")
          Console.WriteLine(hardwareService1.CurrentCameraGeneration.ToString());
        else if (args[start] == "--decoderType")
        {
          Console.WriteLine(hardwareService1.BarcodeDecoder.ToString());
        }
        else
        {
          if (args[start] == "-scheduleTestSync")
          {
            List<Location> locationList = new List<Location>()
            {
              new Location() { Deck = 1, Slot = 1 },
              new Location() { Deck = 1, Slot = 2 }
            };
            HardwareService hardwareService2 = hardwareService1;
            List<Location> locations = locationList;
            HardwareJobSchedule schedule = new HardwareJobSchedule();
            schedule.Priority = HardwareJobPriority.Low;
            HardwareJob hardwareJob;
            hardwareService2.HardSync(locations, schedule, out hardwareJob);
            break;
          }
          if (args[start].StartsWith("--statusCCR"))
          {
            using (TestAndResetCCRExecutor resetCcrExecutor = new TestAndResetCCRExecutor(hardwareService1))
            {
              resetCcrExecutor.Run();
              if (HardwareJobStatus.Errored == resetCcrExecutor.EndStatus)
                Console.WriteLine("!! Failed to reset CCR !!");
              else if (HardwareJobStatus.Completed == resetCcrExecutor.EndStatus)
                Console.WriteLine("CCR should be working.");
            }
          }
          else if (args[start].StartsWith("--statusCamera"))
          {
            Program.CorrectionOptions options = Program.CorrectionOptions.None;
            if (CommandLineOption.GetOptionVal<bool>(args[start], false))
              options |= Program.CorrectionOptions.RestartDevice;
            Program.OnSearchAndFix((Program.GetResult) (_usb => _usb.FindCamera()), (IUsbDeviceService) usbDeviceService, options);
          }
          else if (args[start].StartsWith("--removeStats"))
          {
            HardwareCorrectionStatistic optionVal = CommandLineOption.GetOptionVal<HardwareCorrectionStatistic>(args[start], HardwareCorrectionStatistic.None);
            if (optionVal != HardwareCorrectionStatistic.None)
            {
              HardwareService hardwareService3 = hardwareService1;
              int stat = (int) optionVal;
              HardwareJobSchedule schedule = new HardwareJobSchedule();
              schedule.Priority = HardwareJobPriority.High;
              HardwareJob job;
              if (!hardwareService3.RemoveHardwareCorrectionStats((HardwareCorrectionStatistic) stat, schedule, out job).Success)
              {
                Console.WriteLine("Failed to schedule remove stats job.");
              }
              else
              {
                HardwareJobStatus endStatus;
                clientHelper.WaitForJob(job, out endStatus);
                Console.WriteLine("Remove all stats job {0} ended with status {1}", (object) job.ID, (object) endStatus);
              }
            }
          }
          else if (args[start].StartsWith("-removeAllStats"))
          {
            HardwareService hardwareService4 = hardwareService1;
            HardwareJobSchedule schedule = new HardwareJobSchedule();
            schedule.Priority = HardwareJobPriority.High;
            HardwareJob job;
            if (!hardwareService4.RemoveHardwareCorrectionStats(schedule, out job).Success)
            {
              Console.WriteLine("Failed to schedule remove all stats job.");
            }
            else
            {
              HardwareJobStatus endStatus;
              clientHelper.WaitForJob(job, out endStatus);
              Console.WriteLine("Remove all stats job {0} ended with status {1}", (object) job.ID, (object) endStatus);
            }
          }
          else if (args[start].StartsWith("--consoleDebug"))
          {
            flag = CommandLineOption.GetOptionVal<bool>(args[start], flag);
          }
          else
          {
            if (args[start] == "-customerReturn")
            {
              Thread thread = new Thread(new ParameterizedThreadStart(CustomerReturn.RunExecutor));
              thread.Start((object) hardwareService1);
              thread.Join();
              break;
            }
            if (args[start] == "-returnUnknown")
            {
              using (ReturnUnknownExecutor returnUnknownExecutor = new ReturnUnknownExecutor(hardwareService1))
              {
                returnUnknownExecutor.Run();
                Console.WriteLine("{0} ( ID = {1} ) ended with status {2}", (object) returnUnknownExecutor.Job.ProgramName, (object) returnUnknownExecutor.ID, (object) returnUnknownExecutor.EndStatus);
                Console.WriteLine(" -- program results --");
                returnUnknownExecutor.Results.ForEach((Action<ProgramResult>) (each => Console.WriteLine("  Code: {0}  Message {1}", (object) each.Code, (object) each.Message)));
                break;
              }
            }
            else if (args[start].StartsWith("-randomSync"))
            {
              int defVal1 = 1;
              int num1 = 1;
              int num2 = 1;
              bool defVal2 = false;
              for (int index = start + 1; index < args.Length; ++index)
              {
                if (args[index].StartsWith("--iterations"))
                  defVal1 = CommandLineOption.GetOptionVal<int>(args[index], defVal1);
                else if (args[index].StartsWith("--vendTime"))
                  num1 = CommandLineOption.GetOptionVal<int>(args[index], num1);
                else if (args[index].StartsWith("--vendFrequency"))
                  num2 = CommandLineOption.GetOptionVal<int>(args[index], num2);
                else if (args[index].StartsWith("--oneDisk"))
                  defVal2 = CommandLineOption.GetOptionVal<bool>(args[index], defVal2);
              }
              int exitCode = 0;
              for (int index = 1; index <= defVal1; ++index)
              {
                Console.WriteLine("Iteration {0} of {1} total.", (object) index, (object) defVal1);
                JobExecutor jobExecutor;
                if (defVal2)
                {
                  jobExecutor = (JobExecutor) new OneDiskRandomSyncExecutor(hardwareService1, num1, num2);
                  jobExecutor.AddSink((HardwareEvent) ((j, eventTime, eventMessage) => Console.WriteLine("{0}: {1}", (object) eventTime, (object) eventMessage)));
                }
                else
                  jobExecutor = (JobExecutor) new RandomSyncExecutor(hardwareService1, num1, num2);
                using (jobExecutor)
                {
                  jobExecutor.Run();
                  Console.WriteLine("Job {0} ends with status {1}", (object) jobExecutor.ID, (object) jobExecutor.EndStatus.ToString());
                  if (HardwareJobStatus.Completed != jobExecutor.EndStatus)
                  {
                    Console.WriteLine("** Tool is exiting after job failure **");
                    exitCode = 1;
                    break;
                  }
                }
              }
              Environment.Exit(exitCode);
            }
            else
            {
              if (args[start].StartsWith("--loopy"))
              {
                LoopyExecutor.RunExecutor(CommandLineOption.GetOptionVal<int>(args[start], 10), hardwareService1);
                break;
              }
              if (args[start].StartsWith("--msgServer"))
                AbstractPipeSever.RunServer(args[start], PipeServers.Message, flag);
              else if (args[start].StartsWith("--pipeServer"))
                AbstractPipeSever.RunServer(args[start], PipeServers.Basic, flag);
              else if (args[start] == "-rdbxPipeServer")
                ServiceHostRunner.Run(Constants.HALIPCStrings.PipeServer, flag);
              else if (args[start] == "-rdbxTcpServer")
                ServiceHostRunner.Run(Constants.HALIPCStrings.TcpServer, flag);
              else if ("-kioskTest" == args[start])
                Environment.Exit(KioskTestRunner.RunKioskTest(Program.GetProgramArgs(args, start), hardwareService1));
              else if (args[start] == "-findABEDevice")
              {
                ConsoleLogger consoleLogger = new ConsoleLogger(flag);
                Console.WriteLine("ABE device {0}", hardwareService1.HasABEDevice ? (object) "found" : (object) "not found");
              }
              else if (args[start] == "-findABEDeviceThroughScan")
              {
                AbeDeviceDescriptor descriptor = new AbeDeviceDescriptor((IUsbDeviceService) usbDeviceService);
                Console.WriteLine("ABE device {0}", usbDeviceService.FindDevice((IDeviceDescriptor) descriptor).Found ? (object) "found" : (object) "not found");
              }
              else if ("-findKFCDevices" == args[start])
              {
                string path = "KFC.log";
                using (StreamWriter streamWriter = new StreamWriter(runtimeService.CreateBackup(path)))
                {
                  ConsoleLogger consoleLogger = new ConsoleLogger(flag);
                  ITouchscreenDescriptor touchScreen = usbDeviceService.FindTouchScreen(true);
                  if (touchScreen == null)
                    streamWriter.WriteLine("Could not find a touchscreen or match its driver.");
                  else
                    streamWriter.WriteLine("Found touchscreen HWID {0} {1}", (object) touchScreen.ToString(), (object) touchScreen.Friendlyname);
                  IDeviceDescriptor activeCamera = ServiceLocator.Instance.GetService<IUsbDeviceService>().FindActiveCamera(true);
                  if (activeCamera == null)
                    streamWriter.WriteLine("Could not find a camera or match its driver.");
                  else
                    streamWriter.WriteLine("Found camera {0} {1}", (object) activeCamera.ToString(), (object) activeCamera.Friendlyname);
                }
              }
              else if ("-findActiveTouchScreen" == args[start])
              {
                string path = "TSLocator.log";
                runtimeService.CreateBackup(path);
                using (StreamWriter streamWriter = new StreamWriter(path))
                {
                  ConsoleLogger consoleLogger = new ConsoleLogger(flag);
                  ITouchscreenDescriptor touchScreen = usbDeviceService.FindTouchScreen(true);
                  if (touchScreen == null)
                    streamWriter.WriteLine("Could not find a touchscreen or match its driver.");
                  else
                    streamWriter.WriteLine("Found touchscreen HWID {0} {1}", (object) touchScreen.ToString(), (object) touchScreen.Friendlyname);
                }
              }
              else if (args[start].StartsWith("--findActiveCamera"))
              {
                bool optionVal = CommandLineOption.GetOptionVal<bool>(args[start], false);
                string path = "CameraLocator.log";
                runtimeService.CreateBackup(path);
                using (StreamWriter streamWriter = new StreamWriter(path))
                {
                  ConsoleLogger consoleLogger = new ConsoleLogger(flag);
                  IDeviceDescriptor activeCamera = usbDeviceService.FindActiveCamera(true);
                  if (activeCamera == null)
                  {
                    streamWriter.WriteLine("Could not find a camera or match its driver.");
                  }
                  else
                  {
                    streamWriter.WriteLine("## Camera Info ## ");
                    streamWriter.WriteLine("  HWID         : {0}", (object) activeCamera.ToString());
                    streamWriter.WriteLine("  Friendly     : {0}", (object) activeCamera.Friendlyname);
                    streamWriter.WriteLine("  Status       : {0}", (object) activeCamera.GetStatus().ToString());
                    if (optionVal)
                      streamWriter.WriteLine("  RESET status : {0}", activeCamera.ResetDriver() ? (object) "Ok" : (object) "Failed");
                  }
                }
              }
              else if ("-resetControlSystem" == args[start])
              {
                ConsoleLogger consoleLogger = new ConsoleLogger(flag);
                using (ResetControlSystemJob controlSystemJob = new ResetControlSystemJob(hardwareService1))
                {
                  controlSystemJob.Run();
                  LogHelper.Instance.Log("ResetControlSystem job {0} ended with status {1}", (object) controlSystemJob.ID, (object) controlSystemJob.EndStatus.ToString());
                }
              }
              else if ("-resetPDCMCounter" == args[start])
              {
                Console.WriteLine("Resetting PDCM state ...");
                RegistryStore registryStore = new RegistryStore("SOFTWARE\\Redbox\\HAL");
                registryStore.SetSecretValue<int>("PDCMUpdateCount", 0);
                Console.WriteLine("PDCM state now at {0}", (object) registryStore.GetSecretValue<int>("PDCMUpdateCount", -1));
              }
              else if (args[start] == "-interactive")
              {
                string[] programArgs = Program.GetProgramArgs(args, start);
                using (ScriptMode scriptMode = new ScriptMode(hardwareService1))
                  scriptMode.Run(programArgs);
              }
              else if (args[start].StartsWith("-resetDeck"))
              {
                string optionVal = CommandLineOption.GetOptionVal<string>(args[start], string.Empty);
                string path = runtimeService.RuntimePath("resetdecks.txt");
                using (StreamWriter vl = new StreamWriter((Stream) File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read)))
                {
                  if (string.IsNullOrEmpty(optionVal))
                  {
                    vl.WriteLine("Invalid deck list specified.");
                    break;
                  }
                  StringBuilder buffer = new StringBuilder();
                  char[] separator = new char[1]{ ',' };
                  Array.ForEach<string>(optionVal.Split(separator, StringSplitOptions.RemoveEmptyEntries), (Action<string>) (d =>
                  {
                    vl.WriteLine(" Resetting deck {0}", (object) d);
                    buffer.AppendLine(string.Format(" DECK RESET NUMBER=\"{0}\"", (object) d));
                  }));
                  if (hardwareService1 == null)
                  {
                    vl.WriteLine("Cannot connect to service. Please try again.");
                    break;
                  }
                  hardwareService1.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(buffer.ToString()), out HardwareJob _);
                }
              }
              else if (args[start].StartsWith("--analyzeSync"))
                new SyncCheck(CommandLineOption.GetOptionVal<string>(args[start], "EMPTY"), runtimeService).Summarize();
              else if (args[start].StartsWith("--analyzeRunningSync"))
              {
                using (DefaultConsole defaultConsole = new DefaultConsole())
                {
                  string optionVal = CommandLineOption.GetOptionVal<string>(args[start], "EMPTY");
                  if (optionVal == "EMPTY")
                  {
                    defaultConsole.WriteLine("Invalid job id specified.");
                    break;
                  }
                  HardwareJob job1 = (HardwareJob) null;
                  HardwareCommandResult job2 = hardwareService1.GetJob(optionVal, out job1);
                  if (!job2.Success || job1 == null)
                  {
                    job2.Errors.Dump(Console.Out);
                    break;
                  }
                  HardwareJobStatus endStatus;
                  clientHelper.WaitForJob(job1, out endStatus);
                  if (endStatus != HardwareJobStatus.Completed)
                  {
                    defaultConsole.WriteLine("job {0} ended with status {1}", new object[2]
                    {
                      (object) optionVal,
                      (object) endStatus.ToString()
                    });
                    break;
                  }
                  using (InventoryHelper inventoryHelper = new InventoryHelper(hardwareService1))
                    inventoryHelper.CompareSyncResults(job1, false);
                }
              }
              else if (args[start].StartsWith("-analyzeEmptyStuck"))
                EmptyStuckAnalyzer.Instance.AnalyzeByPattern(CommandLineOption.GetOptionVal<bool>(args[start], false));
              else if (args[start].StartsWith("-assemblyVersions"))
              {
                string path = "c:\\Program Files\\Redbox\\HALService\\bin";
                List<string> stringList = new List<string>();
                string[] strArray = new string[2]
                {
                  "*.dll",
                  "*.exe"
                };
                foreach (string searchPattern in strArray)
                  stringList.AddRange((IEnumerable<string>) Directory.GetFiles(path, searchPattern));
                Console.WriteLine("{0} ( {1} files total )", (object) DateTime.Now, (object) stringList.Count);
                stringList.ForEach((Action<string>) (assembly =>
                {
                  try
                  {
                    AssemblyName assemblyName = AssemblyName.GetAssemblyName(assembly);
                    Console.WriteLine("Assembly {0} has version {1}", (object) assembly, (object) assemblyName.Version.ToString());
                  }
                  catch (Exception ex)
                  {
                    Console.WriteLine("Unable to get version for {0}; message = {1}", (object) assembly, (object) ex.Message);
                  }
                }));
              }
              else if (args[start].StartsWith("-dumpDevices"))
              {
                string path = "AllDevices.txt";
                runtimeService.CreateBackup(path);
                using (StreamWriter streamWriter = new StreamWriter(path))
                {
                  Dictionary<string, List<string>> deviceMap = new Dictionary<string, List<string>>();
                  Action<string, string> onDeviceFound = (Action<string, string>) ((clazz, hwid) =>
                  {
                    List<string> stringList;
                    if (!deviceMap.ContainsKey(clazz))
                      deviceMap[clazz] = stringList = new List<string>();
                    else
                      stringList = deviceMap[clazz];
                    stringList.Add(hwid);
                  });
                  usbDeviceService.EnumDevices(onDeviceFound);
                  foreach (string key in deviceMap.Keys)
                  {
                    streamWriter.WriteLine("Devices for class {0}", (object) key);
                    foreach (string str in deviceMap[key])
                      streamWriter.WriteLine("  HWID: {0}", (object) str);
                  }
                  foreach (string key in deviceMap.Keys)
                    deviceMap[key].Clear();
                }
              }
              else if (args[start].StartsWith("-backupConfig"))
              {
                using (ServiceConfigurationHelper configurationHelper = new ServiceConfigurationHelper(hardwareService1))
                  configurationHelper.BackupConfiguration(runtimeService.KioskId);
              }
              else if (args[start].StartsWith("-configureVMZ"))
              {
                using (StreamWriter streamWriter = new StreamWriter((Stream) File.Open(runtimeService.RuntimePath("configure-VMZ.txt"), FileMode.Append, FileAccess.Write, FileShare.Read)))
                  streamWriter.WriteLine("The upgrade process is no longer supported.");
              }
              else if (args[start].StartsWith("-configureDoorSensors"))
              {
                string optionValue = CommandLineOption.GetOptionValue(args[start]);
                if (string.IsNullOrEmpty(optionValue))
                {
                  Console.WriteLine(" The -configureDoorSensors switch requires an argument");
                  Program.PrintSecureHelp();
                }
                HardwareService hardwareService5 = hardwareService1;
                string empty = string.Empty;
                HardwareJobSchedule schedule = new HardwareJobSchedule();
                schedule.Priority = HardwareJobPriority.Highest;
                HardwareJob job;
                HardwareCommandResult hardwareCommandResult = hardwareService5.ScheduleJob("door-sensor-software-override", empty, false, schedule, out job);
                if (!hardwareCommandResult.Success)
                {
                  Console.WriteLine("Failed to schedule configuration job.");
                  hardwareCommandResult.Errors.Dump(Console.Out);
                }
                else
                {
                  job.Push((object) optionValue);
                  job.Pend();
                  HardwareJobStatus endStatus;
                  clientHelper.WaitForJob(job, out endStatus);
                  Console.WriteLine("Change door sensor configuration ended with status {0}", (object) endStatus);
                }
              }
              else if (args[start].Contains("-checkInventory"))
              {
                string optionValue = CommandLineOption.GetOptionValue(args[start], "=");
                string path = "InventoryCompare.txt";
                runtimeService.CreateBackup(path);
                using (StreamWriter log = new StreamWriter(path))
                  InventoryChecker.Instance.CheckInventory(hardwareService1, optionValue, (TextWriter) log);
              }
              else if ("-resetDupCounter" == args[start])
              {
                if (hardwareService1 != null)
                  new CounterHelper().ResetDuplicateCounterValue(hardwareService1);
              }
              else if ("-checkDupCounter" == args[start])
              {
                runtimeService.CreateBackup("DuplicateCounter.log");
                HardwareService service = hardwareService1;
                if (service == null)
                  break;
                int val;
                new CounterHelper().GetDuplicateCounterValue(service, out val);
                if (val > 0)
                {
                  using (StreamWriter streamWriter = new StreamWriter("DuplicateCounter.log"))
                  {
                    streamWriter.WriteLine("The kiosk has duplicates.");
                    streamWriter.Flush();
                  }
                }
              }
              else if (args[start].StartsWith("--preposVend"))
              {
                int optionVal = CommandLineOption.GetOptionVal<int>(args[start], 3);
                using (DefaultConsole defaultConsole = new DefaultConsole())
                {
                  using (PrepositionTest prepositionTest = new PrepositionTest(optionVal, hardwareService1, (IConsole) defaultConsole))
                  {
                    defaultConsole.Write("Type C for continue, otherwise bail:   ");
                    string str = Console.ReadLine();
                    prepositionTest.PreposJob.Signal(str == "C" ? "continue" : "bail");
                    HardwareJobStatus endStatus;
                    clientHelper.WaitForJob(prepositionTest.PreposJob, out endStatus);
                    defaultConsole.WriteLine("Preposition job {0} ended with status {1}", new object[2]
                    {
                      (object) prepositionTest.PreposJob.ID,
                      (object) endStatus.ToString()
                    });
                  }
                }
              }
              else if (args[start].StartsWith("-vendTest"))
              {
                int optionVal = CommandLineOption.GetOptionVal<int>(args[start], 3);
                int? startAt = new int?();
                if (args.Length > 1 && args[start + 1].StartsWith("-next"))
                  startAt = new int?(CommandLineOption.GetOptionVal<int>(args[start + 1], -1));
                HardwareService service = hardwareService1;
                using (VendTest vendTest = new VendTest(optionVal, service))
                {
                  using (DefaultConsole defaultConsole = new DefaultConsole())
                    vendTest.Execute((IConsole) defaultConsole, startAt);
                }
              }
              else if (args[start].StartsWith("--scheduleClean"))
              {
                int? optionVal = CommandLineOption.GetOptionVal<int?>(args[start], new int?());
                using (DefaultConsole defaultConsole = new DefaultConsole())
                {
                  if (optionVal.HasValue)
                  {
                    int? nullable = optionVal;
                    int num = -1;
                    if (!(nullable.GetValueOrDefault() == num & nullable.HasValue))
                    {
                      RedboxTimer redboxTimer = new RedboxTimer("Clean", new ElapsedEventHandler(Program.GenericimerFired));
                      redboxTimer.ScheduleAtNext(optionVal.Value, 0);
                      defaultConsole.WriteLine("Clean timer should fire {0} {1}", new object[2]
                      {
                        (object) redboxTimer.NextFireTime.Value.ToShortDateString(),
                        (object) redboxTimer.NextFireTime.Value.ToShortTimeString()
                      });
                      Program.GenericWaiter.WaitOne();
                    }
                  }
                  HardwareJob job;
                  if (!hardwareService1.CleanZone(out job).Success)
                  {
                    defaultConsole.WriteLine("Failed to schedule clean job.");
                    break;
                  }
                  job.Pend();
                  HardwareJobStatus endStatus;
                  clientHelper.WaitForJob(job, out endStatus);
                  defaultConsole.WriteLine("Clean {0} ended with status {1}", new object[2]
                  {
                    (object) job.ID,
                    (object) endStatus.ToString()
                  });
                }
              }
              else if ("-e".Equals(args[start]))
              {
                using (InventoryHelper inventoryHelper = new InventoryHelper(hardwareService1))
                  inventoryHelper.FixType(SyncType.Empty);
              }
              else if ("-unknown".Equals(args[start]))
              {
                using (InventoryHelper inventoryHelper = new InventoryHelper(hardwareService1))
                  inventoryHelper.FixType(SyncType.Unknown);
              }
              else if (args[start].StartsWith("--fullSync"))
              {
                string optionVal = CommandLineOption.GetOptionVal<string>(args[start], string.Empty);
                SyncOptions options = SyncOptions.None;
                if (string.Empty != optionVal)
                {
                  string[] array = optionVal.Split(';');
                  if (array.Length == 1)
                    options = Enum<SyncOptions>.ParseIgnoringCase(array[0], SyncOptions.None);
                  else
                    Array.ForEach<string>(array, (Action<string>) (option => options |= Enum<SyncOptions>.ParseIgnoringCase(option, SyncOptions.None)));
                }
                ConsoleLogger consoleLogger = new ConsoleLogger((options & SyncOptions.Debug) != 0);
                using (InventoryHelper inventoryHelper = new InventoryHelper(hardwareService1))
                  inventoryHelper.ScheduleFullSync(options);
              }
              else if ("-fullSync".Equals(args[start]))
              {
                using (InventoryHelper inventoryHelper = new InventoryHelper(hardwareService1))
                  inventoryHelper.ScheduleFullSync();
              }
              else if ("-dumpInventory".Equals(args[start]))
              {
                string path = runtimeService.RuntimePath("inventory.xml");
                runtimeService.CreateBackup(path);
                using (InventoryHelper inventoryHelper = new InventoryHelper(hardwareService1))
                  inventoryHelper.DumpInventoryToFile(path);
              }
              else if ("-secureHelp".Equals(args[start]))
                Program.PrintSecureHelp();
              else if ("-help".Equals(args[start]))
                Program.PrintUsageAndExit();
              else if ("-importInventory".Equals(args[start]))
              {
                if (start >= args.Length - 1)
                  Program.PrintUsageAndExit();
                using (InventoryHelper inventoryHelper = new InventoryHelper(hardwareService1))
                {
                  if (inventoryHelper.ImportInventoryFromFile(args[++start]))
                    Console.WriteLine("Inventory import OK.");
                  else
                    Console.WriteLine("Failed to import Inventory!");
                }
              }
              else if ("-inventorysnapshot".Equals(args[start]))
              {
                if (!Directory.Exists("C:\\Program Files\\Redbox\\inventory-snapshots"))
                {
                  try
                  {
                    Directory.CreateDirectory("C:\\Program Files\\Redbox\\inventory-snapshots");
                  }
                  catch (Exception ex)
                  {
                    Console.WriteLine("Unable to create directory: {0}", (object) ex.Message);
                  }
                }
                int num = 1;
                DateTime now = DateTime.Now;
                string empty = string.Empty;
                string path;
                do
                {
                  path = Path.Combine("C:\\Program Files\\Redbox\\inventory-snapshots", string.Format("{0}-{1}-{2}-{3}-{4}.csv", (object) runtimeService.KioskId, (object) now.Month, (object) now.Day, (object) now.Year, (object) num++));
                }
                while (File.Exists(path));
                using (KioskInventory kioskInventory = new KioskInventory(hardwareService1))
                {
                  List<IInventoryLocation> deckInventory = kioskInventory.DeckInventory;
                  using (StreamWriter file = new StreamWriter(path))
                  {
                    file.WriteLine("Deck,Slot,Barcode");
                    deckInventory.ForEach((Action<IInventoryLocation>) (loc => file.WriteLine(string.Format("{0},{1},'{2}'", (object) loc.Location.Deck, (object) loc.Location.Slot, (object) loc.Matrix))));
                    int itemCount = 1;
                    kioskInventory.DumpbinItems.ForEach((Action<IDumpbinItem>) (item => file.WriteLine(string.Format("9,{0},'{1}'", (object) itemCount++, (object) item.Matrix))));
                  }
                }
              }
              else if ("-updateConfiguration".Equals(args[start]))
              {
                using (ServiceConfigurationHelper configurationHelper = new ServiceConfigurationHelper(hardwareService1))
                {
                  int num;
                  configurationHelper.UpdateOption(args[num = start + 1]);
                  break;
                }
              }
              else if ("-version".Equals(args[start]))
              {
                string path = runtimeService.RuntimePath("version.txt");
                Version version = typeof (Program).Assembly.GetName().Version;
                using (StreamWriter streamWriter = new StreamWriter(path))
                  streamWriter.WriteLine((object) version);
                Console.WriteLine("HALUtilities version: {0}", (object) version);
              }
              else if ("-dumpExcluded".Equals(args[start]))
              {
                string path = runtimeService.RuntimePath("excluded.log");
                using (ExcludedLocsJob excludedLocsJob = new ExcludedLocsJob(hardwareService1))
                {
                  excludedLocsJob.Run();
                  using (StreamWriter streamWriter = new StreamWriter(path))
                  {
                    IList<Location> excludedLocations = excludedLocsJob.ExcludedLocations;
                    if (excludedLocations.Count == 0)
                    {
                      streamWriter.WriteLine("There are no excluded locations.");
                    }
                    else
                    {
                      streamWriter.WriteLine("There are {0} locations total.", (object) excludedLocsJob.Results.Count);
                      foreach (Location location in (IEnumerable<Location>) excludedLocations)
                        streamWriter.WriteLine("Excluded location deck = {0} slot = {1}", (object) location.Deck, (object) location.Slot);
                    }
                  }
                }
              }
              else if (args[start].StartsWith("--hwSurvey"))
                new HardwareSurvey(CommandLineOption.GetOptionVal<SurveyModes>(args[start], SurveyModes.Console)).Run(hardwareService1);
              else if (args[start].StartsWith("--3m"))
              {
                ConsoleLogger consoleLogger = new ConsoleLogger(true);
                new _3MTouchscreen(CommandLineOption.GetOptionVal<Operation_3m>(args[start], Operation_3m.None)).Execute(Console.Out);
              }
              else if (args[start].StartsWith("--dumpStats"))
              {
                HardwareCorrectionStatistic optionVal = CommandLineOption.GetOptionVal<HardwareCorrectionStatistic>(args[start], HardwareCorrectionStatistic.None);
                if (optionVal != HardwareCorrectionStatistic.None)
                {
                  using (DumpHardwareStatisticsExecutor statisticsExecutor = new DumpHardwareStatisticsExecutor(hardwareService1, optionVal))
                    statisticsExecutor.Run();
                }
              }
              else if (args[start].StartsWith("--resetStats"))
              {
                HardwareCorrectionStatistic optionVal = CommandLineOption.GetOptionVal<HardwareCorrectionStatistic>(args[start], HardwareCorrectionStatistic.None);
                if (optionVal != HardwareCorrectionStatistic.None)
                {
                  using (ResetHardwareStatisticsExecutor statisticsExecutor = new ResetHardwareStatisticsExecutor(hardwareService1, optionVal))
                    statisticsExecutor.Run();
                }
              }
              else if (args[start] == "-resetTouchscreen")
              {
                ConsoleLogger consoleLogger = new ConsoleLogger(true);
                HardwareService hardwareService6 = hardwareService1;
                HardwareJobSchedule s = new HardwareJobSchedule();
                s.Priority = HardwareJobPriority.High;
                HardwareJob job;
                if (hardwareService6.ResetTouchscreenController(s, out job).Success)
                {
                  HardwareJobStatus endStatus;
                  clientHelper.WaitForJob(job, out endStatus);
                  Console.WriteLine("Reset touchscreen controller ended with status {0}", (object) endStatus.ToString());
                }
                else
                  Console.WriteLine("Unable to schedule reset touchscreen job.");
              }
              else if (args[start].StartsWith("--dumpKFCSessions"))
              {
                OutputModes optionVal = CommandLineOption.GetOptionVal<OutputModes>(args[start], OutputModes.Console);
                using (KioskFunctionCheckHelper functionCheckHelper = new KioskFunctionCheckHelper(hardwareService1))
                  functionCheckHelper.DumpSessions(optionVal);
              }
              else if (args[start] == "-ipcClient")
              {
                IpcClientTester.RunTest(Program.GetProgramArgs(args, start));
                Environment.Exit(0);
              }
              else if (args[start] == "-powerhouseBackup")
                PowerhouseUtilities.Run(PowerhouseOperations.Backup, runtimeService);
              else if (args[start] == "-powerhouseRestore")
                PowerhouseUtilities.Run(PowerhouseOperations.Restore, runtimeService);
              else if (args[start].StartsWith("--hashFile"))
              {
                string optionValue = CommandLineOption.GetOptionValue(args[start], "=");
                if (string.IsNullOrEmpty(optionValue))
                {
                  Console.WriteLine("Invalid file specified - correct the command line.");
                }
                else
                {
                  TripleDesEncryptionService encryptionService = new TripleDesEncryptionService();
                  try
                  {
                    Console.WriteLine("{0} --> {1}", (object) optionValue, (object) encryptionService.HashFile(optionValue));
                  }
                  catch (Exception ex)
                  {
                    Console.WriteLine("Hash failed with an exception:");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                  }
                }
              }
              else if (args[start].StartsWith("-initSys"))
              {
                ConsoleLogger consoleLogger = new ConsoleLogger(true);
                int exitCode = 0;
                using (InitJob initJob = new InitJob(hardwareService1))
                {
                  initJob.Job.EventRaised += (HardwareEvent) ((job, eventTime, eventMessage) => LogHelper.Instance.Log("{0}: {1}", (object) eventTime, (object) eventMessage));
                  initJob.Run();
                  initJob.Results.ForEach((Action<ProgramResult>) (pr => LogHelper.Instance.Log("  Code:{0} Message:{1} ", (object) pr.Code, (object) pr.Message)));
                  LogHelper.Instance.Log("Init job {0} ends with status {1}", (object) initJob.ID, (object) initJob.EndStatus);
                  exitCode = HardwareJobStatus.Completed == initJob.EndStatus ? 0 : -1;
                }
                Environment.Exit(exitCode);
              }
              else if (args[start].StartsWith("-powerCycleRouter"))
              {
                using (PowerCycleRouterExecutor cycleRouterExecutor = new PowerCycleRouterExecutor(hardwareService1))
                {
                  cycleRouterExecutor.Run();
                  Console.WriteLine("Power cycle router job {0} ended with status {1}", (object) cycleRouterExecutor.ID, (object) cycleRouterExecutor.EndStatus);
                }
              }
              else if (args[start] == "-qlmUnload")
              {
                HardwareService hardwareService7 = hardwareService1;
                HardwareJobSchedule schedule = new HardwareJobSchedule();
                schedule.Priority = HardwareJobPriority.High;
                HardwareJob hardwareJob;
                HardwareCommandResult hardwareCommandResult = hardwareService7.Unload(schedule, out hardwareJob);
                if (hardwareCommandResult.Success)
                {
                  hardwareJob.Pend();
                  Console.WriteLine("{0} ( ID = {1} ) started", (object) hardwareJob.ProgramName, (object) hardwareJob.ID);
                }
                else
                {
                  Console.WriteLine("Failed to schedule job");
                  hardwareCommandResult.Errors.Dump(Console.Out);
                }
              }
              else if (args[start] == "-qlmThin")
              {
                List<string> list = new List<string>();
                using (new DisposeableList<string>((IList<string>) list))
                {
                  for (int index = start + 1; index < args.Length; ++index)
                    list.Add(args[index]);
                  HardwareService hardwareService8 = hardwareService1;
                  string[] array = list.ToArray();
                  HardwareJobSchedule schedule = new HardwareJobSchedule();
                  schedule.Priority = HardwareJobPriority.High;
                  HardwareJob hardwareJob;
                  HardwareCommandResult hardwareCommandResult = hardwareService8.Thin(array, schedule, out hardwareJob);
                  if (hardwareCommandResult.Success)
                  {
                    hardwareJob.Pend();
                    Console.WriteLine("{0} ( ID = {1} ) started", (object) hardwareJob.ProgramName, (object) hardwareJob.ID);
                  }
                  else
                  {
                    Console.WriteLine("Failed to schedule job");
                    hardwareCommandResult.Errors.Dump(Console.Out);
                  }
                }
              }
              else if (args[start].StartsWith("--rebuildInventory"))
              {
                bool optionVal = CommandLineOption.GetOptionVal<bool>(args[start], false);
                using (InventoryHelper inventoryHelper = new InventoryHelper(hardwareService1))
                  inventoryHelper.RebuildInventory(optionVal);
              }
              else if (args[start].StartsWith("-restoreInventory"))
              {
                using (InventoryHelper inventoryHelper = new InventoryHelper(hardwareService1))
                  inventoryHelper.RestoreInventory();
              }
              else if (args[start].StartsWith("--decodeResults"))
              {
                string optionVal = CommandLineOption.GetOptionVal<string>(args[start], (string) null);
                if (string.IsNullOrEmpty(optionVal) || !File.Exists(optionVal))
                {
                  Console.WriteLine("please specify a valid file.");
                  break;
                }
                string[] strArray1 = File.ReadAllLines(optionVal);
                List<Program.FraudLocInfo> fraudLocInfoList = new List<Program.FraudLocInfo>();
                Program.FraudLocInfo fraudLocInfo = (Program.FraudLocInfo) null;
                char[] chArray = new char[1]{ ' ' };
                foreach (string str in strArray1)
                {
                  if (str.Contains("GET Deck"))
                  {
                    string[] strArray2 = str.Split(chArray);
                    fraudLocInfo = new Program.FraudLocInfo(Convert.ToInt32(strArray2[6]), Convert.ToInt32(strArray2[9]));
                    fraudLocInfoList.Add(fraudLocInfo);
                  }
                  else if (str.Contains("PUT Deck"))
                    fraudLocInfo = (Program.FraudLocInfo) null;
                  else if (str.Contains("Matrix"))
                  {
                    string[] strArray3 = str.Split(chArray);
                    fraudLocInfo.Dumps.Add(new Program.DecodeInfo(strArray3[6], Convert.ToInt32(strArray3[8])));
                  }
                }
                using (StreamWriter output = new StreamWriter(Path.GetFileNameWithoutExtension(optionVal) + ".csv"))
                {
                  output.WriteLine("deck,slot,matrix,secure_found");
                  fraudLocInfoList.ForEach((Action<Program.FraudLocInfo>) (each => output.WriteLine("{0},{1},{2},{3}", (object) each.Location.Deck, (object) each.Location.Slot, (object) each.Matrix, (object) each.HasSecureRead())));
                  output.WriteLine(" -- decode stats -- ");
                  List<Program.FraudLocInfo> all = fraudLocInfoList.FindAll((Predicate<Program.FraudLocInfo>) (each => each.Matrix != "UNKNOWN"));
                  output.WriteLine("  Items with found barcode: {0}", (object) all.Count);
                  output.WriteLine("  items with a secure read: {0}", (object) fraudLocInfoList.FindAll((Predicate<Program.FraudLocInfo>) (each => each.HasSecureRead())).Count);
                }
              }
              else
              {
                Console.WriteLine("Unrecognized option '{0}'", (object) args[start]);
                Program.PrintUsageAndExit();
              }
            }
          }
        }
      }
    }

    private class FraudLocInfo
    {
      internal readonly List<Program.DecodeInfo> Dumps = new List<Program.DecodeInfo>();
      internal readonly Location Location;

      public override string ToString()
      {
        return string.Format("Deck = {0} Slot = {1}", (object) this.Location.Deck, (object) this.Location.Slot);
      }

      internal bool HasSecureRead()
      {
        return this.Dumps.FindAll((Predicate<Program.DecodeInfo>) (each => each.IsSecure)).Count > 0;
      }

      internal string Matrix
      {
        get
        {
          List<Program.DecodeInfo> all = this.Dumps.FindAll((Predicate<Program.DecodeInfo>) (each => !each.IsSecure));
          if (all.Count == 0)
            return "UNKNOWN";
          return all.Count <= 1 ? all[0].Matrix : throw new InvalidOperationException();
        }
      }

      internal FraudLocInfo(int deck, int slot)
      {
        this.Location = new Location()
        {
          Deck = deck,
          Slot = slot
        };
      }
    }

    private class DecodeInfo
    {
      internal readonly bool IsSecure;
      internal string Matrix;
      internal int Count;

      internal DecodeInfo(string matrix, int count)
      {
        this.Matrix = matrix;
        this.Count = count;
        this.IsSecure = matrix.Equals("YES", StringComparison.CurrentCultureIgnoreCase);
      }
    }

    [Flags]
    private enum CorrectionOptions
    {
      None = 0,
      RestartDevice = 1,
    }

    private delegate IQueryUsbDeviceResult GetResult(IUsbDeviceService usb);
  }
}
