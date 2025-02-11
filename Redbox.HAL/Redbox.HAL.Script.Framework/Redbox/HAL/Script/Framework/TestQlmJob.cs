using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "qlm-test-sync", Operand = "QLM-TEST-SYNC")]
    internal sealed class TestQlmJob : NativeJobAdapter
    {
        private readonly int[] TargetSlots = new int[4]
        {
            1,
            20,
            41,
            62
        };

        internal TestQlmJob(ExecutionResult r, ExecutionContext c)
            : base(r, c)
        {
        }

        protected override void ExecuteInner()
        {
            var log = ApplicationLog.ConfigureLog(Context, false, "QLM", true, null);
            log.Write("Start QLM test sync.");
            if (ControllerConfiguration.Instance.IsVMZMachine)
            {
                log.Write("The kiosk is not configured with a QLM.");
                Context.CreateInfoResult("SyncFailure", "Kiosk is not configured for a QLM.");
                AddError("The kiosk is not configured for a QLM.");
            }
            else
            {
                var service1 = ServiceLocator.Instance.GetService<IDecksService>();
                if (service1.QlmDeck == null)
                {
                    log.Write("The kiosk is not configured with a QLM.");
                    Context.CreateInfoResult("SyncFailure", "Kiosk is not configured for a QLM.");
                    AddError("The kiosk is not configured for a QLM.");
                }
                else
                {
                    var qlmStatus = ControlSystem.GetQlmStatus();
                    if (QlmStatus.Engaged != qlmStatus)
                    {
                        log.Write(string.Format("The QLM status instruction returned {0}", qlmStatus.ToString()));
                        Context.CreateInfoResult("SyncFailure", "The QLM is not engaged.");
                        AddError("There is no QLM in the bay.");
                    }
                    else
                    {
                        var sensorReadResult = ControlSystem.ReadPickerSensors();
                        if (!sensorReadResult.Success)
                        {
                            Context.CreateInfoResult("HardwareError",
                                string.Format("Read picker sensors returned error {0}", sensorReadResult.Error));
                        }
                        else if (!sensorReadResult.IsFull)
                        {
                            log.Write("There is not a disk in the picker for a test - bailing.");
                            AddError("There is no disk in the picker.");
                            Context.CreateInfoResult("SyncFailure", "There is no disk in the picker.");
                        }
                        else
                        {
                            var service2 = ServiceLocator.Instance.GetService<IRuntimeService>();
                            var number = service1.QlmDeck.Number;
                            foreach (var targetSlot in TargetSlots)
                            {
                                if (!MoveAndPut(number, targetSlot, "UNKNOWN"))
                                {
                                    Context.CreateResult("SyncFailure", "Filing the disk to the QLM failed.", number,
                                        targetSlot, null, new DateTime?(), null);
                                    AddError("Unable to PUT disk.");
                                    return;
                                }

                                service2.Wait(1000);
                                if (!MoveAndGet(number, targetSlot))
                                {
                                    Context.CreateResult("SyncFailure", "Filing the disk to the QLM failed.", number,
                                        targetSlot, null, new DateTime?(), null);
                                    AddError("Failure to get disk.");
                                    return;
                                }

                                service2.Wait(500);
                            }

                            log.Write("The sync completed successfully.");
                            var num = (int)MotionService.MoveVend(MoveMode.Get, log);
                        }
                    }
                }
            }
        }

        private void HandleError(int deck, int slot)
        {
            Context.CreateResult("SyncFailure", "Retrieving the disk from the QLM failed.", deck, slot, null,
                new DateTime?(), null);
            AddError("Job error.");
        }
    }
}