using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using System.Collections.Generic;

namespace HALUtilities.KioskTest
{
  internal sealed class KioskTest_R717 : AbstractKioskTest
  {
    private bool RunDiskAroundBinTest;
    private bool ExcludeSlotsAroundDumpBin;

    protected override JobExecutor GetTransferExecutor(
      TestLocation start,
      TestLocation end,
      bool vend,
      int notUsed)
    {
      return (JobExecutor) new KioskTestExecutor(this.Service, start, end, vend);
    }

    protected override bool CanVisit(TestLocation tl)
    {
      return tl.Deck != 8 || tl.Slot != 82 && tl.Slot != 83 && tl.Slot != 84 && (!this.ExcludeSlotsAroundDumpBin || tl.Slot != 81 && tl.Slot != 85);
    }

    protected override int OnPreTest()
    {
      if (!this.RunDiskAroundBinTest)
        return 0;
      LogHelper.Instance.Log(" ** scheduling disk adjacent to bin test ** ");
      using (AdjacentBinTest adjacentBinTest = new AdjacentBinTest(this.Service))
      {
        adjacentBinTest.Run();
        LogHelper.Instance.Log(" AdjacentBinTest job ended with status {0}", (object) adjacentBinTest.EndStatus);
        return adjacentBinTest.EndStatus != HardwareJobStatus.Completed ? -3 : 0;
      }
    }

    protected override int OnCleanup(TestLocation diskSource)
    {
      LogHelper.Instance.Log("## Schedule load bin job ##");
      return !this.ScheduleAndBlockLoadBinJob(new List<Location>()
      {
        new Location()
        {
          Deck = diskSource.Deck,
          Slot = diskSource.Slot
        }
      }) ? -9 : 0;
    }

    protected override void OnProcessArgument(string argument)
    {
      if (argument.StartsWith("--excludeAroundDump"))
      {
        this.ExcludeSlotsAroundDumpBin = CommandLineOption.GetOptionVal<bool>(argument, this.ExcludeSlotsAroundDumpBin);
      }
      else
      {
        if (!argument.StartsWith("--testDisksAroundBin"))
          return;
        this.RunDiskAroundBinTest = CommandLineOption.GetOptionVal<bool>(argument, this.RunDiskAroundBinTest);
      }
    }

    protected override bool SingleTestNeeded() => this.RunDiskAroundBinTest;

    internal KioskTest_R717(HardwareService s)
      : base(s, KioskConfiguration.R717)
    {
    }

    private bool ScheduleAndBlockLoadBinJob(List<Location> locs)
    {
      using (ClientHelper clientHelper = new ClientHelper(this.Service))
      {
        HardwareJob job = (HardwareJob) null;
        try
        {
          HardwareService service = this.Service;
          List<Location> locs1 = locs;
          HardwareJobSchedule schedule = new HardwareJobSchedule();
          schedule.Priority = HardwareJobPriority.High;
          ref HardwareJob local = ref job;
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
