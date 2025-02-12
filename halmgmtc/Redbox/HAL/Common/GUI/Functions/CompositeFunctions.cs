using System;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;

namespace Redbox.HAL.Common.GUI.Functions
{
    public static class CompositeFunctions
    {
        public static void VendDisk(
            object sender,
            HardwareService service,
            ButtonAspectsManager manager,
            OutputBox box)
        {
            using (manager.MakeAspect(sender))
            {
                using (var vendDiskInPickerJob = new VendDiskInPickerJob(service))
                {
                    vendDiskInPickerJob.Run();
                    foreach (var result in vendDiskInPickerJob.Results)
                    {
                        box.Write(result.Code);
                        if (result.Code.Equals("ItemVended", StringComparison.CurrentCultureIgnoreCase))
                        {
                            box.Write("Disk was taken.");
                            return;
                        }

                        if (result.Code.Equals("HardwareError", StringComparison.CurrentCultureIgnoreCase))
                            box.Write("There was a HW error: " + result.Message);
                    }

                    box.Write("Disk not taken - pulled back in.");
                }
            }
        }

        public static bool GetItem(
            object sender,
            ButtonAspectsManager manager,
            int deck,
            int slot,
            OutputBox box,
            HardwareService service)
        {
            using (manager.MakeAspect(sender))
            {
                return GetItem(deck, slot, box, service);
            }
        }

        public static bool GetItem(int deck, int slot, OutputBox box, HardwareService service)
        {
            using (var getAndCenterResult = new GetAndCenterResult(service, deck, slot, false))
            {
                getAndCenterResult.Run();
                box.Write("GetDVD ended with status {0}", getAndCenterResult.EndStatus.ToString());
                var num = 0;
                foreach (var result in getAndCenterResult.Results)
                {
                    if (result.Code == "ErrorMessage")
                        ++num;
                    box.Write(result.Message);
                }

                return num == 0;
            }
        }

        public static bool PutItem(
            object sender,
            ButtonAspectsManager manager,
            HardwareService service,
            int deck,
            int slot,
            OutputBox box)
        {
            using (manager.MakeAspect(sender))
            {
                return PutItem(service, deck, slot, box);
            }
        }

        public static bool PutItem(HardwareService service, int deck, int slot, OutputBox box)
        {
            using (var inLocationResult = new PutInLocationResult(service, deck, slot))
            {
                inLocationResult.Run();
                var num = 0;
                foreach (var result in inLocationResult.Results)
                {
                    if (result.Code == "JobError")
                        ++num;
                    box.Write(result.Message);
                }

                return num == 0;
            }
        }
    }
}