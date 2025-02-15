using System;
using System.Collections.Generic;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace HALUtilities.KioskTest
{
    internal abstract class AbstractKioskTest : IDisposable
    {
        protected readonly KioskConfiguration Configuration;
        protected readonly HardwareService Service;
        private bool Disposed;
        internal bool RewriteDataOnSuccess;
        private bool RunOneDiskQuickTest;
        private int Transfers;
        internal int VendFrequency = 5;

        protected AbstractKioskTest(HardwareService service, KioskConfiguration config)
        {
            Service = service;
            Configuration = config;
        }

        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            OnDispose(true);
        }

        protected abstract JobExecutor GetTransferExecutor(
            TestLocation start,
            TestLocation end,
            bool vend,
            int iteration);

        protected abstract bool CanVisit(TestLocation loc);

        protected abstract int OnPreTest();

        protected abstract int OnCleanup(TestLocation diskSource);

        protected virtual void OnDispose(bool fromDispose)
        {
        }

        protected abstract void OnProcessArgument(string argument);

        protected abstract bool SingleTestNeeded();

        protected bool UpdateGamp()
        {
            if (!RewriteDataOnSuccess)
                return true;
            var hardwareCommandResult =
                Service.ExecuteServiceCommand("SERVICE update-gamp backup-directory: @'c:\\gamp\\PowerhouseBackup'");
            LogHelper.Instance.Log("Update gamp file data: {0}.",
                hardwareCommandResult.Success ? "OK" : (object)"EPIC FAIL");
            return hardwareCommandResult.Success;
        }

        internal void AcceptArgument(string argument)
        {
            if (argument.StartsWith("--vend"))
                VendFrequency = CommandLineOption.GetOptionVal(argument, VendFrequency);
            else if (argument.StartsWith("--rewriteDataOnSuccess"))
                RewriteDataOnSuccess = CommandLineOption.GetOptionVal(argument, RewriteDataOnSuccess);
            else if (argument.StartsWith("--oneDiskQuickTest"))
                RunOneDiskQuickTest = CommandLineOption.GetOptionVal(argument, RunOneDiskQuickTest);
            else
                OnProcessArgument(argument);
        }

        internal bool SingleTestOnly()
        {
            return RunOneDiskQuickTest || SingleTestNeeded();
        }

        internal int Run(TimeSpan? runTime, TestLocation diskSource, MachineInformation config)
        {
            var nullable1 = new DateTime?();
            if (runTime.HasValue)
            {
                nullable1 = DateTime.Now.Add(runTime.Value);
                LogHelper.Instance.Log("Kiosk Test Starting - will run until {0}. Deck details:", nullable1);
            }
            else
            {
                LogHelper.Instance.Log("Kiosk Test Starting - Deck details:");
            }

            LogHelper.Instance.Log("-------------------------------------");
            LogHelper.Instance.Log(" Machine model - {0}", config.Configuration);
            config.DecksConfiguration.ForEach(dc =>
                LogHelper.Instance.Log("  Deck {0} - Number of Slots {1}", dc.Number, dc.SlotCount));
            LogHelper.Instance.Log("-------------------------------------");
            var testLocationList = PopulateLocations(diskSource, config);
            var start = diskSource;
            while (testLocationList.Count > 0)
            {
                var vend = VendFrequency > 0 && Transfers % VendFrequency == 0;
                var index = new Random(DateTime.Now.Millisecond).Next(0, testLocationList.Count - 1);
                var end = testLocationList[index];
                testLocationList.RemoveAt(index);
                LogHelper.Instance.Log("## Move Disc From Deck {0}, Slot {1} to Deck {2}, Slot {3} - Vend {4} ##",
                    start.Deck, start.Slot, end.Deck, end.Slot, vend);
                if (!RunTransfer(start, end, vend))
                {
                    LogHelper.Instance.Log("Transfer job failed.");
                    Environment.Exit(-1);
                }

                start = end;
                if (nullable1.HasValue)
                {
                    var instance = LogHelper.Instance;
                    var objArray = new object[1];
                    var nullable2 = nullable1;
                    var now1 = DateTime.Now;
                    objArray[0] = nullable2.HasValue ? nullable2.GetValueOrDefault() - now1 : new TimeSpan?();
                    instance.Log("## Time Remaining {0} ##", objArray);
                    var now2 = DateTime.Now;
                    nullable2 = nullable1;
                    if ((nullable2.HasValue ? now2 > nullable2.GetValueOrDefault() ? 1 : 0 : 0) != 0)
                        break;
                }
            }

            RunTransfer(start, diskSource, false);
            LogHelper.Instance.Log("## TestComplete - {0} total transfers ##", Transfers);
            OnCleanup(diskSource);
            UpdateGamp();
            LogHelper.Instance.Log("## Kiosk Test Completed ##");
            return 0;
        }

        internal int RunSingleTests(TestLocation start, MachineInformation c)
        {
            if (!RunOneDiskQuickTest)
                return OnPreTest();
            return !RunQuickTest(start, c) || !UpdateGamp() ? -3 : 0;
        }

        private bool RunTransfer(TestLocation start, TestLocation end, bool vend)
        {
            using (var transferExecutor = GetTransferExecutor(start, end, vend, Transfers))
            {
                transferExecutor.Run();
                if (HardwareJobStatus.Completed == transferExecutor.EndStatus)
                {
                    ++Transfers;
                    return true;
                }

                LogHelper.Instance.Log("**ERROR** Kiosk test job {0} failed with status {1}", transferExecutor.ID,
                    transferExecutor.EndStatus.ToString());
                return false;
            }
        }

        private bool RunQuickTest(TestLocation source, MachineInformation config)
        {
            LogHelper.Instance.Log(" ** scheduling one disk quick deck test ** ");
            var count = config.DecksConfiguration.Count;
            if (config.Configuration != KioskConfiguration.R717)
                count = config.DecksConfiguration.Count - 1;
            using (var oneDiskQuickTest = new OneDiskQuickTest(Service, source.Deck, source.Slot, count))
            {
                oneDiskQuickTest.Run();
                LogHelper.Instance.Log(" OneDiskQuickTest ended with status {0}", oneDiskQuickTest.EndStatus);
                return oneDiskQuickTest.EndStatus == HardwareJobStatus.Completed;
            }
        }

        private List<TestLocation> PopulateLocations(TestLocation source, MachineInformation config)
        {
            var locations = new List<TestLocation>();
            config.DecksConfiguration.ForEach(dc =>
            {
                if (dc.IsQlm)
                    return;
                for (var slot = 1; slot <= dc.SlotCount; ++slot)
                    if (dc.Number != source.Deck || slot != source.Slot)
                    {
                        var loc = new TestLocation(dc.Number, slot);
                        if (CanVisit(loc))
                            locations.Add(loc);
                    }
            });
            return locations;
        }
    }
}