using System;
using System.Text;
using System.Threading;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Common.GUI.Functions
{
    internal sealed class KioskFunctionTest : JobExecutor, IKioskFunctionCheckData
    {
        private readonly OutputBox Box;
        private readonly byte[] CameraSnapImmediate;
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
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("CLEAR");
            stringBuilder.AppendLine(" RINGLIGHT ON");
            stringBuilder.AppendLine(" CAMERA SNAP");
            stringBuilder.AppendLine(" RINGLIGHT OFF");
            CameraSnapImmediate = Encoding.ASCII.GetBytes(stringBuilder.ToString());
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

        protected override void SetupJob(HardwareJob job)
        {
            job.Push(UserIdentifier);
            job.Push(Timestamp.ToString());
            job.Push(TouchscreenDriverTestResult);
            job.Push(CameraDriverTestResult);
            job.Push(SnapDecodeTestResult);
            job.Push(TrackTestResult);
            job.Push(VendDoorTestResult);
            job.Push(InitTestResult);
            job.Push(VerticalSlotTestResult);
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

        internal TestState TestCameraSnap(int deck, int slot)
        {
            SnapDecodeStatus = TestState.Failure;
            if (!CompositeFunctions.GetItem(deck, slot, Box, Service))
            {
                SnapDecodeStatus = TestState.Failure;
            }
            else
            {
                var num1 = HasCortex ? 1 : 3;
                var readBarcodeResult = ReadBarcodeResult.ReadBarcodeOfDiskInPicker(Service);
                Box.Write(string.Format("Found {0} barcodes ({1}), time={2} {3}", readBarcodeResult.Count,
                    readBarcodeResult.Barcode, readBarcodeResult.ScanTime,
                    readBarcodeResult.IsDuplicate ? "DUPLICATE" : (object)string.Empty));
                var num2 = int.Parse(readBarcodeResult.ScanTime.Split('.')[0]);
                SnapDecodeStatus = int.Parse(readBarcodeResult.Count) < num1 || num2 > 0
                    ? TestState.Failure
                    : TestState.Success;
                CompositeFunctions.PutItem(Service, deck, slot, Box);
            }

            return SnapDecodeStatus;
        }

        internal TestState TestVendDoor()
        {
            if ("SUCCESS" != ExecuteInstructionWithResult("VENDDOOR RENT"))
            {
                VendDoorStatus = TestState.Failure;
            }
            else
            {
                Thread.Sleep(500);
                VendDoorStatus = "SUCCESS" == ExecuteInstructionWithResult("VENDDOOR CLOSE")
                    ? TestState.Success
                    : TestState.Failure;
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
            foreach (var result in inventoryStatsJob.Results)
                if (result.Code == "TotalEmptyCount")
                    Box.Write(string.Format("  EMPTY slots: {0}", result.Message));
                else if (result.Code == "UnknownCount")
                    Box.Write(string.Format("  UNKNOWN slots: {0}", result.Message));
            Box.Write("Inventory Stats: ");
            return true;
        }

        private string ExecuteInstructionWithResult(string inst)
        {
            var job = CommonFunctions.ExecuteInstruction(Service, inst, 120000);
            if (job != null)
            {
                var topOfStack = job.GetTopOfStack();
                Box.Write(inst + " - " + topOfStack);
                return topOfStack;
            }

            Box.Write("Command failed.");
            return string.Empty;
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
}