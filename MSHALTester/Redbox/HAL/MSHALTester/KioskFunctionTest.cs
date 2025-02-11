using System;
using System.Collections.Generic;
using System.Threading;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.MSHALTester;

internal sealed class KioskFunctionTest : JobExecutor, IKioskFunctionCheckData
{
    private readonly OutputBox Box;
    private readonly bool HasCortex;
    private TestState CameraDriverStatus;
    private TestState InitStatus;
    private TestState SnapDecodeStatus;
    private TestState TSDriverStatus;
    private TestState VendDoorStatus;
    private TestState VerticalSlotTestStatus;

    internal KioskFunctionTest(
        HardwareService service,
        OutputBox box,
        bool hasCortex,
        string uname)
        : base(service)
    {
        Box = box;
        HasCortex = hasCortex;
        Timestamp = DateTime.Now;
        UserIdentifier = uname;
        TSDriverStatus = CameraDriverStatus = SnapDecodeStatus = TestState.NotStarted;
        VendDoorStatus = InitStatus = VerticalSlotTestStatus = TestState.NotStarted;
    }

    protected override string JobName => "kiosk-function-test-data";

    public string VerticalSlotTestResult => VerticalSlotTestStatus.ToString();

    public string InitTestResult => InitStatus.ToString();

    public string VendDoorTestResult => VendDoorStatus.ToString();

    public string TrackTestResult => InitStatus.ToString();

    public string SnapDecodeTestResult => SnapDecodeStatus.ToString();

    public string TouchscreenDriverTestResult => TSDriverStatus.ToString();

    public string CameraDriverTestResult => CameraDriverStatus.ToString();

    public DateTime Timestamp { get; }

    public string UserIdentifier { get; private set; }

    protected override void SetupJob()
    {
        Job.Push(UserIdentifier);
        Job.Push(Timestamp.ToString());
        Job.Push(TouchscreenDriverTestResult);
        Job.Push(CameraDriverTestResult);
        Job.Push(SnapDecodeTestResult);
        Job.Push(TrackTestResult);
        Job.Push(VendDoorTestResult);
        Job.Push(InitTestResult);
        Job.Push(VerticalSlotTestResult);
    }

    internal void SendData(string username)
    {
        UserIdentifier = username;
        Run();
        Box.Write("Report queued.");
    }

    internal TestState TestDrivers()
    {
        var testState1 = MatchTS();
        var testState2 = MatchCamera();
        return TestState.Success != testState1 || TestState.Success != testState2
            ? TestState.Failure
            : TestState.Success;
    }

    internal TestState TestCameraSnap(Location loc)
    {
        SnapDecodeStatus = TestState.Failure;
        var locationList = new List<Location>();
        locationList.Add(loc);
        var service = Service;
        var locations = locationList;
        var schedule = new HardwareJobSchedule();
        schedule.Priority = HardwareJobPriority.Highest;
        HardwareJob job;
        if (!service.HardSync(locations, "KFC Test Sync", schedule, out job).Success)
            Box.Write("Unable to communicate with HAL.");
        else
            using (var clientHelper = new ClientHelper(Service))
            {
                HardwareJobStatus endStatus;
                clientHelper.WaitForJob(job, out endStatus);
                if (HardwareJobStatus.Completed == endStatus)
                {
                    IDictionary<string, string> symbols;
                    if (job.GetSymbols(out symbols).Success)
                        foreach (var key in symbols.Keys)
                            if (key.Equals("MSTESTER-SYMBOL-FORMATTED-DETAIL-MSG"))
                            {
                                var str1 = symbols[key];
                                LogHelper.Instance.Log("[TestCameraSnap] val = {0}", str1);
                                var num1 = HasCortex ? 1 : 3;
                                var num2 = str1.IndexOf(':');
                                var strArray = str1.Substring(num2 + 1).Split(new char[1]
                                {
                                    ' '
                                }, StringSplitOptions.RemoveEmptyEntries);
                                var str2 = strArray[0];
                                var int32 = Convert.ToInt32(strArray[2].Substring(1, 1));
                                var timeSpan = TimeSpan.Parse(strArray[5]);
                                SnapDecodeStatus = int32 < num1 || timeSpan.Seconds > 0
                                    ? TestState.Failure
                                    : TestState.Success;
                                break;
                            }
                }
                else
                {
                    Box.Write("Decode test failed.");
                }
            }

        return SnapDecodeStatus;
    }

    internal TestState TestVendDoor()
    {
        VendDoorStatus = TestState.Failure;
        var service = ServiceLocator.Instance.GetService<IControlSystem>();
        if (service.VendDoorRent().Success)
        {
            Thread.Sleep(500);
            VendDoorStatus = service.VendDoorClose().Success ? TestState.Success : TestState.Failure;
        }

        return VendDoorStatus;
    }

    internal TestState RunVertialSlotTest(int slot)
    {
        using (var verticalSync = new VerticalSync(Service, slot))
        {
            verticalSync.Run();
            if (HardwareJobStatus.Completed == verticalSync.EndStatus)
            {
                Box.Write("The job completed successfully.");
                VerticalSlotTestStatus = TestState.Success;
            }
            else
            {
                foreach (var result in verticalSync.Results)
                    Box.Write(string.Format("Failure at Deck {0} Slot {1} MSG: {2}", result.Deck, result.Slot,
                        result.Message));
                VerticalSlotTestStatus = TestState.Failure;
            }
        }

        return VerticalSlotTestStatus;
    }

    internal TestState RunInit()
    {
        using (var initJob = new InitJob(Service))
        {
            initJob.Run();
            if (HardwareJobStatus.Completed == initJob.EndStatus)
            {
                Box.Write("Init succeeded.");
                InitStatus = TestState.Success;
            }
            else
            {
                InitStatus = TestState.Failure;
                if (initJob.Errors.Count > 0)
                {
                    foreach (var error in initJob.Errors)
                        Box.Write(error.Details);
                    Box.Write("Init didn't succeed; errors follow:");
                }
            }

            return InitStatus;
        }
    }

    internal bool GetUnknownStats()
    {
        var inventoryStatsJob = new InventoryStatsJob(Service);
        inventoryStatsJob.Run();
        if (inventoryStatsJob.EndStatus != HardwareJobStatus.Completed)
            return false;
        inventoryStatsJob.Results.ForEach(result =>
        {
            if (result.Code == "TotalEmptyCount")
            {
                Box.Write(string.Format("  EMPTY slots: {0}", result.Message));
            }
            else
            {
                if (!(result.Code == "UnknownCount"))
                    return;
                Box.Write(string.Format("  UNKNOWN slots: {0}", result.Message));
            }
        });
        Box.Write("Inventory Stats: ");
        return true;
    }

    private TestState MatchCamera()
    {
        var activeCamera = ServiceLocator.Instance.GetService<IUsbDeviceService>().FindActiveCamera(true);
        if (activeCamera != null)
        {
            CameraDriverStatus = TestState.Success;
            Box.Write("Matched camera {0}", activeCamera.Friendlyname);
        }
        else
        {
            Box.Write("Failed to locate camera or match camera driver.");
            CameraDriverStatus = TestState.Failure;
        }

        return CameraDriverStatus;
    }

    private TestState MatchTS()
    {
        var touchScreen =
            (IDeviceDescriptor)ServiceLocator.Instance.GetService<IUsbDeviceService>().FindTouchScreen(true);
        if (touchScreen != null)
        {
            TSDriverStatus = TestState.Success;
            Box.Write("Found Touchscreen {0}", touchScreen.Friendlyname);
        }
        else
        {
            TSDriverStatus = TestState.Failure;
            Box.Write("Failed to locate touchscreen or match driver.");
        }

        return TSDriverStatus;
    }
}