using System;
using System.IO;
using System.Threading;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;

namespace HALUtilities
{
    internal sealed class InventoryHelper : IDisposable
    {
        private readonly ManualResetEvent Event = new ManualResetEvent(false);
        private readonly ClientHelper Helper;
        private readonly HardwareService Service;
        private int completedSyncs;
        private bool Disposed;

        internal InventoryHelper(HardwareService service)
        {
            Service = service;
            Helper = new ClientHelper(service);
        }

        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            Helper.Dispose();
            Event.Close();
        }

        internal void DumpInventoryToFile(string path)
        {
            var inventoryState = Service.GetInventoryState();
            if (!inventoryState.Errors.ContainsError())
                File.WriteAllText(path, inventoryState.CommandMessages[0]);
            else
                inventoryState.Errors.Dump(Console.Out);
        }

        internal bool ImportInventoryFromFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Console.WriteLine("The file '{0}' doesn't exist.", fileName);
                return false;
            }

            try
            {
                return Service.SetInventoryState(File.ReadAllText(fileName)).Success;
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
                LogHelper.Instance.Log("Can't get job results for {0}", syncJob.ID);
            }
            else
            {
                var inventoryMatches = 0;
                var inventoryMismatches = 0;
                Array.ForEach(results, _r =>
                {
                    if (!(_r.Code == "InventoryState"))
                        return;
                    if (InventoryItemsEqual(_r, outputSuccessfulCompare))
                        ++inventoryMatches;
                    else
                        ++inventoryMismatches;
                });
                LogHelper.Instance.Log("Analysis summary:");
                LogHelper.Instance.Log("  {0} matches.", inventoryMatches);
                LogHelper.Instance.Log("  {0} mismatches.", inventoryMismatches);
                if (inventoryMatches + inventoryMismatches == 717)
                    return;
                LogHelper.Instance.Log("Wrong analysis - expected 717, found {0}",
                    inventoryMatches + inventoryMismatches);
            }
        }

        internal bool FixType(SyncType type)
        {
            if (type == SyncType.Empty)
                return Service.ServiceEmptySync().Success;
            return type == SyncType.Unknown && Service.ServiceUnknownSync().Success;
        }

        internal bool ScheduleFullSync()
        {
            return ScheduleFullSync(SyncOptions.None);
        }

        internal bool ScheduleFullSync(SyncOptions options)
        {
            LogHelper.Instance.Log("Schedule full sync with options: ");
            if (options == SyncOptions.None)
                LogHelper.Instance.Log("  None");
            else
                foreach (SyncOptions syncOptions in Enum.GetValues(typeof(SyncOptions)))
                    if ((options & syncOptions) != SyncOptions.None)
                        LogHelper.Instance.Log("  {0}", syncOptions.ToString());

            var thread = (Thread)null;
            if ((options & SyncOptions.AsyncSoftSync) != SyncOptions.None)
            {
                thread = new Thread(RunSoftSync);
                thread.Start();
            }

            var hardwareCommandResult = Service.FullSync();
            if (!hardwareCommandResult.Success)
                return false;
            if ((options & SyncOptions.Attach) != SyncOptions.None)
            {
                HardwareJob job1;
                if (Service.GetJob(hardwareCommandResult.CommandMessages[0].Split('|')[0], out job1).Success)
                {
                    job1.EventRaised += (job, eventTime, eventMessage) =>
                        LogHelper.Instance.Log("{0}: {1}", eventTime, eventMessage);
                    HardwareJobStatus endStatus;
                    Helper.WaitForJob(job1, out endStatus);
                    LogHelper.Instance.Log("{0} Sync job {1} ended with status {2}", DateTime.Now, job1.ID,
                        endStatus.ToString());
                    if ((options & SyncOptions.Analyze) != SyncOptions.None)
                        CompareSyncResults(job1, (options & SyncOptions.OutputSuccessfulSyncCompare) != 0);
                    if (thread != null)
                    {
                        Event.Set();
                        LogHelper.Instance.Log(" ** Completed {0} soft syncs ** ", completedSyncs);
                        thread.Join(5000);
                    }
                }
            }

            return hardwareCommandResult.Success;
        }

        internal bool RestoreInventory()
        {
            var flag = true;
            var service = Service;
            var schedule = new HardwareJobSchedule();
            schedule.Priority = HardwareJobPriority.Highest;
            HardwareJob job;
            var hardwareCommandResult = service.ScheduleJob("restore-inventory-from-backup", "Utilities restore", false,
                schedule, out job);
            if (hardwareCommandResult.Success)
            {
                HardwareJobStatus endStatus;
                Helper.WaitForJob(job, out endStatus);
                Console.WriteLine("Restore job {0} ended with status {1}", job.ID, endStatus);
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
            var flag = true;
            using (var hardwareStatusExecutor = new HardwareStatusExecutor(Service))
            {
                hardwareStatusExecutor.Run();
                if (hardwareStatusExecutor.InventoryError | force)
                    using (var inventoryExecutor = new RebuildInventoryExecutor(Service))
                    {
                        inventoryExecutor.Run();
                        Console.WriteLine("Rebuild job {0} ended with status {1}", inventoryExecutor.ID,
                            inventoryExecutor.EndStatus);
                        flag = HardwareJobStatus.Completed == inventoryExecutor.EndStatus;
                        if (!flag)
                        {
                            Console.WriteLine("Rebuild job failed.");
                            inventoryExecutor.Errors.Dump(Console.Out);
                        }
                    }
            }

            return flag;
        }

        private void RunSoftSync()
        {
            var schedule = new HardwareJobSchedule
            {
                Priority = HardwareJobPriority.High
            };
            using (var clientHelper = new ClientHelper(Service))
            {
                while (!Event.WaitOne(30000))
                {
                    HardwareJob job;
                    if (Service.SoftSync(schedule, out job).Success)
                    {
                        job.Pend();
                        HardwareJobStatus endStatus;
                        clientHelper.WaitForJob(job, out endStatus);
                        if (HardwareJobStatus.Completed == endStatus)
                            ++completedSyncs;
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
                    LogHelper.Instance.Log("Sync: inventory at deck {0} slot {1} ID = {2} was the same", result.Deck,
                        result.Slot, result.PreviousItem.ToString());
                return true;
            }

            LogHelper.Instance.Log("Sync: inventory changed at deck {0} slot {1} ( went from {2} -> {3} )", result.Deck,
                result.Slot, result.PreviousItem.ToString(), result.ItemID.ToString());
            return false;
        }
    }
}