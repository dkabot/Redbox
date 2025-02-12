using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;
using System;
using System.IO;
using System.Threading;

namespace HALUtilities
{
  internal sealed class InventoryHelper : IDisposable
  {
    private bool Disposed;
    private int completedSyncs;
    private readonly ManualResetEvent Event = new ManualResetEvent(false);
    private readonly HardwareService Service;
    private readonly ClientHelper Helper;

    public void Dispose()
    {
      if (this.Disposed)
        return;
      this.Disposed = true;
      this.Helper.Dispose();
      this.Event.Close();
    }

    internal void DumpInventoryToFile(string path)
    {
      HardwareCommandResult inventoryState = this.Service.GetInventoryState();
      if (!inventoryState.Errors.ContainsError())
        File.WriteAllText(path, inventoryState.CommandMessages[0]);
      else
        inventoryState.Errors.Dump(Console.Out);
    }

    internal bool ImportInventoryFromFile(string fileName)
    {
      if (!File.Exists(fileName))
      {
        Console.WriteLine("The file '{0}' doesn't exist.", (object) fileName);
        return false;
      }
      try
      {
        return this.Service.SetInventoryState(File.ReadAllText(fileName)).Success;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
        return false;
      }
    }

    internal void CompareSyncResults(HardwareJob syncJob, bool outputSuccessfulCompare)
    {
      ProgramResult[] results;
      if (!syncJob.GetResults(out results).Success)
      {
        LogHelper.Instance.Log("Can't get job results for {0}", (object) syncJob.ID);
      }
      else
      {
        int inventoryMatches = 0;
        int inventoryMismatches = 0;
        Array.ForEach<ProgramResult>(results, (Action<ProgramResult>) (_r =>
        {
          if (!(_r.Code == "InventoryState"))
            return;
          if (this.InventoryItemsEqual(_r, outputSuccessfulCompare))
            ++inventoryMatches;
          else
            ++inventoryMismatches;
        }));
        LogHelper.Instance.Log("Analysis summary:");
        LogHelper.Instance.Log("  {0} matches.", (object) inventoryMatches);
        LogHelper.Instance.Log("  {0} mismatches.", (object) inventoryMismatches);
        if (inventoryMatches + inventoryMismatches == 717)
          return;
        LogHelper.Instance.Log("Wrong analysis - expected 717, found {0}", (object) (inventoryMatches + inventoryMismatches));
      }
    }

    internal bool FixType(SyncType type)
    {
      if (type == SyncType.Empty)
        return this.Service.ServiceEmptySync().Success;
      return type == SyncType.Unknown && this.Service.ServiceUnknownSync().Success;
    }

    internal bool ScheduleFullSync() => this.ScheduleFullSync(SyncOptions.None);

    internal bool ScheduleFullSync(SyncOptions options)
    {
      LogHelper.Instance.Log("Schedule full sync with options: ");
      if (options == SyncOptions.None)
      {
        LogHelper.Instance.Log("  None");
      }
      else
      {
        foreach (SyncOptions syncOptions in Enum.GetValues(typeof (SyncOptions)))
        {
          if ((options & syncOptions) != SyncOptions.None)
            LogHelper.Instance.Log("  {0}", (object) syncOptions.ToString());
        }
      }
      Thread thread = (Thread) null;
      if ((options & SyncOptions.AsyncSoftSync) != SyncOptions.None)
      {
        thread = new Thread(new ThreadStart(this.RunSoftSync));
        thread.Start();
      }
      HardwareCommandResult hardwareCommandResult = this.Service.FullSync();
      if (!hardwareCommandResult.Success)
        return false;
      if ((options & SyncOptions.Attach) != SyncOptions.None)
      {
        HardwareJob job1;
        if (this.Service.GetJob(hardwareCommandResult.CommandMessages[0].Split('|')[0], out job1).Success)
        {
          job1.EventRaised += (HardwareEvent) ((job, eventTime, eventMessage) => LogHelper.Instance.Log("{0}: {1}", (object) eventTime, (object) eventMessage));
          HardwareJobStatus endStatus;
          this.Helper.WaitForJob(job1, out endStatus);
          LogHelper.Instance.Log("{0} Sync job {1} ended with status {2}", (object) DateTime.Now, (object) job1.ID, (object) endStatus.ToString());
          if ((options & SyncOptions.Analyze) != SyncOptions.None)
            this.CompareSyncResults(job1, (options & SyncOptions.OutputSuccessfulSyncCompare) != 0);
          if (thread != null)
          {
            this.Event.Set();
            LogHelper.Instance.Log(" ** Completed {0} soft syncs ** ", (object) this.completedSyncs);
            thread.Join(5000);
          }
        }
      }
      return hardwareCommandResult.Success;
    }

    internal bool RestoreInventory()
    {
      bool flag = true;
      HardwareService service = this.Service;
      HardwareJobSchedule schedule = new HardwareJobSchedule();
      schedule.Priority = HardwareJobPriority.Highest;
      HardwareJob job;
      HardwareCommandResult hardwareCommandResult = service.ScheduleJob("restore-inventory-from-backup", "Utilities restore", false, schedule, out job);
      if (hardwareCommandResult.Success)
      {
        HardwareJobStatus endStatus;
        this.Helper.WaitForJob(job, out endStatus);
        Console.WriteLine("Restore job {0} ended with status {1}", (object) job.ID, (object) endStatus);
        if (HardwareJobStatus.Completed != endStatus)
        {
          Console.WriteLine("Restore job failed.");
          ErrorList errors;
          if (job.GetErrors(out errors).Success)
            errors.Dump(Console.Out);
          flag = false;
        }
        job.Trash();
      }
      else
      {
        Console.WriteLine("Failed to schedule job.");
        hardwareCommandResult.Errors.Dump(Console.Out);
        flag = false;
      }
      return flag;
    }

    internal bool RebuildInventory(bool force)
    {
      bool flag = true;
      using (HardwareStatusExecutor hardwareStatusExecutor = new HardwareStatusExecutor(this.Service))
      {
        hardwareStatusExecutor.Run();
        if (hardwareStatusExecutor.InventoryError | force)
        {
          using (RebuildInventoryExecutor inventoryExecutor = new RebuildInventoryExecutor(this.Service))
          {
            inventoryExecutor.Run();
            Console.WriteLine("Rebuild job {0} ended with status {1}", (object) inventoryExecutor.ID, (object) inventoryExecutor.EndStatus);
            flag = HardwareJobStatus.Completed == inventoryExecutor.EndStatus;
            if (!flag)
            {
              Console.WriteLine("Rebuild job failed.");
              inventoryExecutor.Errors.Dump(Console.Out);
            }
          }
        }
      }
      return flag;
    }

    internal InventoryHelper(HardwareService service)
    {
      this.Service = service;
      this.Helper = new ClientHelper(service);
    }

    private void RunSoftSync()
    {
      HardwareJobSchedule schedule = new HardwareJobSchedule()
      {
        Priority = HardwareJobPriority.High
      };
      using (ClientHelper clientHelper = new ClientHelper(this.Service))
      {
        while (!this.Event.WaitOne(30000))
        {
          HardwareJob job;
          if (this.Service.SoftSync(schedule, out job).Success)
          {
            job.Pend();
            HardwareJobStatus endStatus;
            clientHelper.WaitForJob(job, out endStatus);
            if (HardwareJobStatus.Completed == endStatus)
              ++this.completedSyncs;
            job.Trash();
          }
        }
      }
    }

    private bool InventoryItemsEqual(ProgramResult result, bool logSuccess)
    {
      if (result.ItemID.ToString() == result.PreviousItem.ToString())
      {
        if (logSuccess)
          LogHelper.Instance.Log("Sync: inventory at deck {0} slot {1} ID = {2} was the same", (object) result.Deck, (object) result.Slot, (object) result.PreviousItem.ToString());
        return true;
      }
      LogHelper.Instance.Log("Sync: inventory changed at deck {0} slot {1} ( went from {2} -> {3} )", (object) result.Deck, (object) result.Slot, (object) result.PreviousItem.ToString(), (object) result.ItemID.ToString());
      return false;
    }
  }
}
