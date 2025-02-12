using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Compression;
using Redbox.HAL.IPC.Framework;
using Redbox.IPC.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redbox.HAL.Client
{
  public sealed class HardwareService
  {
    private const int DefaultTimeout = 60000;

    public HardwareService(IPCProtocol protocol) => this.Protocol = protocol;

    public HardwareCommandResult Init(HardwareJobSchedule schedule, out HardwareJob job)
    {
      return this.ScheduleJob("init", schedule, out job);
    }

    public HardwareCommandResult Unload(HardwareJobSchedule schedule, out HardwareJob job)
    {
      return this.ScheduleJob("qlm-unload", schedule, out job);
    }

    public HardwareCommandResult CleanZone(HardwareJobSchedule schedule, out HardwareJob job)
    {
      return this.ScheduleJob("clean-vmz", schedule, out job);
    }

    public HardwareCommandResult CleanZone(out HardwareJob job)
    {
      return this.ScheduleJob("clean-vmz", new HardwareJobSchedule()
      {
        Priority = HardwareJobPriority.Low
      }, out job);
    }

    public HardwareCommandResult Thin(
      string[] ids,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("thin", schedule, out job);
      if (!hardwareCommandResult.Success)
        return hardwareCommandResult;
      this.PushArray(ids, job);
      return hardwareCommandResult;
    }

    public HardwareCommandResult ThinVMZ(
      string[] toBin,
      string[] rebalance,
      string[] thins,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("thin-vmz", schedule, out job);
      if (!hardwareCommandResult.Success)
        return hardwareCommandResult;
      this.PushArray(thins, job);
      this.PushArray((string[]) null, job);
      this.PushArray(rebalance, job);
      this.PushArray(toBin, job);
      return hardwareCommandResult;
    }

    public HardwareCommandResult ThinVMZ(
      string[] toBin,
      string[] rebalance,
      string[] thinRedeploys,
      string[] thins,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("thin-vmz", schedule, out job);
      if (!hardwareCommandResult.Success)
        return hardwareCommandResult;
      this.PushArray(thins, job);
      this.PushArray(thinRedeploys, job);
      this.PushArray(rebalance, job);
      this.PushArray(toBin, job);
      return hardwareCommandResult;
    }

    public HardwareCommandResult ThinVMZ(
      string[] toBin,
      string[] thins,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("thin-vmz", schedule, out job);
      if (!hardwareCommandResult.Success)
        return hardwareCommandResult;
      this.PushArray(thins, job);
      this.PushArray((string[]) null, job);
      this.PushArray((string[]) null, job);
      this.PushArray(toBin, job);
      return hardwareCommandResult;
    }

    public HardwareCommandResult GetNonBarcodeInventory(
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      return this.ScheduleJob("get-non-barcode-inventory", schedule, out job);
    }

    public HardwareCommandResult GetInventoryStats(
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      return this.ScheduleJob("get-inventory-stats", schedule, out job);
    }

    public HardwareCommandResult ChangeAudioChannelState(
      HardwareJobSchedule schedule,
      SpeakerState newState,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("change-audio-channel-state", schedule, out job);
      if (hardwareCommandResult.Success)
        job.Push((object) newState.ToString());
      return hardwareCommandResult;
    }

    public HardwareCommandResult ResetLocations(
      List<Location> locs,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("reset-locations", schedule, out job);
      if (!hardwareCommandResult.Success)
        return hardwareCommandResult;
      this.PushLocs(locs, job);
      return hardwareCommandResult;
    }

    public HardwareCommandResult SyncUnknowns(HardwareJobSchedule schedule, out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ExecuteServiceCommand("SERVICE sync-unknowns");
      job = hardwareCommandResult.Success ? HardwareJob.Parse(this, hardwareCommandResult.CommandMessages[0]) : (HardwareJob) null;
      return hardwareCommandResult;
    }

    public HardwareCommandResult PrepUnloadZone(
      int rotation,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("prep-zone-for-unload", schedule, out job);
      if (hardwareCommandResult.Success)
      {
        job.Push((object) 0);
        job.Push((object) rotation);
      }
      return hardwareCommandResult;
    }

    public HardwareCommandResult PrepUnloadZone(
      int rotation,
      int deckCount,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("prep-zone-for-unload", schedule, out job);
      if (hardwareCommandResult.Success)
      {
        job.Push((object) deckCount);
        job.Push((object) rotation);
      }
      return hardwareCommandResult;
    }

    public HardwareCommandResult PrepDeckForRedeploy(
      string deckNumber,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("prep-deck-for-redeploy", schedule, out job);
      if (hardwareCommandResult.Success)
      {
        job.Push((object) deckNumber);
        job.Push((object) 1);
      }
      return hardwareCommandResult;
    }

    public HardwareCommandResult PrepForRedeploy(
      string[] decks,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("prep-deck-for-redeploy", schedule, out job);
      if (!hardwareCommandResult.Success)
        return hardwareCommandResult;
      this.PushArray(decks, job);
      return hardwareCommandResult;
    }

    public HardwareCommandResult VMZRemovedThins(HardwareJobSchedule schedule, out HardwareJob job)
    {
      return this.ScheduleJob("vmz-removed-thins", schedule, out job);
    }

    public HardwareCommandResult ClearMerchStatus(
      string[] barcodesToClear,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("clear-merch-status", schedule, out job);
      if (!hardwareCommandResult.Success)
        return hardwareCommandResult;
      this.PushArray(barcodesToClear, job);
      return hardwareCommandResult;
    }

    public HardwareCommandResult LoadBin(
      List<Location> locs,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("load-bin", schedule, out job);
      if (hardwareCommandResult.Success)
      {
        this.PushLocs(locs, job);
        job.Push((object) "LOCATION");
      }
      return hardwareCommandResult;
    }

    public HardwareCommandResult LoadBin(
      string[] barcodes,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("load-bin", schedule, out job);
      if (hardwareCommandResult.Success)
      {
        this.PushArray(barcodes, job);
        job.Push((object) "BARCODE");
      }
      return hardwareCommandResult;
    }

    public HardwareCommandResult Vend(
      string[] ids,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("vend", schedule, out job);
      if (hardwareCommandResult.Success)
      {
        this.PushArray(ids, job);
        job.Push((object) "BY-ID");
      }
      return hardwareCommandResult;
    }

    public HardwareCommandResult Vend(
      List<Location> locations,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("vend", schedule, out job);
      if (hardwareCommandResult.Success)
      {
        this.PushLocs(locations, job);
        job.Push((object) "BY-LOCATION");
      }
      return hardwareCommandResult;
    }

    public HardwareCommandResult GetBarcodeVMZPosition(
      string[] idsToQuery,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      HardwareCommandResult barcodeVmzPosition = this.ScheduleJob("get-barcode-vmz-location", schedule, out job);
      if (!barcodeVmzPosition.Success)
        return barcodeVmzPosition;
      this.PushArray(idsToQuery, job);
      return barcodeVmzPosition;
    }

    public HardwareCommandResult VMZDetail(HardwareJobSchedule schedule, out HardwareJob job)
    {
      return this.ScheduleJob("vmz-details", schedule, out job);
    }

    public HardwareCommandResult VMZMerchSummary(HardwareJobSchedule schedule, out HardwareJob job)
    {
      return this.ScheduleJob("vmz-merch-summary", schedule, out job);
    }

    public HardwareCommandResult FileMarkerDisk(
      int deck,
      int slot,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("file-marker-disk", schedule, out job);
      if (hardwareCommandResult.Success)
      {
        job.Push((object) slot);
        job.Push((object) deck);
      }
      return hardwareCommandResult;
    }

    public HardwareCommandResult MerchandizeClearAndOffset(
      int deck,
      int slot,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("merch-clear-and-offset", schedule, out job);
      if (hardwareCommandResult.Success)
      {
        job.Push((object) slot);
        job.Push((object) deck);
      }
      return hardwareCommandResult;
    }

    public HardwareCommandResult NotifyMaintModeState(bool goingIntoMM)
    {
      return this.ExecuteCommand(string.Format("SERVICE mm-status status: '{0}'", (object) goingIntoMM.ToString()));
    }

    public HardwareCommandResult GetNonThinsInVMZ(HardwareJobSchedule schedule, out HardwareJob job)
    {
      return this.ScheduleJob("get-nonthins-in-vmz", schedule, out job);
    }

    public HardwareCommandResult GetBarcodesInBin(HardwareJobSchedule schedule, out HardwareJob job)
    {
      return this.ScheduleJob("get-barcodes-in-bin", schedule, out job);
    }

    public HardwareCommandResult HardSync(
      SyncRange range,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("sync", schedule, out job);
      if (hardwareCommandResult.Success)
        job.Push((object) range.Slots.End, (object) range.EndDeck, (object) range.Slots.Start, (object) range.StartDeck);
      return hardwareCommandResult;
    }

    public HardwareCommandResult HardSync(
      List<Location> locations,
      string label,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("sync-locations", label, false, schedule, out job);
      if (!hardwareCommandResult.Success)
        return hardwareCommandResult;
      this.PushLocs(locations, job);
      return hardwareCommandResult;
    }

    public HardwareCommandResult HardSync(
      List<Location> locations,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      return this.HardSync(locations, (string) null, schedule, out job);
    }

    public HardwareCommandResult FieldInsertFraudWithCheck(
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      return this.ScheduleJob("field-insert-with-fraud-check", schedule, out job);
    }

    public HardwareCommandResult FraudHardwarePost(
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      return this.ScheduleJob("fraud-sensor-post-test", schedule, out job);
    }

    public HardwareCommandResult ReadFraudDisc(HardwareJobSchedule schedule, out HardwareJob job)
    {
      return this.ScheduleJob("read-fraud-disc", schedule, out job);
    }

    public HardwareCommandResult Return(HardwareJobSchedule schedule, out HardwareJob job)
    {
      return this.ScheduleJob("return", schedule, out job);
    }

    public HardwareCommandResult SoftSync(HardwareJobSchedule schedule, out HardwareJob job)
    {
      return this.ScheduleJob("soft-sync", schedule, out job);
    }

    public HardwareCommandResult QlmUnload(HardwareJobSchedule schedule, out HardwareJob job)
    {
      return this.ScheduleJob("qlm-unload", schedule, out job);
    }

    public HardwareCommandResult CheckPicker(HardwareJobSchedule schedule, out HardwareJob job)
    {
      return this.ScheduleJob("check-picker", schedule, out job);
    }

    public HardwareCommandResult GetMachineInfo(HardwareJobSchedule schedule, out HardwareJob job)
    {
      return this.ScheduleJob("get-machine-info", schedule, out job);
    }

    public HardwareCommandResult GetHardwareStatus(
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      return this.ScheduleJob("hardware-status", schedule, out job);
    }

    public HardwareCommandResult QlmUnloadAndThin(
      string[] ids,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("unload-thin", schedule, out job);
      if (!hardwareCommandResult.Success)
        return hardwareCommandResult;
      this.PushArray(ids, job);
      return hardwareCommandResult;
    }

    public HardwareCommandResult VendUnknown(
      int deck,
      int slot,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("vend-unknown", schedule, out job);
      if (hardwareCommandResult.Success)
      {
        job.Push((object) slot);
        job.Push((object) deck);
      }
      return hardwareCommandResult;
    }

    public HardwareCommandResult ReturnUnknown(
      HardwareJobSchedule schedule,
      string returnTime,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("return-unknown", schedule, out job);
      if (hardwareCommandResult.Success)
        job.Push((object) returnTime);
      return hardwareCommandResult;
    }

    public HardwareCommandResult PreposVend(
      HardwareJobSchedule schedule,
      string[] ids,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("vend", schedule, out job);
      if (hardwareCommandResult.Success)
      {
        this.PushArray(ids, job);
        job.Push((object) "PREPOSITION-PICKER");
      }
      return hardwareCommandResult;
    }

    public HardwareCommandResult ExchangerStatus(HardwareJobSchedule schedule, out HardwareJob job)
    {
      return this.ScheduleJob("air-exchanger-status", schedule, out job);
    }

    public HardwareCommandResult MarkBarcodesUnknown(
      HardwareJobSchedule s,
      string[] barcodes,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("mark-barcodes-unknown", s, out job);
      if (!hardwareCommandResult.Success)
        return hardwareCommandResult;
      this.PushArray(barcodes, job);
      return hardwareCommandResult;
    }

    public HardwareCommandResult MarkLocationsUnknown(
      HardwareJobSchedule s,
      List<Location> locs,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("mark-locations-unknown", s, out job);
      if (!hardwareCommandResult.Success)
        return hardwareCommandResult;
      this.PushLocs(locs, job);
      return hardwareCommandResult;
    }

    public HardwareCommandResult ResetTouchscreenController(
      HardwareJobSchedule s,
      out HardwareJob job)
    {
      return this.ScheduleJob("reset-touchscreen-controller", s, out job);
    }

    public HardwareCommandResult GetHardwareCorrectionStats(
      HardwareCorrectionStatistic stat,
      out List<IHardwareCorrectionStatistic> stats)
    {
      stats = new List<IHardwareCorrectionStatistic>();
      using (GetHardwareCorrectionStatistics correctionStatistics = new GetHardwareCorrectionStatistics(this, stat))
      {
        correctionStatistics.Run();
        if (HardwareJobStatus.Completed == correctionStatistics.EndStatus)
          stats.AddRange((IEnumerable<IHardwareCorrectionStatistic>) correctionStatistics.Stats);
        return correctionStatistics.ScheduleResult;
      }
    }

    public HardwareCommandResult GetHardwareCorrectionStats(
      out List<IHardwareCorrectionStatistic> stats)
    {
      stats = new List<IHardwareCorrectionStatistic>();
      using (GetAllHardwareCorrectionStatistics correctionStatistics = new GetAllHardwareCorrectionStatistics(this))
      {
        correctionStatistics.Run();
        if (HardwareJobStatus.Completed == correctionStatistics.EndStatus)
          stats.AddRange((IEnumerable<IHardwareCorrectionStatistic>) correctionStatistics.Stats);
        return correctionStatistics.ScheduleResult;
      }
    }

    public HardwareCommandResult RemoveHardwareCorrectionStats(
      HardwareCorrectionStatistic stat,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      HardwareCommandResult hardwareCommandResult = this.ScheduleJob("clear-hardware-statistics", (string) null, false, schedule, out job);
      if (hardwareCommandResult.Success)
        job.Push((object) stat.ToString());
      return hardwareCommandResult;
    }

    public HardwareCommandResult RemoveHardwareCorrectionStats(
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      return this.ScheduleJob("clear-all-hardware-statistics", (string) null, false, schedule, out job);
    }

    public HardwareCommandResult GetConfiguration(string name)
    {
      return this.ExecuteCommand(string.Format("CONFIG get name: '{0}'", (object) name));
    }

    public HardwareCommandResult SetConfiguration(string name, string xmlConfig)
    {
      byte[] inArray = CompressionAlgorithmFactory.GetAlgorithm(CompressionType.LZMA).Compress(Encoding.ASCII.GetBytes(xmlConfig));
      return this.ExecuteCommand(string.Format("CONFIG set name: '{0}' data: '{1}'", (object) name, (object) Convert.ToBase64String(inArray)));
    }

    public HardwareCommandResult LoadConfiguration(string path)
    {
      return this.ExecuteCommand(string.Format("CONFIG load {0}", path != null ? (object) ("path: '" + path + "'") : (object) string.Empty));
    }

    public HardwareCommandResult SaveConfiguration(string path)
    {
      return this.ExecuteCommand(string.Format("CONFIG save {0}", path != null ? (object) ("path: '" + path + "'") : (object) string.Empty));
    }

    public HardwareCommandResult RestoreConfiguration(string path)
    {
      return this.ExecuteCommand(string.Format("CONFIG restore {0}", path != null ? (object) ("path: '" + path + "'") : (object) string.Empty));
    }

    public HardwareCommandResult GetInventoryState()
    {
      return this.ExecuteCommand("CONFIG get-inventory-state");
    }

    public HardwareCommandResult SetInventoryState(string xml)
    {
      return this.ExecuteCommand(string.Format("CONFIG set-inventory-state data: '{0}'", (object) Convert.ToBase64String(CompressionAlgorithmFactory.GetAlgorithm(CompressionType.LZMA).Compress(Encoding.ASCII.GetBytes(xml)))));
    }

    public HardwareCommandResult PowerCycleRouter(HardwareJobSchedule s, out HardwareJob job)
    {
      return this.ScheduleJob("power-cycle-router", s, out job);
    }

    public HardwareCommandResult GetProgramScript(string programName)
    {
      return this.ExecuteCommand(string.Format("PROGRAM get name: '{0}'", (object) programName));
    }

    public HardwareCommandResult SetProgramScript(string programName, string script)
    {
      byte[] inArray = CompressionAlgorithmFactory.GetAlgorithm(CompressionType.LZMA).Compress(Encoding.ASCII.GetBytes(script));
      return this.ExecuteCommand(string.Format("PROGRAM set name: '{0}' data: '{1}'", (object) programName, (object) Convert.ToBase64String(inArray)));
    }

    public HardwareCommandResult SetProgramRequiresClientConnection(
      string programName,
      bool requiresClientConnection)
    {
      return this.ExecuteCommand(string.Format("PROGRAM properties name: '{0}' requires-client-connection: {1}", (object) programName, (object) requiresClientConnection));
    }

    public HardwareCommandResult ResumeJobs(HardwareJob[] jobs)
    {
      HardwareCommandResult hardwareCommandResult1 = new HardwareCommandResult();
      foreach (HardwareJob job in jobs)
      {
        HardwareCommandResult hardwareCommandResult2 = job.Resume();
        if (!hardwareCommandResult2.Success && hardwareCommandResult2.Errors.Count > 0)
          hardwareCommandResult1.Errors.AddRange((IEnumerable<Error>) hardwareCommandResult2.Errors);
      }
      return hardwareCommandResult1;
    }

    public HardwareCommandResult SuspendJobsBelowPriority(
      HardwareJobPriority priority,
      out HardwareJob[] suspendedJobs)
    {
      List<HardwareJob> hardwareJobList = new List<HardwareJob>();
      HardwareJob[] jobs1 = (HardwareJob[]) null;
      HardwareCommandResult hardwareCommandResult1 = new HardwareCommandResult();
      HardwareCommandResult jobs2 = this.GetJobs(out jobs1);
      if (!jobs2.Success)
      {
        if (jobs2.Errors.Count > 0)
          hardwareCommandResult1.Errors.AddRange((IEnumerable<Error>) jobs2.Errors);
        suspendedJobs = hardwareJobList.ToArray();
        return hardwareCommandResult1;
      }
      foreach (HardwareJob hardwareJob in jobs1)
      {
        if ((!(hardwareJob.ID != Constants.ExecutionContexts.ImmediateModeContext) ? 0 : (hardwareJob.Status == HardwareJobStatus.Pending ? 1 : (hardwareJob.Status == HardwareJobStatus.Running ? 1 : 0))) != 0 && hardwareJob.Priority > priority)
        {
          HardwareCommandResult hardwareCommandResult2 = hardwareJob.Suspend();
          if (hardwareCommandResult2.Success)
            hardwareJobList.Add(hardwareJob);
          else if (hardwareCommandResult2.Errors.Count > 0)
            hardwareCommandResult1.Errors.AddRange((IEnumerable<Error>) hardwareCommandResult2.Errors);
        }
      }
      suspendedJobs = hardwareJobList.ToArray();
      return hardwareCommandResult1;
    }

    public HardwareCommandResult SuspendAll() => this.ExecuteCommand("JOB suspend-all");

    public HardwareCommandResult ResumeAll() => this.ExecuteCommand("JOB resume-all");

    public HardwareCommandResult CollectGarbage(bool force)
    {
      return this.ExecuteCommand(string.Format("JOB collect-garbage force: {0}", force ? (object) "true" : (object) "false"));
    }

    public HardwareCommandResult TrashJob(string jobId)
    {
      return this.ExecuteCommand(string.Format("JOB trash job: {0}", (object) jobId));
    }

    public HardwareCommandResult GetJob(string jobId, out HardwareJob job)
    {
      job = (HardwareJob) null;
      HardwareCommandResult job1 = this.ExecuteCommand(string.Format("JOB get job: '{0}'", (object) jobId));
      if (job1.Success)
        job = HardwareJob.Parse(this, job1.CommandMessages[0]);
      return job1;
    }

    public HardwareCommandResult GetJobs(out HardwareJob[] jobs)
    {
      HardwareCommandResult jobs1 = this.ExecuteCommand("JOB list");
      jobs = jobs1.Success ? jobs1.CommandMessages.ConvertAll<HardwareJob>((Converter<string, HardwareJob>) (each => HardwareJob.Parse(this, each))).ToArray() : new HardwareJob[0];
      return jobs1;
    }

    public HardwareCommandResult CompileProgram(
      string path,
      string programName,
      bool requiresClientConnection)
    {
      return this.ExecuteCommand(string.Format("PROGRAM compile path: @'{0}' name: '{1}' requires-client-connection: {2}", (object) path, (object) programName, (object) requiresClientConnection));
    }

    public HardwareCommandResult RemoveProgram(string programName)
    {
      return this.ExecuteCommand(string.Format("PROGRAM remove name: '{0}'", (object) programName));
    }

    public HardwareCommandResult ScheduleJob(
      string programName,
      string label,
      bool enableDebugging,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      string command = string.Format("JOB schedule name: '{0}' priority: {1}", (object) programName, (object) schedule.Priority);
      if (enableDebugging)
        command += string.Format(" debugging: {0}", (object) enableDebugging);
      if (schedule.StartTime.HasValue)
        command += string.Format(" startTime: '{0}'", (object) schedule.StartTime);
      if (label != null)
        command += string.Format(" label: '{0}'", (object) label);
      job = (HardwareJob) null;
      HardwareCommandResult hardwareCommandResult = this.ExecuteCommand(command);
      if (hardwareCommandResult.Success)
        job = HardwareJob.Parse(this, hardwareCommandResult.CommandMessages[0]);
      return hardwareCommandResult;
    }

    public HardwareCommandResult GetPrograms() => this.ExecuteCommand("PROGRAM list");

    public HardwareCommandResult ExecuteImmediate(
      string statement,
      int? timeout,
      out HardwareJob job)
    {
      job = (HardwareJob) null;
      HardwareCommandResult hardwareCommandResult = this.ExecuteCommand(string.Format("JOB execute-immediate statement: '{0}'", (object) statement), timeout.HasValue ? timeout.Value : 30000);
      if (hardwareCommandResult.Success)
        job = HardwareJob.Parse(this, hardwareCommandResult.CommandMessages[0]);
      return hardwareCommandResult;
    }

    public HardwareCommandResult ExecuteImmediate(string statement, out HardwareJob job)
    {
      job = (HardwareJob) null;
      HardwareCommandResult hardwareCommandResult = this.ExecuteCommand(string.Format("JOB execute-immediate statement: '{0}'", (object) statement));
      if (hardwareCommandResult.Success)
        job = HardwareJob.Parse(this, hardwareCommandResult.CommandMessages[0]);
      return hardwareCommandResult;
    }

    public HardwareCommandResult GetSchedulerStatus(out string status)
    {
      status = (string) null;
      HardwareCommandResult schedulerStatus = this.ExecuteCommand("JOB scheduler-status");
      if (schedulerStatus.Success)
        status = schedulerStatus.CommandMessages[0];
      return schedulerStatus;
    }

    public HardwareCommandResult ExecuteImmediateProgram(byte[] bytes, out HardwareJob job)
    {
      job = (HardwareJob) null;
      HardwareCommandResult hardwareCommandResult = this.ExecuteCommand(string.Format("JOB execute-immediate-base64 statement: '{0}'", (object) Convert.ToBase64String(bytes)));
      if (hardwareCommandResult.Success)
        job = HardwareJob.Parse(this, hardwareCommandResult.CommandMessages[0]);
      return hardwareCommandResult;
    }

    public HardwareCommandResult GetInitStatus(out string status)
    {
      status = (string) null;
      HardwareCommandResult initStatus = this.ExecuteCommand("JOB init-status");
      if (initStatus.Success)
        status = initStatus.CommandMessages[0];
      return initStatus;
    }

    public HardwareCommandResult GetKioskID(out string id)
    {
      id = "UNKNOWN";
      HardwareCommandResult kioskId = this.ExecuteCommand("SERVICE get-kiosk-id");
      if (kioskId.Success)
        id = kioskId.CommandMessages[0];
      return kioskId;
    }

    public HardwareCommandResult GetQuickReturnStatus(out string status)
    {
      status = (string) null;
      HardwareCommandResult quickReturnStatus = this.ExecuteCommand("JOB quick-return-status");
      if (quickReturnStatus.Success)
        status = quickReturnStatus.CommandMessages[0];
      return quickReturnStatus;
    }

    public HardwareCommandResult ServiceUnknownSync()
    {
      return this.ExecuteCommand("SERVICE sync-unknowns");
    }

    public HardwareCommandResult ServiceEmptySync() => this.ExecuteCommand("SERVICE sync-empty");

    public HardwareCommandResult FullSync() => this.ExecuteCommand("SERVICE full-sync");

    public HardwareCommandResult RebuildInventory(HardwareJobSchedule s, out HardwareJob job)
    {
      return this.ScheduleJob("rebuild-inventory-database", s, out job);
    }

    public HardwareCommandResult TestAndResetCCR(HardwareJobSchedule schedule, out HardwareJob job)
    {
      return this.ScheduleJob("test-and-reset-ccr", (string) null, false, schedule, out job);
    }

    public HardwareCommandResult TestSomeData(int dataSize)
    {
      return this.ExecuteCommand(string.Format("ipctest test-ipc-xfer size: '{0}'", (object) dataSize.ToString()));
    }

    public HardwareCommandResult GetKioskFunctionCheckData(
      out IList<IKioskFunctionCheckData> sessions)
    {
      sessions = (IList<IKioskFunctionCheckData>) new List<IKioskFunctionCheckData>();
      using (KioskFunctionCheckExecutor functionCheckExecutor = new KioskFunctionCheckExecutor(this))
      {
        functionCheckExecutor.Run();
        if (functionCheckExecutor.ScheduleResult.Success)
        {
          foreach (IKioskFunctionCheckData session in (IEnumerable<IKioskFunctionCheckData>) functionCheckExecutor.Sessions)
            sessions.Add(session);
        }
        return functionCheckExecutor.ScheduleResult;
      }
    }

    public HardwareCommandResult ExecuteServiceCommand(string command)
    {
      return this.ExecuteCommand(command);
    }

    public HardwareCommandResult ExecuteServiceCommand(string command, int timeout)
    {
      return this.ExecuteCommand(command, timeout);
    }

    public HardwareCommandResult QueryHealth(string outputFile)
    {
      return this.ExecuteCommand(string.Format("HEALTH check path: @'{0}'", (object) outputFile));
    }

    public bool SupportsCameraFraudScan
    {
      get
      {
        using (MachineConfiguration machineConfiguration = new MachineConfiguration(this))
        {
          machineConfiguration.Run();
          return machineConfiguration.SupportsFraudScan;
        }
      }
    }

    public DateTime? IRHardwareInstalled
    {
      get
      {
        using (MachineConfiguration machineConfiguration = new MachineConfiguration(this))
        {
          machineConfiguration.Run();
          return machineConfiguration.IRHardwareInstall;
        }
      }
    }

    public bool HasABEDevice
    {
      get
      {
        using (MachineConfiguration machineConfiguration = new MachineConfiguration(this))
        {
          machineConfiguration.Run();
          return machineConfiguration.HasABEDevice;
        }
      }
    }

    public bool SupportsRouterReset
    {
      get
      {
        using (MachineConfiguration machineConfiguration = new MachineConfiguration(this))
        {
          machineConfiguration.Run();
          return machineConfiguration.HasRouterPowerRelay;
        }
      }
    }

    public CameraGeneration CurrentCameraGeneration
    {
      get
      {
        using (MachineConfiguration machineConfiguration = new MachineConfiguration(this))
        {
          machineConfiguration.Run();
          return machineConfiguration.CurrentCameraGeneration;
        }
      }
    }

    public BarcodeServices BarcodeDecoder
    {
      get
      {
        using (MachineConfiguration machineConfiguration = new MachineConfiguration(this))
        {
          machineConfiguration.Run();
          return machineConfiguration.BarcodeDecoder;
        }
      }
    }

    public int? CommandTimeout { get; set; }

    internal HardwareCommandResult ExecuteCommand(string command)
    {
      return ClientCommand<HardwareCommandResult>.ExecuteCommand(this.Protocol, command);
    }

    internal HardwareCommandResult ExecuteCommand(string command, int timeout)
    {
      return ClientCommand<HardwareCommandResult>.ExecuteCommand(this.Protocol, new int?(timeout), command);
    }

    internal IIpcClientSession GetSession() => ClientSessionFactory.GetClientSession(this.Protocol);

    internal IPCProtocol Protocol { get; private set; }

    private HardwareCommandResult ScheduleJob(
      string name,
      HardwareJobSchedule schedule,
      out HardwareJob job)
    {
      return this.ScheduleJob(name, (string) null, false, schedule, out job);
    }

    private void PushLocs(List<Location> locs, HardwareJob job)
    {
      if (locs == null || locs.Count == 0)
      {
        job.Push((object) 0);
      }
      else
      {
        locs.ForEach((Action<Location>) (loc =>
        {
          job.Push((object) loc.Slot);
          job.Push((object) loc.Deck);
        }));
        job.Push((object) locs.Count);
      }
    }

    private void PushArray(string[] array, HardwareJob job)
    {
      if (array == null || array.Length == 0)
      {
        job.Push((object) 0);
      }
      else
      {
        job.Push((object[]) array);
        job.Push((object) array.Length);
      }
    }
  }
}
