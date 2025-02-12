using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using System;
using System.Collections.Generic;

namespace HALUtilities.KioskTest
{
  internal abstract class AbstractKioskTest : IDisposable
  {
    protected readonly KioskConfiguration Configuration;
    protected readonly HardwareService Service;
    internal bool RewriteDataOnSuccess;
    internal int VendFrequency = 5;
    private int Transfers;
    private bool RunOneDiskQuickTest;
    private bool Disposed;

    public void Dispose()
    {
      if (this.Disposed)
        return;
      this.Disposed = true;
      this.OnDispose(true);
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
      if (!this.RewriteDataOnSuccess)
        return true;
      HardwareCommandResult hardwareCommandResult = this.Service.ExecuteServiceCommand("SERVICE update-gamp backup-directory: @'c:\\gamp\\PowerhouseBackup'");
      LogHelper.Instance.Log("Update gamp file data: {0}.", hardwareCommandResult.Success ? (object) "OK" : (object) "EPIC FAIL");
      return hardwareCommandResult.Success;
    }

    protected AbstractKioskTest(HardwareService service, KioskConfiguration config)
    {
      this.Service = service;
      this.Configuration = config;
    }

    internal void AcceptArgument(string argument)
    {
      if (argument.StartsWith("--vend"))
        this.VendFrequency = CommandLineOption.GetOptionVal<int>(argument, this.VendFrequency);
      else if (argument.StartsWith("--rewriteDataOnSuccess"))
        this.RewriteDataOnSuccess = CommandLineOption.GetOptionVal<bool>(argument, this.RewriteDataOnSuccess);
      else if (argument.StartsWith("--oneDiskQuickTest"))
        this.RunOneDiskQuickTest = CommandLineOption.GetOptionVal<bool>(argument, this.RunOneDiskQuickTest);
      else
        this.OnProcessArgument(argument);
    }

    internal bool SingleTestOnly() => this.RunOneDiskQuickTest || this.SingleTestNeeded();

    internal int Run(TimeSpan? runTime, TestLocation diskSource, MachineInformation config)
    {
      DateTime? nullable1 = new DateTime?();
      if (runTime.HasValue)
      {
        nullable1 = new DateTime?(DateTime.Now.Add(runTime.Value));
        LogHelper.Instance.Log("Kiosk Test Starting - will run until {0}. Deck details:", (object) nullable1);
      }
      else
        LogHelper.Instance.Log("Kiosk Test Starting - Deck details:");
      LogHelper.Instance.Log("-------------------------------------");
      LogHelper.Instance.Log(" Machine model - {0}", (object) config.Configuration);
      config.DecksConfiguration.ForEach((Action<IDeckConfig>) (dc => LogHelper.Instance.Log("  Deck {0} - Number of Slots {1}", (object) dc.Number, (object) dc.SlotCount)));
      LogHelper.Instance.Log("-------------------------------------");
      List<TestLocation> testLocationList = this.PopulateLocations(diskSource, config);
      TestLocation start = diskSource;
      while (testLocationList.Count > 0)
      {
        bool vend = this.VendFrequency > 0 && this.Transfers % this.VendFrequency == 0;
        int index = new Random(DateTime.Now.Millisecond).Next(0, testLocationList.Count - 1);
        TestLocation end = testLocationList[index];
        testLocationList.RemoveAt(index);
        LogHelper.Instance.Log("## Move Disc From Deck {0}, Slot {1} to Deck {2}, Slot {3} - Vend {4} ##", (object) start.Deck, (object) start.Slot, (object) end.Deck, (object) end.Slot, (object) vend);
        if (!this.RunTransfer(start, end, vend))
        {
          LogHelper.Instance.Log("Transfer job failed.");
          Environment.Exit(-1);
        }
        start = end;
        if (nullable1.HasValue)
        {
          LogHelper instance = LogHelper.Instance;
          object[] objArray = new object[1];
          DateTime? nullable2 = nullable1;
          DateTime now1 = DateTime.Now;
          objArray[0] = (object) (nullable2.HasValue ? new TimeSpan?(nullable2.GetValueOrDefault() - now1) : new TimeSpan?());
          instance.Log("## Time Remaining {0} ##", objArray);
          DateTime now2 = DateTime.Now;
          nullable2 = nullable1;
          if ((nullable2.HasValue ? (now2 > nullable2.GetValueOrDefault() ? 1 : 0) : 0) != 0)
            break;
        }
      }
      this.RunTransfer(start, diskSource, false);
      LogHelper.Instance.Log("## TestComplete - {0} total transfers ##", (object) this.Transfers);
      this.OnCleanup(diskSource);
      this.UpdateGamp();
      LogHelper.Instance.Log("## Kiosk Test Completed ##");
      return 0;
    }

    internal int RunSingleTests(TestLocation start, MachineInformation c)
    {
      if (!this.RunOneDiskQuickTest)
        return this.OnPreTest();
      return !this.RunQuickTest(start, c) || !this.UpdateGamp() ? -3 : 0;
    }

    private bool RunTransfer(TestLocation start, TestLocation end, bool vend)
    {
      using (JobExecutor transferExecutor = this.GetTransferExecutor(start, end, vend, this.Transfers))
      {
        transferExecutor.Run();
        if (HardwareJobStatus.Completed == transferExecutor.EndStatus)
        {
          ++this.Transfers;
          return true;
        }
        LogHelper.Instance.Log("**ERROR** Kiosk test job {0} failed with status {1}", (object) transferExecutor.ID, (object) transferExecutor.EndStatus.ToString());
        return false;
      }
    }

    private bool RunQuickTest(TestLocation source, MachineInformation config)
    {
      LogHelper.Instance.Log(" ** scheduling one disk quick deck test ** ");
      int count = config.DecksConfiguration.Count;
      if (config.Configuration != KioskConfiguration.R717)
        count = config.DecksConfiguration.Count - 1;
      using (OneDiskQuickTest oneDiskQuickTest = new OneDiskQuickTest(this.Service, source.Deck, source.Slot, count))
      {
        oneDiskQuickTest.Run();
        LogHelper.Instance.Log(" OneDiskQuickTest ended with status {0}", (object) oneDiskQuickTest.EndStatus);
        return oneDiskQuickTest.EndStatus == HardwareJobStatus.Completed;
      }
    }

    private List<TestLocation> PopulateLocations(TestLocation source, MachineInformation config)
    {
      List<TestLocation> locations = new List<TestLocation>();
      config.DecksConfiguration.ForEach((Action<IDeckConfig>) (dc =>
      {
        if (dc.IsQlm)
          return;
        for (int slot = 1; slot <= dc.SlotCount; ++slot)
        {
          if (dc.Number != source.Deck || slot != source.Slot)
          {
            TestLocation loc = new TestLocation(dc.Number, slot);
            if (this.CanVisit(loc))
              locations.Add(loc);
          }
        }
      }));
      return locations;
    }
  }
}
