using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using Redbox.HAL.IPC.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Client
{
    public sealed class HardwareService
    {
        private const int DefaultTimeout = 60000;

        public HardwareService(IPCProtocol protocol)
        {
            Protocol = protocol;
        }

        public bool HasABEDevice
        {
            get
            {
                using (var machineConfiguration = new MachineConfiguration(this))
                {
                    machineConfiguration.Run();
                    return machineConfiguration.HasABEDevice;
                }
            }
        }

        public bool HasFraudDevice
        {
            get
            {
                using (var machineConfiguration = new MachineConfiguration(this))
                {
                    machineConfiguration.Run();
                    return machineConfiguration.HasFraudDevice;
                }
            }
        }

        public int? CommandTimeout { get; set; }

        internal IPCProtocol Protocol { get; }

        public HardwareCommandResult Init(HardwareJobSchedule schedule, out HardwareJob job)
        {
            return ScheduleJob("init", schedule, out job);
        }

        public HardwareCommandResult Unload(HardwareJobSchedule schedule, out HardwareJob job)
        {
            return ScheduleJob("qlm-unload", schedule, out job);
        }

        public HardwareCommandResult CleanZone(HardwareJobSchedule schedule, out HardwareJob job)
        {
            return ScheduleJob("clean-vmz", schedule, out job);
        }

        public HardwareCommandResult CleanZone(out HardwareJob job)
        {
            return ScheduleJob("clean-vmz", new HardwareJobSchedule
            {
                Priority = HardwareJobPriority.Low
            }, out job);
        }

        public HardwareCommandResult Thin(
            string[] ids,
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            var hardwareCommandResult = ScheduleJob("thin", schedule, out job);
            if (!hardwareCommandResult.Success)
                return hardwareCommandResult;
            PushArray(ids, job);
            return hardwareCommandResult;
        }

        public HardwareCommandResult ThinVMZ(
            string[] toBin,
            string[] rebalance,
            string[] thins,
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            var hardwareCommandResult = ScheduleJob("thin-vmz", schedule, out job);
            if (!hardwareCommandResult.Success)
                return hardwareCommandResult;
            PushArray(thins, job);
            PushArray(null, job);
            PushArray(rebalance, job);
            PushArray(toBin, job);
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
            var hardwareCommandResult = ScheduleJob("thin-vmz", schedule, out job);
            if (!hardwareCommandResult.Success)
                return hardwareCommandResult;
            PushArray(thins, job);
            PushArray(thinRedeploys, job);
            PushArray(rebalance, job);
            PushArray(toBin, job);
            return hardwareCommandResult;
        }

        public HardwareCommandResult ThinVMZ(
            string[] toBin,
            string[] thins,
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            var hardwareCommandResult = ScheduleJob("thin-vmz", schedule, out job);
            if (!hardwareCommandResult.Success)
                return hardwareCommandResult;
            PushArray(thins, job);
            PushArray(null, job);
            PushArray(null, job);
            PushArray(toBin, job);
            return hardwareCommandResult;
        }

        public HardwareCommandResult GetNonBarcodeInventory(
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            return ScheduleJob("get-non-barcode-inventory", schedule, out job);
        }

        public HardwareCommandResult GetInventoryStats(
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            return ScheduleJob("get-inventory-stats", schedule, out job);
        }

        public HardwareCommandResult ChangeAudioChannelState(
            HardwareJobSchedule schedule,
            SpeakerState newState,
            out HardwareJob job)
        {
            var hardwareCommandResult = ScheduleJob("change-audio-channel-state", schedule, out job);
            if (hardwareCommandResult.Success)
                job.Push(newState.ToString());
            return hardwareCommandResult;
        }

        public HardwareCommandResult ResetLocations(
            List<Location> locs,
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            var hardwareCommandResult = ScheduleJob("reset-locations", schedule, out job);
            if (!hardwareCommandResult.Success)
                return hardwareCommandResult;
            PushLocs(locs, job);
            return hardwareCommandResult;
        }

        public HardwareCommandResult SyncUnknowns(HardwareJobSchedule schedule, out HardwareJob job)
        {
            var hardwareCommandResult = ExecuteServiceCommand("SERVICE sync-unknowns");
            job = hardwareCommandResult.Success
                ? HardwareJob.Parse(this, hardwareCommandResult.CommandMessages[0])
                : null;
            return hardwareCommandResult;
        }

        public HardwareCommandResult PrepUnloadZone(
            int rotation,
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            var hardwareCommandResult = ScheduleJob("prep-zone-for-unload", schedule, out job);
            if (hardwareCommandResult.Success)
            {
                job.Push(0);
                job.Push(rotation);
            }

            return hardwareCommandResult;
        }

        public HardwareCommandResult PrepUnloadZone(
            int rotation,
            int deckCount,
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            var hardwareCommandResult = ScheduleJob("prep-zone-for-unload", schedule, out job);
            if (hardwareCommandResult.Success)
            {
                job.Push(deckCount);
                job.Push(rotation);
            }

            return hardwareCommandResult;
        }

        public HardwareCommandResult PrepDeckForRedeploy(
            string deckNumber,
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            var hardwareCommandResult = ScheduleJob("prep-deck-for-redeploy", schedule, out job);
            if (hardwareCommandResult.Success)
            {
                job.Push(deckNumber);
                job.Push(1);
            }

            return hardwareCommandResult;
        }

        public HardwareCommandResult PrepForRedeploy(
            string[] decks,
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            var hardwareCommandResult = ScheduleJob("prep-deck-for-redeploy", schedule, out job);
            if (!hardwareCommandResult.Success)
                return hardwareCommandResult;
            PushArray(decks, job);
            return hardwareCommandResult;
        }

        public HardwareCommandResult VMZRemovedThins(HardwareJobSchedule schedule, out HardwareJob job)
        {
            return ScheduleJob("vmz-removed-thins", schedule, out job);
        }

        public HardwareCommandResult ClearMerchStatus(
            string[] barcodesToClear,
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            var hardwareCommandResult = ScheduleJob("clear-merch-status", schedule, out job);
            if (!hardwareCommandResult.Success)
                return hardwareCommandResult;
            PushArray(barcodesToClear, job);
            return hardwareCommandResult;
        }

        public HardwareCommandResult LoadBin(
            List<Location> locs,
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            var hardwareCommandResult = ScheduleJob("load-bin", schedule, out job);
            if (hardwareCommandResult.Success)
            {
                PushLocs(locs, job);
                job.Push("LOCATION");
            }

            return hardwareCommandResult;
        }

        public HardwareCommandResult Vend(
            string[] ids,
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            var hardwareCommandResult = ScheduleJob("vend", schedule, out job);
            if (hardwareCommandResult.Success)
            {
                PushArray(ids, job);
                job.Push("BY-ID");
            }

            return hardwareCommandResult;
        }

        public HardwareCommandResult Vend(
            List<Location> locations,
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            var hardwareCommandResult = ScheduleJob("vend", schedule, out job);
            if (hardwareCommandResult.Success)
            {
                PushLocs(locations, job);
                job.Push("BY-LOCATION");
            }

            return hardwareCommandResult;
        }

        public HardwareCommandResult GetBarcodeVMZPosition(
            string[] idsToQuery,
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            var barcodeVmzPosition = ScheduleJob("get-barcode-vmz-location", schedule, out job);
            if (!barcodeVmzPosition.Success)
                return barcodeVmzPosition;
            PushArray(idsToQuery, job);
            return barcodeVmzPosition;
        }

        public HardwareCommandResult VMZDetail(HardwareJobSchedule schedule, out HardwareJob job)
        {
            return ScheduleJob("vmz-details", schedule, out job);
        }

        public HardwareCommandResult VMZMerchSummary(HardwareJobSchedule schedule, out HardwareJob job)
        {
            return ScheduleJob("vmz-merch-summary", schedule, out job);
        }

        public HardwareCommandResult FileMarkerDisk(
            int deck,
            int slot,
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            var hardwareCommandResult = ScheduleJob("file-marker-disk", schedule, out job);
            if (hardwareCommandResult.Success)
            {
                job.Push(slot);
                job.Push(deck);
            }

            return hardwareCommandResult;
        }

        public HardwareCommandResult MerchandizeClearAndOffset(
            int deck,
            int slot,
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            var hardwareCommandResult = ScheduleJob("merch-clear-and-offset", schedule, out job);
            if (hardwareCommandResult.Success)
            {
                job.Push(slot);
                job.Push(deck);
            }

            return hardwareCommandResult;
        }

        public HardwareCommandResult NotifyMaintModeState(bool goingIntoMM)
        {
            return ExecuteCommand(string.Format("SERVICE mm-status status: '{0}'", goingIntoMM.ToString()));
        }

        public HardwareCommandResult GetNonThinsInVMZ(HardwareJobSchedule schedule, out HardwareJob job)
        {
            return ScheduleJob("get-nonthins-in-vmz", schedule, out job);
        }

        public HardwareCommandResult GetBarcodesInBin(HardwareJobSchedule schedule, out HardwareJob job)
        {
            return ScheduleJob("get-barcodes-in-bin", schedule, out job);
        }

        public HardwareCommandResult HardSync(
            SyncRange range,
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            var hardwareCommandResult = ScheduleJob("sync", schedule, out job);
            if (hardwareCommandResult.Success)
                job.Push(range.Slots.End, range.EndDeck, range.Slots.Start, range.StartDeck);
            return hardwareCommandResult;
        }

        public HardwareCommandResult FieldInsertFraudWithCheck(
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            return ScheduleJob("field-insert-with-fraud-check", schedule, out job);
        }

        public HardwareCommandResult FraudHardwarePost(
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            return ScheduleJob("fraud-sensor-post-test", schedule, out job);
        }

        public HardwareCommandResult ReadFraudDisc(HardwareJobSchedule schedule, out HardwareJob job)
        {
            return ScheduleJob("read-fraud-disc", schedule, out job);
        }

        public HardwareCommandResult Return(HardwareJobSchedule schedule, out HardwareJob job)
        {
            return ScheduleJob("return", schedule, out job);
        }

        public HardwareCommandResult SoftSync(HardwareJobSchedule schedule, out HardwareJob job)
        {
            return ScheduleJob("soft-sync", schedule, out job);
        }

        public HardwareCommandResult QlmUnload(HardwareJobSchedule schedule, out HardwareJob job)
        {
            return ScheduleJob("qlm-unload", schedule, out job);
        }

        public HardwareCommandResult CheckPicker(HardwareJobSchedule schedule, out HardwareJob job)
        {
            return ScheduleJob("check-picker", schedule, out job);
        }

        public HardwareCommandResult GetMachineInfo(HardwareJobSchedule schedule, out HardwareJob job)
        {
            return ScheduleJob("get-machine-info", schedule, out job);
        }

        public HardwareCommandResult GetHardwareStatus(
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            return ScheduleJob("hardware-status", schedule, out job);
        }

        public HardwareCommandResult QlmUnloadAndThin(
            string[] ids,
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            var hardwareCommandResult = ScheduleJob("unload-thin", schedule, out job);
            if (!hardwareCommandResult.Success)
                return hardwareCommandResult;
            PushArray(ids, job);
            return hardwareCommandResult;
        }

        public HardwareCommandResult VendUnknown(
            int deck,
            int slot,
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            var hardwareCommandResult = ScheduleJob("vend-unknown", schedule, out job);
            if (hardwareCommandResult.Success)
            {
                job.Push(slot);
                job.Push(deck);
            }

            return hardwareCommandResult;
        }

        public HardwareCommandResult ReturnUnknown(
            HardwareJobSchedule schedule,
            string returnTime,
            out HardwareJob job)
        {
            var hardwareCommandResult = ScheduleJob("return-unknown", schedule, out job);
            if (hardwareCommandResult.Success)
                job.Push(returnTime);
            return hardwareCommandResult;
        }

        public HardwareCommandResult PreposVend(
            HardwareJobSchedule schedule,
            string[] ids,
            out HardwareJob job)
        {
            var hardwareCommandResult = ScheduleJob("vend", schedule, out job);
            if (hardwareCommandResult.Success)
            {
                PushArray(ids, job);
                job.Push("PREPOSITION-PICKER");
            }

            return hardwareCommandResult;
        }

        public HardwareCommandResult ExchangerStatus(HardwareJobSchedule schedule, out HardwareJob job)
        {
            return ScheduleJob("air-exchanger-status", schedule, out job);
        }

        public HardwareCommandResult MarkBarcodesUnknown(
            HardwareJobSchedule s,
            string[] barcodes,
            out HardwareJob job)
        {
            var hardwareCommandResult = ScheduleJob("mark-barcodes-unknown", s, out job);
            if (!hardwareCommandResult.Success)
                return hardwareCommandResult;
            PushArray(barcodes, job);
            return hardwareCommandResult;
        }

        public HardwareCommandResult MarkLocationsUnknown(
            HardwareJobSchedule s,
            List<Location> locs,
            out HardwareJob job)
        {
            var hardwareCommandResult = ScheduleJob("mark-locations-unknown", s, out job);
            if (!hardwareCommandResult.Success)
                return hardwareCommandResult;
            PushLocs(locs, job);
            return hardwareCommandResult;
        }

        public HardwareCommandResult ResetTouchscreenController(
            HardwareJobSchedule s,
            out HardwareJob job)
        {
            return ScheduleJob("reset-touchscreen-controller", s, out job);
        }

        public HardwareCommandResult GetConfiguration(string name)
        {
            return ExecuteCommand(string.Format("CONFIG get name: '{0}'", name));
        }

        public HardwareCommandResult SetConfiguration(string name, string xmlConfig)
        {
            var inputArray = CompressionAlgorithm.GetAlgorithm(CompressionType.LZMA)
                .Compress(Encoding.ASCII.GetBytes(xmlConfig));
            return ExecuteCommand(string.Format("CONFIG set name: '{0}' data: '{1}'", name,
                ByteHelper.ToBase64(inputArray)));
        }

        public HardwareCommandResult LoadConfiguration(string path)
        {
            return ExecuteCommand(string.Format("CONFIG load {0}",
                path != null ? "path: '" + path + "'" : (object)string.Empty));
        }

        public HardwareCommandResult SaveConfiguration(string path)
        {
            return ExecuteCommand(string.Format("CONFIG save {0}",
                path != null ? "path: '" + path + "'" : (object)string.Empty));
        }

        public HardwareCommandResult RestoreConfiguration(string path)
        {
            return ExecuteCommand(string.Format("CONFIG restore {0}",
                path != null ? "path: '" + path + "'" : (object)string.Empty));
        }

        public HardwareCommandResult GetInventoryState()
        {
            return ExecuteCommand("CONFIG get-inventory-state");
        }

        public HardwareCommandResult SetInventoryState(string xml)
        {
            return ExecuteCommand(string.Format("CONFIG set-inventory-state data: '{0}'",
                ByteHelper.ToBase64(CompressionAlgorithm.GetAlgorithm(CompressionType.LZMA)
                    .Compress(Encoding.ASCII.GetBytes(xml)))));
        }

        public HardwareCommandResult GetProgramScript(string programName)
        {
            return ExecuteCommand(string.Format("PROGRAM get name: '{0}'", programName));
        }

        public HardwareCommandResult SetProgramScript(string programName, string script)
        {
            var inputArray = CompressionAlgorithm.GetAlgorithm(CompressionType.LZMA)
                .Compress(Encoding.ASCII.GetBytes(script));
            return ExecuteCommand(string.Format("PROGRAM set name: '{0}' data: '{1}'", programName,
                ByteHelper.ToBase64(inputArray)));
        }

        public HardwareCommandResult SetProgramRequiresClientConnection(
            string programName,
            bool requiresClientConnection)
        {
            return ExecuteCommand(string.Format("PROGRAM properties name: '{0}' requires-client-connection: {1}",
                programName, requiresClientConnection));
        }

        public HardwareCommandResult ResumeJobs(HardwareJob[] jobs)
        {
            var hardwareCommandResult1 = new HardwareCommandResult();
            foreach (var job in jobs)
            {
                var hardwareCommandResult2 = job.Resume();
                if (!hardwareCommandResult2.Success && hardwareCommandResult2.Errors.Count > 0)
                    hardwareCommandResult1.Errors.AddRange(hardwareCommandResult2.Errors);
            }

            return hardwareCommandResult1;
        }

        public HardwareCommandResult SuspendJobsBelowPriority(
            HardwareJobPriority priority,
            out HardwareJob[] suspendedJobs)
        {
            var hardwareJobList = new List<HardwareJob>();
            var jobs1 = (HardwareJob[])null;
            var hardwareCommandResult1 = new HardwareCommandResult();
            var jobs2 = GetJobs(out jobs1);
            if (!jobs2.Success)
            {
                if (jobs2.Errors.Count > 0)
                    hardwareCommandResult1.Errors.AddRange(jobs2.Errors);
                suspendedJobs = hardwareJobList.ToArray();
                return hardwareCommandResult1;
            }

            foreach (var hardwareJob in jobs1)
                if ((!(hardwareJob.ID != Constants.ExecutionContexts.ImmediateModeContext) ? 0 :
                        hardwareJob.Status == HardwareJobStatus.Pending ? 1 :
                        hardwareJob.Status == HardwareJobStatus.Running ? 1 : 0) != 0 &&
                    hardwareJob.Priority > priority)
                {
                    var hardwareCommandResult2 = hardwareJob.Suspend();
                    if (hardwareCommandResult2.Success)
                        hardwareJobList.Add(hardwareJob);
                    else if (hardwareCommandResult2.Errors.Count > 0)
                        hardwareCommandResult1.Errors.AddRange(hardwareCommandResult2.Errors);
                }

            suspendedJobs = hardwareJobList.ToArray();
            return hardwareCommandResult1;
        }

        public HardwareCommandResult SuspendAll()
        {
            return ExecuteCommand("JOB suspend-all");
        }

        public HardwareCommandResult ResumeAll()
        {
            return ExecuteCommand("JOB resume-all");
        }

        public HardwareCommandResult CollectGarbage(bool force)
        {
            return ExecuteCommand(string.Format("JOB collect-garbage force: {0}", force ? "true" : (object)"false"));
        }

        public HardwareCommandResult TrashJob(string jobId)
        {
            return ExecuteCommand(string.Format("JOB trash job: {0}", jobId));
        }

        public HardwareCommandResult GetJob(string jobId, out HardwareJob job)
        {
            job = null;
            var job1 = ExecuteCommand(string.Format("JOB get job: '{0}'", jobId));
            if (job1.Success)
                job = HardwareJob.Parse(this, job1.CommandMessages[0]);
            return job1;
        }

        public HardwareCommandResult GetJobs(out HardwareJob[] jobs)
        {
            var jobs1 = ExecuteCommand("JOB list");
            jobs = jobs1.Success
                ? jobs1.CommandMessages.ConvertAll(each => HardwareJob.Parse(this, each)).ToArray()
                : new HardwareJob[0];
            return jobs1;
        }

        public HardwareCommandResult CompileProgram(
            string path,
            string programName,
            bool requiresClientConnection)
        {
            return ExecuteCommand(string.Format(
                "PROGRAM compile path: @'{0}' name: '{1}' requires-client-connection: {2}", path, programName,
                requiresClientConnection));
        }

        public HardwareCommandResult RemoveProgram(string programName)
        {
            return ExecuteCommand(string.Format("PROGRAM remove name: '{0}'", programName));
        }

        public HardwareCommandResult ScheduleJob(
            string programName,
            string label,
            bool enableDebugging,
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            var command = string.Format("JOB schedule name: '{0}' priority: {1}", programName, schedule.Priority);
            if (enableDebugging)
                command += string.Format(" debugging: {0}", enableDebugging);
            if (schedule.StartTime.HasValue)
                command += string.Format(" startTime: '{0}'", schedule.StartTime);
            if (label != null)
                command += string.Format(" label: '{0}'", label);
            job = null;
            var hardwareCommandResult = ExecuteCommand(command);
            if (hardwareCommandResult.Success)
                job = HardwareJob.Parse(this, hardwareCommandResult.CommandMessages[0]);
            return hardwareCommandResult;
        }

        public HardwareCommandResult GetPrograms()
        {
            return ExecuteCommand("PROGRAM list");
        }

        public HardwareCommandResult ExecuteImmediate(
            string statement,
            int? timeout,
            out HardwareJob job)
        {
            job = null;
            var hardwareCommandResult =
                ExecuteCommand(string.Format("JOB execute-immediate statement: '{0}'", statement),
                    timeout.HasValue ? timeout.Value : 30000);
            if (hardwareCommandResult.Success)
                job = HardwareJob.Parse(this, hardwareCommandResult.CommandMessages[0]);
            return hardwareCommandResult;
        }

        public HardwareCommandResult ExecuteImmediate(string statement, out HardwareJob job)
        {
            job = null;
            var hardwareCommandResult =
                ExecuteCommand(string.Format("JOB execute-immediate statement: '{0}'", statement));
            if (hardwareCommandResult.Success)
                job = HardwareJob.Parse(this, hardwareCommandResult.CommandMessages[0]);
            return hardwareCommandResult;
        }

        public HardwareCommandResult GetSchedulerStatus(out string status)
        {
            status = null;
            var schedulerStatus = ExecuteCommand("JOB scheduler-status");
            if (schedulerStatus.Success)
                status = schedulerStatus.CommandMessages[0];
            return schedulerStatus;
        }

        public HardwareCommandResult ExecuteImmediateProgram(byte[] bytes, out HardwareJob job)
        {
            job = null;
            var hardwareCommandResult = ExecuteCommand(string.Format("JOB execute-immediate-base64 statement: '{0}'",
                ByteHelper.ToBase64(bytes)));
            if (hardwareCommandResult.Success)
                job = HardwareJob.Parse(this, hardwareCommandResult.CommandMessages[0]);
            return hardwareCommandResult;
        }

        public HardwareCommandResult GetInitStatus(out string status)
        {
            status = null;
            var initStatus = ExecuteCommand("JOB init-status");
            if (initStatus.Success)
                status = initStatus.CommandMessages[0];
            return initStatus;
        }

        public HardwareCommandResult GetKioskID(out string id)
        {
            id = "UNKNOWN";
            var kioskId = ExecuteCommand("SERVICE get-kiosk-id");
            if (kioskId.Success)
                id = kioskId.CommandMessages[0];
            return kioskId;
        }

        public HardwareCommandResult GetQuickReturnStatus(out string status)
        {
            status = null;
            var quickReturnStatus = ExecuteCommand("JOB quick-return-status");
            if (quickReturnStatus.Success)
                status = quickReturnStatus.CommandMessages[0];
            return quickReturnStatus;
        }

        public HardwareCommandResult ServiceUnknownSync()
        {
            return ExecuteCommand("SERVICE sync-unknowns");
        }

        public HardwareCommandResult ServiceEmptySync()
        {
            return ExecuteCommand("SERVICE sync-empty");
        }

        public HardwareCommandResult FullSync()
        {
            return ExecuteCommand("SERVICE full-sync");
        }

        public HardwareCommandResult TestSomeData(int dataSize)
        {
            return ExecuteCommand(string.Format("ipctest test-ipc-xfer size: '{0}'", dataSize.ToString()));
        }

        public HardwareCommandResult GetKioskFunctionCheckData(
            out IList<IKioskFunctionCheckData> sessions)
        {
            sessions = new List<IKioskFunctionCheckData>();
            using (var functionCheckExecutor = new KioskFunctionCheckExecutor(this))
            {
                functionCheckExecutor.Run();
                if (functionCheckExecutor.ScheduleResult.Success)
                    foreach (var session in functionCheckExecutor.Sessions)
                        sessions.Add(session);
                return functionCheckExecutor.ScheduleResult;
            }
        }

        public HardwareCommandResult ExecuteServiceCommand(string command)
        {
            return ExecuteCommand(command);
        }

        public HardwareCommandResult ExecuteServiceCommand(string command, int timeout)
        {
            return ExecuteCommand(command, timeout);
        }

        public HardwareCommandResult QueryHealth(string outputFile)
        {
            return ExecuteCommand(string.Format("HEALTH check path: @'{0}'", outputFile));
        }

        internal HardwareCommandResult ExecuteCommand(string command)
        {
            return ClientCommand<HardwareCommandResult>.ExecuteCommand(Protocol, command);
        }

        internal HardwareCommandResult ExecuteCommand(string command, int timeout)
        {
            return ClientCommand<HardwareCommandResult>.ExecuteCommand(Protocol, timeout, command);
        }

        internal IIpcClientSession GetSession()
        {
            return ClientSessionFactory.GetClientSession(Protocol);
        }

        private HardwareCommandResult ScheduleJob(
            string name,
            HardwareJobSchedule schedule,
            out HardwareJob job)
        {
            return ScheduleJob(name, null, false, schedule, out job);
        }

        private void PushLocs(List<Location> locs, HardwareJob job)
        {
            if (locs == null || locs.Count == 0)
            {
                job.Push(0);
            }
            else
            {
                foreach (var loc in locs)
                {
                    job.Push(loc.Slot);
                    job.Push(loc.Deck);
                }

                job.Push(locs.Count);
            }
        }

        private void PushArray(string[] array, HardwareJob job)
        {
            if (array == null || array.Length == 0)
            {
                job.Push(0);
            }
            else
            {
                job.Push(array);
                job.Push(array.Length);
            }
        }
    }
}