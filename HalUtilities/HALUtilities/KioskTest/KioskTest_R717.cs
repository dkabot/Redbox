using System.Collections.Generic;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace HALUtilities.KioskTest
{
    internal sealed class KioskTest_R717 : AbstractKioskTest
    {
        private bool ExcludeSlotsAroundDumpBin;
        private bool RunDiskAroundBinTest;

        internal KioskTest_R717(HardwareService s)
            : base(s, KioskConfiguration.R717)
        {
        }

        protected override JobExecutor GetTransferExecutor(
            TestLocation start,
            TestLocation end,
            bool vend,
            int notUsed)
        {
            return new KioskTestExecutor(Service, start, end, vend);
        }

        protected override bool CanVisit(TestLocation tl)
        {
            return tl.Deck != 8 || (tl.Slot != 82 && tl.Slot != 83 && tl.Slot != 84 &&
                                    (!ExcludeSlotsAroundDumpBin || (tl.Slot != 81 && tl.Slot != 85)));
        }

        protected override int OnPreTest()
        {
            if (!RunDiskAroundBinTest)
                return 0;
            LogHelper.Instance.Log(" ** scheduling disk adjacent to bin test ** ");
            using (var adjacentBinTest = new AdjacentBinTest(Service))
            {
                adjacentBinTest.Run();
                LogHelper.Instance.Log(" AdjacentBinTest job ended with status {0}", adjacentBinTest.EndStatus);
                return adjacentBinTest.EndStatus != HardwareJobStatus.Completed ? -3 : 0;
            }
        }

        protected override int OnCleanup(TestLocation diskSource)
        {
            LogHelper.Instance.Log("## Schedule load bin job ##");
            return !ScheduleAndBlockLoadBinJob(new List<Location>
            {
                new Location
                {
                    Deck = diskSource.Deck,
                    Slot = diskSource.Slot
                }
            })
                ? -9
                : 0;
        }

        protected override void OnProcessArgument(string argument)
        {
            if (argument.StartsWith("--excludeAroundDump"))
            {
                ExcludeSlotsAroundDumpBin = CommandLineOption.GetOptionVal(argument, ExcludeSlotsAroundDumpBin);
            }
            else
            {
                if (!argument.StartsWith("--testDisksAroundBin"))
                    return;
                RunDiskAroundBinTest = CommandLineOption.GetOptionVal(argument, RunDiskAroundBinTest);
            }
        }

        protected override bool SingleTestNeeded()
        {
            return RunDiskAroundBinTest;
        }

        private bool ScheduleAndBlockLoadBinJob(List<Location> locs)
        {
            using (var clientHelper = new ClientHelper(Service))
            {
                var job = (HardwareJob)null;
                try
                {
                    var service = Service;
                    var locs1 = locs;
                    var schedule = new HardwareJobSchedule();
                    schedule.Priority = HardwareJobPriority.High;
                    ref var local = ref job;
                    if (!service.LoadBin(locs1, schedule, out local).Success)
                    {
                        LogHelper.Instance.Log("**ERROR** Failed to create bin job");
                        return false;
                    }

                    HardwareJobStatus endStatus;
                    if (clientHelper.WaitForJob(job, out endStatus))
                        return endStatus == HardwareJobStatus.Completed;
                    LogHelper.Instance.Log("**ERROR** Wait for load bin job failed");
                    return false;
                }
                finally
                {
                    job?.Trash();
                }
            }
        }
    }
}