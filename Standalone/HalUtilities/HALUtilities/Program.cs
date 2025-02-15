using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Timers;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Component.Model.Services;
using Redbox.HAL.Core;
using Redbox.IPC.Framework;

namespace HALUtilities
{
    internal sealed class Program
    {
        private const string CSVDirectory = "C:\\Program Files\\Redbox\\inventory-snapshots";
        private const int ServiceTimeout = 30000;
        private const string DuplicateCounterFile = "DuplicateCounter.log";
        private static readonly ManualResetEvent GenericWaiter = new ManualResetEvent(false);

        private static void PrintUsageAndExit()
        {
            PrintUsageAndExit(new string[0]);
        }

        private static void PrintUsageAndExit(string[] secureOptions)
        {
            Console.WriteLine("HALUtilities.exe version {0}", typeof(Program).Assembly.GetName().Version);
            Console.WriteLine("## Options ## ");
            Console.WriteLine("  Where: ");
            Console.WriteLine("  \t\t--cameraGeneration: returns which camera generation is in use.");
            Console.WriteLine("  \t\t--decoderType: returns which barcode decoder is in use.");
            Console.WriteLine(
                "  \t\t--findTripplite:<true|false>  Probes for any tripplite devices attached to the computer.");
            Console.WriteLine("  \t\t-findABEDevice  Probes for an attached ABE device via HAL service.");
            Console.WriteLine("  \t\t-findABEDeviceThroughScan  Probes for an attached ABE device via USB query.");
            Console.WriteLine(
                "  \t\t-findKFCDevices  Probes for active touchscreen & camera attached to computer. Logs to KFC.log.");
            Console.WriteLine(
                "  \t\t-findActiveTouchScreen  Probes for active touchscreen attached to computer. Logs to TSLocator.log.");
            Console.WriteLine(
                "  \t\t-findActiveCamera  Probes for active camera attached to computer. Logs to CameraLocator.log.");
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
            Console.WriteLine(
                "  \t\t-inventorysnapshot   if specified, generates an inventory snapshot to a csv file in \"C:\\Program Files\\Redbox\\inventory-snapshot\"");
            Console.WriteLine("  \t\t-importInventory takes inventory from an XML file and copies it into inventory");
            Console.WriteLine(
                "  \t\t-updateConfiguration CONFIG:OPTION:NEWVALUE:VALIDATE Update OPTION in configuration CONFIG to value NEWVALUE.");
            Console.WriteLine("  \t\t-restartQR Tries to restart the QR device.");
            Console.WriteLine("  \t\t-stopQR Tries to stop the QR device (does not remove it from configuration).");
            Console.WriteLine(
                "  \t\t-checkQRStatus If the QR device is not configured and active, will generate a 'qr-status.log' file with any errors.");
            Console.WriteLine("  \t\t-version Report the tool's verison.");
            Console.WriteLine("  \t\t-checkDupCounter Generate a log file if the machine has counters.");
            Console.WriteLine("  \t\t-resetDupCounter Reset the duplicate barcode counter.");
            Console.WriteLine("  \t\t-updateReboot={time} Updates the reboot time and cycles the service.");
            Console.WriteLine("  \t\t-checkInventory={path} Checks HAL inventory against file specified.");
            Console.WriteLine("  \t\t-assemblyVersions Dumps the HAL assembly versions to the console.");
            Console.WriteLine(
                "  \t\t-bmpconverter Specifies to run as bitmap converter; passes remaining arguments to converter.");
            Array.ForEach(secureOptions, secMsg => Console.WriteLine(secMsg));
            Console.WriteLine("  \t\t-help print this message and exit.");
            Environment.Exit(1);
        }

        private static void PrintSecureHelp()
        {
            PrintUsageAndExit(new string[10]
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
            GenericWaiter.Set();
        }

        private static string[] GetProgramArgs(string[] allArgs, int start)
        {
            var destinationArray = new string[allArgs.Length - (start + 1)];
            Array.Copy(allArgs, start + 1, destinationArray, 0, destinationArray.Length);
            return destinationArray;
        }

        private static void OnSearchAndFix(
            GetResult getResult,
            IUsbDeviceService usb,
            CorrectionOptions options)
        {
            var consoleLogger = new ConsoleLogger(true);
            var queryUsbDeviceResult = getResult(usb);
            if (!queryUsbDeviceResult.Found)
            {
                LogHelper.Instance.Log("Unable to find device.");
                Environment.Exit(256);
            }

            var exitCode = 0;
            LogHelper.Instance.Log("Found device {0}", queryUsbDeviceResult);
            if (queryUsbDeviceResult.Running)
            {
                LogHelper.Instance.Log("The device appears to be working.");
            }
            else if ((CorrectionOptions.RestartDevice & options) != CorrectionOptions.None)
            {
                if (queryUsbDeviceResult.IsNotStarted)
                    LogHelper.Instance.Log(" Device not started; RESET returned {0}",
                        queryUsbDeviceResult.Descriptor.ResetDriver());
                else if (queryUsbDeviceResult.IsDisabled)
                    LogHelper.Instance.Log(" Device not enabled; change to enable returned {0}",
                        usb.ChangeByHWID(queryUsbDeviceResult.Descriptor, DeviceState.Enable));
                var deviceStatus = usb.FindDeviceStatus(queryUsbDeviceResult.Descriptor);
                LogHelper.Instance.Log(" Device status AFTER fix = {0}", deviceStatus);
                if ((deviceStatus & DeviceStatus.Found) != DeviceStatus.None &&
                    (deviceStatus & DeviceStatus.Enabled) != DeviceStatus.None)
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
                PrintUsageAndExit();
            var runtimeService = (IRuntimeService)new RuntimeService();
            ServiceLocator.Instance.AddService<IRuntimeService>(runtimeService);
            ServiceLocator.Instance.AddService<IDeviceSetupClassFactory>(new DeviceSetupClassFactory());
            var usbDeviceService = new UsbDeviceService(true);
            ServiceLocator.Instance.AddService<IUsbDeviceService>(usbDeviceService);
            var hardwareService1 = new HardwareService(IPCProtocol.Parse(Constants.HALIPCStrings.TcpServer));
            var clientHelper = new ClientHelper(hardwareService1);
            var flag = true;
            for (var start = 0; start < args.Length; ++start)
                if (args[start].StartsWith("--protocol"))
                {
                    var optionValue = CommandLineOption.GetOptionValue(args[start], "=");
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
                {
                    Console.WriteLine(hardwareService1.CurrentCameraGeneration.ToString());
                }
                else if (args[start] == "--decoderType")
                {
                    Console.WriteLine(hardwareService1.BarcodeDecoder.ToString());
                }
                else
                {
                    if (args[start] == "-scheduleTestSync")
                    {
                        var locationList = new List<Location>
                        {
                            new Location { Deck = 1, Slot = 1 },
                            new Location { Deck = 1, Slot = 2 }
                        };
                        var hardwareService2 = hardwareService1;
                        var locations = locationList;
                        var schedule = new HardwareJobSchedule();
                        schedule.Priority = HardwareJobPriority.Low;
                        HardwareJob hardwareJob;
                        hardwareService2.HardSync(locations, schedule, out hardwareJob);
                        break;
                    }

                    if (args[start].StartsWith("--statusCCR"))
                    {
                        using (var resetCcrExecutor = new TestAndResetCCRExecutor(hardwareService1))
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
                        var options = CorrectionOptions.None;
                        if (CommandLineOption.GetOptionVal(args[start], false))
                            options |= CorrectionOptions.RestartDevice;
                        OnSearchAndFix(_usb => _usb.FindCamera(), usbDeviceService, options);
                    }
                    else if (args[start].StartsWith("--removeStats"))
                    {
                        var optionVal = CommandLineOption.GetOptionVal(args[start], HardwareCorrectionStatistic.None);
                        if (optionVal != HardwareCorrectionStatistic.None)
                        {
                            var hardwareService3 = hardwareService1;
                            var stat = (int)optionVal;
                            var schedule = new HardwareJobSchedule();
                            schedule.Priority = HardwareJobPriority.High;
                            HardwareJob job;
                            if (!hardwareService3
                                    .RemoveHardwareCorrectionStats((HardwareCorrectionStatistic)stat, schedule, out job)
                                    .Success)
                            {
                                Console.WriteLine("Failed to schedule remove stats job.");
                            }
                            else
                            {
                                HardwareJobStatus endStatus;
                                clientHelper.WaitForJob(job, out endStatus);
                                Console.WriteLine("Remove all stats job {0} ended with status {1}", job.ID, endStatus);
                            }
                        }
                    }
                    else if (args[start].StartsWith("-removeAllStats"))
                    {
                        var hardwareService4 = hardwareService1;
                        var schedule = new HardwareJobSchedule();
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
                            Console.WriteLine("Remove all stats job {0} ended with status {1}", job.ID, endStatus);
                        }
                    }
                    else if (args[start].StartsWith("--consoleDebug"))
                    {
                        flag = CommandLineOption.GetOptionVal(args[start], flag);
                    }
                    else
                    {
                        if (args[start] == "-customerReturn")
                        {
                            var thread = new Thread(CustomerReturn.RunExecutor);
                            thread.Start(hardwareService1);
                            thread.Join();
                            break;
                        }

                        if (args[start] == "-returnUnknown")
                            using (var returnUnknownExecutor = new ReturnUnknownExecutor(hardwareService1))
                            {
                                returnUnknownExecutor.Run();
                                Console.WriteLine("{0} ( ID = {1} ) ended with status {2}",
                                    returnUnknownExecutor.Job.ProgramName, returnUnknownExecutor.ID,
                                    returnUnknownExecutor.EndStatus);
                                Console.WriteLine(" -- program results --");
                                returnUnknownExecutor.Results.ForEach(each =>
                                    Console.WriteLine("  Code: {0}  Message {1}", each.Code, each.Message));
                                break;
                            }

                        if (args[start].StartsWith("-randomSync"))
                        {
                            var defVal1 = 1;
                            var num1 = 1;
                            var num2 = 1;
                            var defVal2 = false;
                            for (var index = start + 1; index < args.Length; ++index)
                                if (args[index].StartsWith("--iterations"))
                                    defVal1 = CommandLineOption.GetOptionVal(args[index], defVal1);
                                else if (args[index].StartsWith("--vendTime"))
                                    num1 = CommandLineOption.GetOptionVal(args[index], num1);
                                else if (args[index].StartsWith("--vendFrequency"))
                                    num2 = CommandLineOption.GetOptionVal(args[index], num2);
                                else if (args[index].StartsWith("--oneDisk"))
                                    defVal2 = CommandLineOption.GetOptionVal(args[index], defVal2);
                            var exitCode = 0;
                            for (var index = 1; index <= defVal1; ++index)
                            {
                                Console.WriteLine("Iteration {0} of {1} total.", index, defVal1);
                                JobExecutor jobExecutor;
                                if (defVal2)
                                {
                                    jobExecutor = new OneDiskRandomSyncExecutor(hardwareService1, num1, num2);
                                    jobExecutor.AddSink((j, eventTime, eventMessage) =>
                                        Console.WriteLine("{0}: {1}", eventTime, eventMessage));
                                }
                                else
                                {
                                    jobExecutor = new RandomSyncExecutor(hardwareService1, num1, num2);
                                }

                                using (jobExecutor)
                                {
                                    jobExecutor.Run();
                                    Console.WriteLine("Job {0} ends with status {1}", jobExecutor.ID,
                                        jobExecutor.EndStatus.ToString());
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
                                LoopyExecutor.RunExecutor(CommandLineOption.GetOptionVal(args[start], 10),
                                    hardwareService1);
                                break;
                            }

                            if (args[start].StartsWith("--msgServer"))
                            {
                                AbstractPipeSever.RunServer(args[start], PipeServers.Message, flag);
                            }
                            else if (args[start].StartsWith("--pipeServer"))
                            {
                                AbstractPipeSever.RunServer(args[start], PipeServers.Basic, flag);
                            }
                            else if (args[start] == "-rdbxPipeServer")
                            {
                                ServiceHostRunner.Run(Constants.HALIPCStrings.PipeServer, flag);
                            }
                            else if (args[start] == "-rdbxTcpServer")
                            {
                                ServiceHostRunner.Run(Constants.HALIPCStrings.TcpServer, flag);
                            }
                            else if ("-kioskTest" == args[start])
                            {
                                Environment.Exit(KioskTestRunner.RunKioskTest(GetProgramArgs(args, start),
                                    hardwareService1));
                            }
                            else if (args[start] == "-findABEDevice")
                            {
                                var consoleLogger = new ConsoleLogger(flag);
                                Console.WriteLine("ABE device {0}",
                                    hardwareService1.HasABEDevice ? "found" : (object)"not found");
                            }
                            else if (args[start] == "-findABEDeviceThroughScan")
                            {
                                var descriptor = new AbeDeviceDescriptor(usbDeviceService);
                                Console.WriteLine("ABE device {0}",
                                    usbDeviceService.FindDevice(descriptor).Found ? "found" : (object)"not found");
                            }
                            else if ("-findKFCDevices" == args[start])
                            {
                                var path = "KFC.log";
                                using (var streamWriter = new StreamWriter(runtimeService.CreateBackup(path)))
                                {
                                    var consoleLogger = new ConsoleLogger(flag);
                                    var touchScreen = usbDeviceService.FindTouchScreen(true);
                                    if (touchScreen == null)
                                        streamWriter.WriteLine("Could not find a touchscreen or match its driver.");
                                    else
                                        streamWriter.WriteLine("Found touchscreen HWID {0} {1}", touchScreen.ToString(),
                                            touchScreen.Friendlyname);
                                    var activeCamera = ServiceLocator.Instance.GetService<IUsbDeviceService>()
                                        .FindActiveCamera(true);
                                    if (activeCamera == null)
                                        streamWriter.WriteLine("Could not find a camera or match its driver.");
                                    else
                                        streamWriter.WriteLine("Found camera {0} {1}", activeCamera.ToString(),
                                            activeCamera.Friendlyname);
                                }
                            }
                            else if ("-findActiveTouchScreen" == args[start])
                            {
                                var path = "TSLocator.log";
                                runtimeService.CreateBackup(path);
                                using (var streamWriter = new StreamWriter(path))
                                {
                                    var consoleLogger = new ConsoleLogger(flag);
                                    var touchScreen = usbDeviceService.FindTouchScreen(true);
                                    if (touchScreen == null)
                                        streamWriter.WriteLine("Could not find a touchscreen or match its driver.");
                                    else
                                        streamWriter.WriteLine("Found touchscreen HWID {0} {1}", touchScreen.ToString(),
                                            touchScreen.Friendlyname);
                                }
                            }
                            else if (args[start].StartsWith("--findActiveCamera"))
                            {
                                var optionVal = CommandLineOption.GetOptionVal(args[start], false);
                                var path = "CameraLocator.log";
                                runtimeService.CreateBackup(path);
                                using (var streamWriter = new StreamWriter(path))
                                {
                                    var consoleLogger = new ConsoleLogger(flag);
                                    var activeCamera = usbDeviceService.FindActiveCamera(true);
                                    if (activeCamera == null)
                                    {
                                        streamWriter.WriteLine("Could not find a camera or match its driver.");
                                    }
                                    else
                                    {
                                        streamWriter.WriteLine("## Camera Info ## ");
                                        streamWriter.WriteLine("  HWID         : {0}", activeCamera.ToString());
                                        streamWriter.WriteLine("  Friendly     : {0}", activeCamera.Friendlyname);
                                        streamWriter.WriteLine("  Status       : {0}",
                                            activeCamera.GetStatus().ToString());
                                        if (optionVal)
                                            streamWriter.WriteLine("  RESET status : {0}",
                                                activeCamera.ResetDriver() ? "Ok" : (object)"Failed");
                                    }
                                }
                            }
                            else if ("-resetControlSystem" == args[start])
                            {
                                var consoleLogger = new ConsoleLogger(flag);
                                using (var controlSystemJob = new ResetControlSystemJob(hardwareService1))
                                {
                                    controlSystemJob.Run();
                                    LogHelper.Instance.Log("ResetControlSystem job {0} ended with status {1}",
                                        controlSystemJob.ID, controlSystemJob.EndStatus.ToString());
                                }
                            }
                            else if ("-resetPDCMCounter" == args[start])
                            {
                                Console.WriteLine("Resetting PDCM state ...");
                                var registryStore = new RegistryStore("SOFTWARE\\Redbox\\HAL");
                                registryStore.SetSecretValue("PDCMUpdateCount", 0);
                                Console.WriteLine("PDCM state now at {0}",
                                    registryStore.GetSecretValue("PDCMUpdateCount", -1));
                            }
                            else if (args[start] == "-interactive")
                            {
                                var programArgs = GetProgramArgs(args, start);
                                using (var scriptMode = new ScriptMode(hardwareService1))
                                {
                                    scriptMode.Run(programArgs);
                                }
                            }
                            else if (args[start].StartsWith("-resetDeck"))
                            {
                                var optionVal = CommandLineOption.GetOptionVal(args[start], string.Empty);
                                var path = runtimeService.RuntimePath("resetdecks.txt");
                                using (var vl = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write,
                                           FileShare.Read)))
                                {
                                    if (string.IsNullOrEmpty(optionVal))
                                    {
                                        vl.WriteLine("Invalid deck list specified.");
                                        break;
                                    }

                                    var buffer = new StringBuilder();
                                    var separator = new char[1] { ',' };
                                    Array.ForEach(optionVal.Split(separator, StringSplitOptions.RemoveEmptyEntries),
                                        d =>
                                        {
                                            vl.WriteLine(" Resetting deck {0}", d);
                                            buffer.AppendLine(string.Format(" DECK RESET NUMBER=\"{0}\"", d));
                                        });
                                    if (hardwareService1 == null)
                                    {
                                        vl.WriteLine("Cannot connect to service. Please try again.");
                                        break;
                                    }

                                    hardwareService1.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(buffer.ToString()),
                                        out var _);
                                }
                            }
                            else if (args[start].StartsWith("--analyzeSync"))
                            {
                                new SyncCheck(CommandLineOption.GetOptionVal(args[start], "EMPTY"), runtimeService)
                                    .Summarize();
                            }
                            else if (args[start].StartsWith("--analyzeRunningSync"))
                            {
                                using (var defaultConsole = new DefaultConsole())
                                {
                                    var optionVal = CommandLineOption.GetOptionVal(args[start], "EMPTY");
                                    if (optionVal == "EMPTY")
                                    {
                                        defaultConsole.WriteLine("Invalid job id specified.");
                                        break;
                                    }

                                    var job1 = (HardwareJob)null;
                                    var job2 = hardwareService1.GetJob(optionVal, out job1);
                                    if (!job2.Success || job1 == null)
                                    {
                                        job2.Errors.Dump(Console.Out);
                                        break;
                                    }

                                    HardwareJobStatus endStatus;
                                    clientHelper.WaitForJob(job1, out endStatus);
                                    if (endStatus != HardwareJobStatus.Completed)
                                    {
                                        defaultConsole.WriteLine("job {0} ended with status {1}", optionVal,
                                            endStatus.ToString());
                                        break;
                                    }

                                    using (var inventoryHelper = new InventoryHelper(hardwareService1))
                                    {
                                        inventoryHelper.CompareSyncResults(job1, false);
                                    }
                                }
                            }
                            else if (args[start].StartsWith("-analyzeEmptyStuck"))
                            {
                                EmptyStuckAnalyzer.Instance.AnalyzeByPattern(
                                    CommandLineOption.GetOptionVal(args[start], false));
                            }
                            else if (args[start].StartsWith("-assemblyVersions"))
                            {
                                var path = "c:\\Program Files\\Redbox\\HALService\\bin";
                                var stringList = new List<string>();
                                var strArray = new string[2]
                                {
                                    "*.dll",
                                    "*.exe"
                                };
                                foreach (var searchPattern in strArray)
                                    stringList.AddRange(Directory.GetFiles(path, searchPattern));
                                Console.WriteLine("{0} ( {1} files total )", DateTime.Now, stringList.Count);
                                stringList.ForEach(assembly =>
                                {
                                    try
                                    {
                                        var assemblyName = AssemblyName.GetAssemblyName(assembly);
                                        Console.WriteLine("Assembly {0} has version {1}", assembly,
                                            assemblyName.Version.ToString());
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Unable to get version for {0}; message = {1}", assembly,
                                            ex.Message);
                                    }
                                });
                            }
                            else if (args[start].StartsWith("-dumpDevices"))
                            {
                                var path = "AllDevices.txt";
                                runtimeService.CreateBackup(path);
                                using (var streamWriter = new StreamWriter(path))
                                {
                                    var deviceMap = new Dictionary<string, List<string>>();
                                    var onDeviceFound = (Action<string, string>)((clazz, hwid) =>
                                    {
                                        List<string> stringList;
                                        if (!deviceMap.ContainsKey(clazz))
                                            deviceMap[clazz] = stringList = new List<string>();
                                        else
                                            stringList = deviceMap[clazz];
                                        stringList.Add(hwid);
                                    });
                                    usbDeviceService.EnumDevices(onDeviceFound);
                                    foreach (var key in deviceMap.Keys)
                                    {
                                        streamWriter.WriteLine("Devices for class {0}", key);
                                        foreach (var str in deviceMap[key])
                                            streamWriter.WriteLine("  HWID: {0}", str);
                                    }

                                    foreach (var key in deviceMap.Keys)
                                        deviceMap[key].Clear();
                                }
                            }
                            else if (args[start].StartsWith("-backupConfig"))
                            {
                                using (var configurationHelper = new ServiceConfigurationHelper(hardwareService1))
                                {
                                    configurationHelper.BackupConfiguration(runtimeService.KioskId);
                                }
                            }
                            else if (args[start].StartsWith("-configureVMZ"))
                            {
                                using (var streamWriter = new StreamWriter(File.Open(
                                           runtimeService.RuntimePath("configure-VMZ.txt"), FileMode.Append,
                                           FileAccess.Write, FileShare.Read)))
                                {
                                    streamWriter.WriteLine("The upgrade process is no longer supported.");
                                }
                            }
                            else if (args[start].StartsWith("-configureDoorSensors"))
                            {
                                var optionValue = CommandLineOption.GetOptionValue(args[start]);
                                if (string.IsNullOrEmpty(optionValue))
                                {
                                    Console.WriteLine(" The -configureDoorSensors switch requires an argument");
                                    PrintSecureHelp();
                                }

                                var hardwareService5 = hardwareService1;
                                var empty = string.Empty;
                                var schedule = new HardwareJobSchedule();
                                schedule.Priority = HardwareJobPriority.Highest;
                                HardwareJob job;
                                var hardwareCommandResult =
                                    hardwareService5.ScheduleJob("door-sensor-software-override", empty, false,
                                        schedule, out job);
                                if (!hardwareCommandResult.Success)
                                {
                                    Console.WriteLine("Failed to schedule configuration job.");
                                    hardwareCommandResult.Errors.Dump(Console.Out);
                                }
                                else
                                {
                                    job.Push(optionValue);
                                    job.Pend();
                                    HardwareJobStatus endStatus;
                                    clientHelper.WaitForJob(job, out endStatus);
                                    Console.WriteLine("Change door sensor configuration ended with status {0}",
                                        endStatus);
                                }
                            }
                            else if (args[start].Contains("-checkInventory"))
                            {
                                var optionValue = CommandLineOption.GetOptionValue(args[start], "=");
                                var path = "InventoryCompare.txt";
                                runtimeService.CreateBackup(path);
                                using (var log = new StreamWriter(path))
                                {
                                    InventoryChecker.Instance.CheckInventory(hardwareService1, optionValue, log);
                                }
                            }
                            else if ("-resetDupCounter" == args[start])
                            {
                                if (hardwareService1 != null)
                                    new CounterHelper().ResetDuplicateCounterValue(hardwareService1);
                            }
                            else if ("-checkDupCounter" == args[start])
                            {
                                runtimeService.CreateBackup("DuplicateCounter.log");
                                var service = hardwareService1;
                                if (service == null)
                                    break;
                                int val;
                                new CounterHelper().GetDuplicateCounterValue(service, out val);
                                if (val > 0)
                                    using (var streamWriter = new StreamWriter("DuplicateCounter.log"))
                                    {
                                        streamWriter.WriteLine("The kiosk has duplicates.");
                                        streamWriter.Flush();
                                    }
                            }
                            else if (args[start].StartsWith("--preposVend"))
                            {
                                var optionVal = CommandLineOption.GetOptionVal(args[start], 3);
                                using (var defaultConsole = new DefaultConsole())
                                {
                                    using (var prepositionTest =
                                           new PrepositionTest(optionVal, hardwareService1, defaultConsole))
                                    {
                                        defaultConsole.Write("Type C for continue, otherwise bail:   ");
                                        var str = Console.ReadLine();
                                        prepositionTest.PreposJob.Signal(str == "C" ? "continue" : "bail");
                                        HardwareJobStatus endStatus;
                                        clientHelper.WaitForJob(prepositionTest.PreposJob, out endStatus);
                                        defaultConsole.WriteLine("Preposition job {0} ended with status {1}",
                                            prepositionTest.PreposJob.ID, endStatus.ToString());
                                    }
                                }
                            }
                            else if (args[start].StartsWith("-vendTest"))
                            {
                                var optionVal = CommandLineOption.GetOptionVal(args[start], 3);
                                var startAt = new int?();
                                if (args.Length > 1 && args[start + 1].StartsWith("-next"))
                                    startAt = CommandLineOption.GetOptionVal(args[start + 1], -1);
                                var service = hardwareService1;
                                using (var vendTest = new VendTest(optionVal, service))
                                {
                                    using (var defaultConsole = new DefaultConsole())
                                    {
                                        vendTest.Execute(defaultConsole, startAt);
                                    }
                                }
                            }
                            else if (args[start].StartsWith("--scheduleClean"))
                            {
                                var optionVal = CommandLineOption.GetOptionVal(args[start], new int?());
                                using (var defaultConsole = new DefaultConsole())
                                {
                                    if (optionVal.HasValue)
                                    {
                                        var nullable = optionVal;
                                        var num = -1;
                                        if (!((nullable.GetValueOrDefault() == num) & nullable.HasValue))
                                        {
                                            var redboxTimer = new RedboxTimer("Clean", GenericimerFired);
                                            redboxTimer.ScheduleAtNext(optionVal.Value, 0);
                                            defaultConsole.WriteLine("Clean timer should fire {0} {1}",
                                                redboxTimer.NextFireTime.Value.ToShortDateString(),
                                                redboxTimer.NextFireTime.Value.ToShortTimeString());
                                            GenericWaiter.WaitOne();
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
                                    defaultConsole.WriteLine("Clean {0} ended with status {1}", job.ID,
                                        endStatus.ToString());
                                }
                            }
                            else if ("-e".Equals(args[start]))
                            {
                                using (var inventoryHelper = new InventoryHelper(hardwareService1))
                                {
                                    inventoryHelper.FixType(SyncType.Empty);
                                }
                            }
                            else if ("-unknown".Equals(args[start]))
                            {
                                using (var inventoryHelper = new InventoryHelper(hardwareService1))
                                {
                                    inventoryHelper.FixType(SyncType.Unknown);
                                }
                            }
                            else if (args[start].StartsWith("--fullSync"))
                            {
                                var optionVal = CommandLineOption.GetOptionVal(args[start], string.Empty);
                                var options = SyncOptions.None;
                                if (string.Empty != optionVal)
                                {
                                    var array = optionVal.Split(';');
                                    if (array.Length == 1)
                                        options = Enum<SyncOptions>.ParseIgnoringCase(array[0], SyncOptions.None);
                                    else
                                        Array.ForEach(array,
                                            option => options |=
                                                Enum<SyncOptions>.ParseIgnoringCase(option, SyncOptions.None));
                                }

                                var consoleLogger = new ConsoleLogger((options & SyncOptions.Debug) != 0);
                                using (var inventoryHelper = new InventoryHelper(hardwareService1))
                                {
                                    inventoryHelper.ScheduleFullSync(options);
                                }
                            }
                            else if ("-fullSync".Equals(args[start]))
                            {
                                using (var inventoryHelper = new InventoryHelper(hardwareService1))
                                {
                                    inventoryHelper.ScheduleFullSync();
                                }
                            }
                            else if ("-dumpInventory".Equals(args[start]))
                            {
                                var path = runtimeService.RuntimePath("inventory.xml");
                                runtimeService.CreateBackup(path);
                                using (var inventoryHelper = new InventoryHelper(hardwareService1))
                                {
                                    inventoryHelper.DumpInventoryToFile(path);
                                }
                            }
                            else if ("-secureHelp".Equals(args[start]))
                            {
                                PrintSecureHelp();
                            }
                            else if ("-help".Equals(args[start]))
                            {
                                PrintUsageAndExit();
                            }
                            else if ("-importInventory".Equals(args[start]))
                            {
                                if (start >= args.Length - 1)
                                    PrintUsageAndExit();
                                using (var inventoryHelper = new InventoryHelper(hardwareService1))
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
                                    try
                                    {
                                        Directory.CreateDirectory("C:\\Program Files\\Redbox\\inventory-snapshots");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Unable to create directory: {0}", ex.Message);
                                    }

                                var num = 1;
                                var now = DateTime.Now;
                                var empty = string.Empty;
                                string path;
                                do
                                {
                                    path = Path.Combine("C:\\Program Files\\Redbox\\inventory-snapshots",
                                        string.Format("{0}-{1}-{2}-{3}-{4}.csv", runtimeService.KioskId, now.Month,
                                            now.Day, now.Year, num++));
                                } while (File.Exists(path));

                                using (var kioskInventory = new KioskInventory(hardwareService1))
                                {
                                    var deckInventory = kioskInventory.DeckInventory;
                                    using (var file = new StreamWriter(path))
                                    {
                                        file.WriteLine("Deck,Slot,Barcode");
                                        deckInventory.ForEach(loc => file.WriteLine("{0},{1},'{2}'", loc.Location.Deck,
                                            loc.Location.Slot, loc.Matrix));
                                        var itemCount = 1;
                                        kioskInventory.DumpbinItems.ForEach(item =>
                                            file.WriteLine("9,{0},'{1}'", itemCount++, item.Matrix));
                                    }
                                }
                            }
                            else if ("-updateConfiguration".Equals(args[start]))
                            {
                                using (var configurationHelper = new ServiceConfigurationHelper(hardwareService1))
                                {
                                    int num;
                                    configurationHelper.UpdateOption(args[num = start + 1]);
                                    break;
                                }
                            }
                            else if ("-version".Equals(args[start]))
                            {
                                var path = runtimeService.RuntimePath("version.txt");
                                var version = typeof(Program).Assembly.GetName().Version;
                                using (var streamWriter = new StreamWriter(path))
                                {
                                    streamWriter.WriteLine(version);
                                }

                                Console.WriteLine("HALUtilities version: {0}", version);
                            }
                            else if ("-dumpExcluded".Equals(args[start]))
                            {
                                var path = runtimeService.RuntimePath("excluded.log");
                                using (var excludedLocsJob = new ExcludedLocsJob(hardwareService1))
                                {
                                    excludedLocsJob.Run();
                                    using (var streamWriter = new StreamWriter(path))
                                    {
                                        var excludedLocations = excludedLocsJob.ExcludedLocations;
                                        if (excludedLocations.Count == 0)
                                        {
                                            streamWriter.WriteLine("There are no excluded locations.");
                                        }
                                        else
                                        {
                                            streamWriter.WriteLine("There are {0} locations total.",
                                                excludedLocsJob.Results.Count);
                                            foreach (var location in excludedLocations)
                                                streamWriter.WriteLine("Excluded location deck = {0} slot = {1}",
                                                    location.Deck, location.Slot);
                                        }
                                    }
                                }
                            }
                            else if (args[start].StartsWith("--hwSurvey"))
                            {
                                new HardwareSurvey(CommandLineOption.GetOptionVal(args[start], SurveyModes.Console))
                                    .Run(hardwareService1);
                            }
                            else if (args[start].StartsWith("--3m"))
                            {
                                var consoleLogger = new ConsoleLogger(true);
                                new _3MTouchscreen(CommandLineOption.GetOptionVal(args[start], Operation_3m.None))
                                    .Execute(Console.Out);
                            }
                            else if (args[start].StartsWith("--dumpStats"))
                            {
                                var optionVal =
                                    CommandLineOption.GetOptionVal(args[start], HardwareCorrectionStatistic.None);
                                if (optionVal != HardwareCorrectionStatistic.None)
                                    using (var statisticsExecutor =
                                           new DumpHardwareStatisticsExecutor(hardwareService1, optionVal))
                                    {
                                        statisticsExecutor.Run();
                                    }
                            }
                            else if (args[start].StartsWith("--resetStats"))
                            {
                                var optionVal =
                                    CommandLineOption.GetOptionVal(args[start], HardwareCorrectionStatistic.None);
                                if (optionVal != HardwareCorrectionStatistic.None)
                                    using (var statisticsExecutor =
                                           new ResetHardwareStatisticsExecutor(hardwareService1, optionVal))
                                    {
                                        statisticsExecutor.Run();
                                    }
                            }
                            else if (args[start] == "-resetTouchscreen")
                            {
                                var consoleLogger = new ConsoleLogger(true);
                                var hardwareService6 = hardwareService1;
                                var s = new HardwareJobSchedule();
                                s.Priority = HardwareJobPriority.High;
                                HardwareJob job;
                                if (hardwareService6.ResetTouchscreenController(s, out job).Success)
                                {
                                    HardwareJobStatus endStatus;
                                    clientHelper.WaitForJob(job, out endStatus);
                                    Console.WriteLine("Reset touchscreen controller ended with status {0}",
                                        endStatus.ToString());
                                }
                                else
                                {
                                    Console.WriteLine("Unable to schedule reset touchscreen job.");
                                }
                            }
                            else if (args[start].StartsWith("--dumpKFCSessions"))
                            {
                                var optionVal = CommandLineOption.GetOptionVal(args[start], OutputModes.Console);
                                using (var functionCheckHelper = new KioskFunctionCheckHelper(hardwareService1))
                                {
                                    functionCheckHelper.DumpSessions(optionVal);
                                }
                            }
                            else if (args[start] == "-ipcClient")
                            {
                                IpcClientTester.RunTest(GetProgramArgs(args, start));
                                Environment.Exit(0);
                            }
                            else if (args[start] == "-powerhouseBackup")
                            {
                                PowerhouseUtilities.Run(PowerhouseOperations.Backup, runtimeService);
                            }
                            else if (args[start] == "-powerhouseRestore")
                            {
                                PowerhouseUtilities.Run(PowerhouseOperations.Restore, runtimeService);
                            }
                            else if (args[start].StartsWith("--hashFile"))
                            {
                                var optionValue = CommandLineOption.GetOptionValue(args[start], "=");
                                if (string.IsNullOrEmpty(optionValue))
                                {
                                    Console.WriteLine("Invalid file specified - correct the command line.");
                                }
                                else
                                {
                                    var encryptionService = new TripleDesEncryptionService();
                                    try
                                    {
                                        Console.WriteLine("{0} --> {1}", optionValue,
                                            encryptionService.HashFile(optionValue));
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
                                var consoleLogger = new ConsoleLogger(true);
                                var exitCode = 0;
                                using (var initJob = new InitJob(hardwareService1))
                                {
                                    initJob.Job.EventRaised += (job, eventTime, eventMessage) =>
                                        LogHelper.Instance.Log("{0}: {1}", eventTime, eventMessage);
                                    initJob.Run();
                                    initJob.Results.ForEach(pr =>
                                        LogHelper.Instance.Log("  Code:{0} Message:{1} ", pr.Code, pr.Message));
                                    LogHelper.Instance.Log("Init job {0} ends with status {1}", initJob.ID,
                                        initJob.EndStatus);
                                    exitCode = HardwareJobStatus.Completed == initJob.EndStatus ? 0 : -1;
                                }

                                Environment.Exit(exitCode);
                            }
                            else if (args[start].StartsWith("-powerCycleRouter"))
                            {
                                using (var cycleRouterExecutor = new PowerCycleRouterExecutor(hardwareService1))
                                {
                                    cycleRouterExecutor.Run();
                                    Console.WriteLine("Power cycle router job {0} ended with status {1}",
                                        cycleRouterExecutor.ID, cycleRouterExecutor.EndStatus);
                                }
                            }
                            else if (args[start] == "-qlmUnload")
                            {
                                var hardwareService7 = hardwareService1;
                                var schedule = new HardwareJobSchedule();
                                schedule.Priority = HardwareJobPriority.High;
                                HardwareJob hardwareJob;
                                var hardwareCommandResult = hardwareService7.Unload(schedule, out hardwareJob);
                                if (hardwareCommandResult.Success)
                                {
                                    hardwareJob.Pend();
                                    Console.WriteLine("{0} ( ID = {1} ) started", hardwareJob.ProgramName,
                                        hardwareJob.ID);
                                }
                                else
                                {
                                    Console.WriteLine("Failed to schedule job");
                                    hardwareCommandResult.Errors.Dump(Console.Out);
                                }
                            }
                            else if (args[start] == "-qlmThin")
                            {
                                var list = new List<string>();
                                using (new DisposeableList<string>(list))
                                {
                                    for (var index = start + 1; index < args.Length; ++index)
                                        list.Add(args[index]);
                                    var hardwareService8 = hardwareService1;
                                    var array = list.ToArray();
                                    var schedule = new HardwareJobSchedule();
                                    schedule.Priority = HardwareJobPriority.High;
                                    HardwareJob hardwareJob;
                                    var hardwareCommandResult = hardwareService8.Thin(array, schedule, out hardwareJob);
                                    if (hardwareCommandResult.Success)
                                    {
                                        hardwareJob.Pend();
                                        Console.WriteLine("{0} ( ID = {1} ) started", hardwareJob.ProgramName,
                                            hardwareJob.ID);
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
                                var optionVal = CommandLineOption.GetOptionVal(args[start], false);
                                using (var inventoryHelper = new InventoryHelper(hardwareService1))
                                {
                                    inventoryHelper.RebuildInventory(optionVal);
                                }
                            }
                            else if (args[start].StartsWith("-restoreInventory"))
                            {
                                using (var inventoryHelper = new InventoryHelper(hardwareService1))
                                {
                                    inventoryHelper.RestoreInventory();
                                }
                            }
                            else if (args[start].StartsWith("--decodeResults"))
                            {
                                var optionVal = CommandLineOption.GetOptionVal(args[start], (string)null);
                                if (string.IsNullOrEmpty(optionVal) || !File.Exists(optionVal))
                                {
                                    Console.WriteLine("please specify a valid file.");
                                    break;
                                }

                                var strArray1 = File.ReadAllLines(optionVal);
                                var fraudLocInfoList = new List<FraudLocInfo>();
                                var fraudLocInfo = (FraudLocInfo)null;
                                var chArray = new char[1] { ' ' };
                                foreach (var str in strArray1)
                                    if (str.Contains("GET Deck"))
                                    {
                                        var strArray2 = str.Split(chArray);
                                        fraudLocInfo = new FraudLocInfo(Convert.ToInt32(strArray2[6]),
                                            Convert.ToInt32(strArray2[9]));
                                        fraudLocInfoList.Add(fraudLocInfo);
                                    }
                                    else if (str.Contains("PUT Deck"))
                                    {
                                        fraudLocInfo = null;
                                    }
                                    else if (str.Contains("Matrix"))
                                    {
                                        var strArray3 = str.Split(chArray);
                                        fraudLocInfo.Dumps.Add(new DecodeInfo(strArray3[6],
                                            Convert.ToInt32(strArray3[8])));
                                    }

                                using (var output =
                                       new StreamWriter(Path.GetFileNameWithoutExtension(optionVal) + ".csv"))
                                {
                                    output.WriteLine("deck,slot,matrix,secure_found");
                                    fraudLocInfoList.ForEach(each => output.WriteLine("{0},{1},{2},{3}",
                                        each.Location.Deck, each.Location.Slot, each.Matrix, each.HasSecureRead()));
                                    output.WriteLine(" -- decode stats -- ");
                                    var all = fraudLocInfoList.FindAll(each => each.Matrix != "UNKNOWN");
                                    output.WriteLine("  Items with found barcode: {0}", all.Count);
                                    output.WriteLine("  items with a secure read: {0}",
                                        fraudLocInfoList.FindAll(each => each.HasSecureRead()).Count);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Unrecognized option '{0}'", args[start]);
                                PrintUsageAndExit();
                            }
                        }
                    }
                }
        }

        private class FraudLocInfo
        {
            internal readonly List<DecodeInfo> Dumps = new List<DecodeInfo>();
            internal readonly Location Location;

            internal FraudLocInfo(int deck, int slot)
            {
                Location = new Location
                {
                    Deck = deck,
                    Slot = slot
                };
            }

            internal string Matrix
            {
                get
                {
                    var all = Dumps.FindAll(each => !each.IsSecure);
                    if (all.Count == 0)
                        return "UNKNOWN";
                    return all.Count <= 1 ? all[0].Matrix : throw new InvalidOperationException();
                }
            }

            public override string ToString()
            {
                return string.Format("Deck = {0} Slot = {1}", Location.Deck, Location.Slot);
            }

            internal bool HasSecureRead()
            {
                return Dumps.FindAll(each => each.IsSecure).Count > 0;
            }
        }

        private class DecodeInfo
        {
            internal readonly bool IsSecure;
            internal readonly string Matrix;
            internal int Count;

            internal DecodeInfo(string matrix, int count)
            {
                Matrix = matrix;
                Count = count;
                IsSecure = matrix.Equals("YES", StringComparison.CurrentCultureIgnoreCase);
            }
        }

        [Flags]
        private enum CorrectionOptions
        {
            None = 0,
            RestartDevice = 1
        }

        private delegate IQueryUsbDeviceResult GetResult(IUsbDeviceService usb);
    }
}