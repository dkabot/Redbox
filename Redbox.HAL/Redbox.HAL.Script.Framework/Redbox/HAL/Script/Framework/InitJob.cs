using System;
using System.Collections.Generic;
using System.Diagnostics;
using Redbox.DirectShow;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "init", Operand = "INIT")]
    internal sealed class InitJob : NativeJobAdapter
    {
        internal InitJob(ExecutionResult r, ExecutionContext c)
            : base(r, c)
        {
        }

        protected override void ExecuteInner()
        {
            var service = ServiceLocator.Instance.GetService<IRuntimeService>();
            var applicationLog = ApplicationLog.ConfigureLog(Context, false, "init", true, null);
            applicationLog.Write("Init start.");
            applicationLog.Write("RESET controller boards.");
            var symbolValue = Context.GetSymbolValue("__EE_BOOTSTRAP_INIT", Result.Errors);
            var flag = false;
            if (Result.Errors.Count == 0)
                flag = (bool)symbolValue;
            else
                Result.Errors.Clear();
            if (flag)
            {
                if (ControllerConfiguration.Instance.DisableAutoExposureAtInit)
                    ExposureFixer.ResetCameraProperties();
                service.Wait(ControllerConfiguration.Instance.MinInitWakeupTime);
                LogHelper.Instance.Log("Wait for COM ports complete.");
            }

            try
            {
                if (ServiceLocator.Instance.GetService<IControlSystemService>().IsInitialized)
                {
                    ControlSystem.Shutdown();
                    service.Wait(2000);
                }

                if (!ControlSystem.Initialize().Success)
                {
                    applicationLog.Write("Reset of the boards timed out.");
                    LogHelper.Instance.Log("RESET instruction returned timeout; maybe communication error?",
                        LogEntryType.Error);
                    Context.Send("RESET instruction returned timeout.");
                    Context.CreateInfoResult("RESET", "RESET instruction timed out.");
                    AddError("RESET instruction returned timeout.");
                }
                else
                {
                    ServiceLocator.Instance.GetService<IMotionControlService>().Initialize();
                    applicationLog.Write("RESET completed successfully; get controller board VERSION stamps.");
                    Context.Send("GET VERSION INFO");
                    var revision = ControlSystem.GetRevision();
                    if (!revision.Success)
                    {
                        applicationLog.Write("The Version instruction timed out.");
                        LogHelper.Instance.Log("VERSION instruction returned timeout.", LogEntryType.Error);
                        Context.Send("VERSION instruction returned timeout.");
                        Context.CreateInfoResult("VERSION", "VERSION instruction timed out.");
                        AddError("VERSION instruction returned timeout.");
                    }
                    else
                    {
                        Array.ForEach(revision.Responses,
                            each => Context.CreateInfoResult("VERSION INFO", each.Version));
                        Context.CreateInfoResult("VERSION INFO", revision.Revision);
                        Context.Send("TEST TRACK");
                        ControlSystem.TrackOpen();
                        service.Wait(200);
                        ControlSystem.TrackClose();
                        service.Wait(200);
                        if (!ControlSystem.TrackOpen().Success)
                        {
                            Context.CreateInfoResult("TRACK OPEN FAILED", "The TRACK OPEN instruction timed out.");
                            AddError("TRACK OPEN instruction failed.");
                        }
                        else
                        {
                            service.Wait(200);
                            if (!ControlSystem.TrackClose().Success)
                            {
                                Context.CreateInfoResult("TRACK CLOSE FAILED",
                                    "The TRACK CLOSE instruction timed out.");
                                AddError("TRACK CLOSE instruction failed.");
                            }
                            else
                            {
                                service.Wait(200);
                                applicationLog.Write("Ensure the gripper is not obstructed.");
                                Context.Send("CHECK OBSTRUCTIONS");
                                var errorCodes1 = ServiceLocator.Instance.GetService<IControllerService>()
                                    .ClearGripper();
                                if (errorCodes1 != ErrorCodes.Success)
                                {
                                    if (MotionService.CurrentLocation != null)
                                        Context.CreateResult(StackEnd.Top, "PICKER OBSTRUCTED",
                                            "The picker is obstructed and cannot be moved.",
                                            MotionService.CurrentLocation.Deck, MotionService.CurrentLocation.Slot,
                                            null, new DateTime?(), null);
                                    AddError(string.Format(
                                        "The picker is obstructed and cannot be moved: clear gripper returned {0}.",
                                        errorCodes1.ToString()));
                                }
                                else
                                {
                                    var errorCodes2 = MotionService.InitAxes();
                                    if (errorCodes2 != ErrorCodes.Success)
                                    {
                                        var str = string.Format("Initializing the axes failed with code {0}",
                                            errorCodes2.ToString());
                                        Result.Errors.Add(Error.NewError("E999", "Init axes error.", str));
                                        applicationLog.WriteFormatted(str, LogEntryType.Error);
                                        Context.CreateInfoResult("InitError", str);
                                    }
                                    else
                                    {
                                        StartCamera();
                                        using (var checkPickerOperation = new CheckPickerOperation(Context))
                                        {
                                            if (checkPickerOperation.CheckPicker(Result, null) !=
                                                ErrorCodes.PickerEmpty)
                                            {
                                                AddError("Failed to put disk away.");
                                                return;
                                            }
                                        }

                                        applicationLog.Write("Move to the vend position.");
                                        Context.Send("MOVE TO VEND");
                                        var errorCodes3 = MotionService.MoveVend(MoveMode.Get, AppLog);
                                        if (errorCodes3 == ErrorCodes.Success)
                                            return;
                                        Context.CreateInfoResult(StackEnd.Top, errorCodes3.ToString().ToUpper(),
                                            "There was an error executing the MOVEVEND instruction.");
                                        AddError("The MOVEVEND instruction failed.");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                StartCamera();
                if (flag)
                    CheckShutdown(Context, service);
                applicationLog.Write("Initialization complete.");
                Context.Send("INIT STOP");
            }
        }

        private void CheckShutdown(ExecutionContext ctx, IRuntimeService runtimeService)
        {
            if (!ControllerConfiguration.Instance.TrackDirtyShutdown)
                return;
            var eventLog = Array.Find(EventLog.GetEventLogs(), each => each.Log == "System");
            if (eventLog == null)
                return;
            var list = new List<EventLogEntry>();
            using (new DisposeableList<EventLogEntry>(list))
            {
                foreach (EventLogEntry entry in eventLog.Entries)
                    if (entry.InstanceId == 41L || entry.InstanceId == 12L)
                        list.Add(entry);
                if (list.Count == 0)
                    return;
                list.Sort(new EventEntryComparer());
                var e = list[0];
                if (e.InstanceId == 41L)
                {
                    var service = ServiceLocator.Instance.GetService<IHardwareCorrectionStatisticService>();
                    var stats = service.GetStats(HardwareCorrectionStatistic.UnexpectedPowerLoss);
                    if (stats.Count > 0 &&
                        stats.Find(each => each.CorrectionTime == e.TimeGenerated && each.ProgramName == "init") !=
                        null)
                    {
                        LogHelper.Instance.Log(
                            "[init, {0}] Kiosk shutdown due to power event ( id = {1} timestamp = {2} ) - event exists.",
                            ctx.ID, e.InstanceId, e.TimeGenerated);
                    }
                    else
                    {
                        LogHelper.Instance.Log(
                            "[init, {0}] Kiosk shutdown due to power event ( id = {1} timestamp = {2} )", ctx.ID,
                            e.InstanceId, e.TimeGenerated);
                        service.Insert(HardwareCorrectionStatistic.UnexpectedPowerLoss, ctx, false, e.TimeGenerated);
                    }
                }
                else
                {
                    LogHelper.Instance.Log("[init, {0}] Kiosk shutdown was ok", ctx.ID);
                }
            }
        }

        private void StartCamera()
        {
            var configuredDevice = ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice();
            if (configuredDevice.IsConnected)
                return;
            var flag = configuredDevice.Start(true);
            if (!flag)
                flag = configuredDevice.Restart();
            LogHelper.Instance.WithContext("CAMERA START returned {0}", flag);
        }

        private class EventEntryComparer : IComparer<EventLogEntry>
        {
            public int Compare(EventLogEntry x, EventLogEntry y)
            {
                return y.TimeGenerated.CompareTo(x.TimeGenerated);
            }
        }
    }
}