using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Redbox.HAL.Client;
using Redbox.HAL.Core;

namespace HALUtilities
{
    internal sealed class VendTest : IDisposable
    {
        private readonly ClientHelper Helper;
        private readonly int Iterations;
        private readonly HardwareService Service;
        private readonly AutoResetEvent Waiter = new AutoResetEvent(false);

        internal VendTest(int iter, HardwareService service)
        {
            Iterations = iter;
            Helper = new ClientHelper(service);
            Service = service;
        }

        public void Dispose()
        {
            Helper.Dispose();
        }

        internal void Execute(IConsole console, int? startAt)
        {
            if (startAt.HasValue && startAt.Value != -1)
            {
                new RedboxTimer("Vend Test", TimerFired).ScheduleAtNext(startAt.Value, 0);
                Waiter.WaitOne();
            }

            console.WriteLine("Starting test at {0} on {1}", DateTime.Now.ToShortTimeString(),
                DateTime.Now.ToShortDateString());
            var locationList = new List<Location>();
            for (var index = 0; index < Iterations; ++index)
            {
                console.WriteLine("Starting iteration {0}", index);
                using (var inventorySelector = new RandomInventorySelector(Service))
                {
                    IInventoryLocation inventoryLocation;
                    do
                    {
                        inventoryLocation = inventorySelector.Select();
                        if (inventoryLocation != null)
                        {
                            locationList.Add(inventoryLocation.Location);
                            var service = Service;
                            var locations = locationList;
                            var schedule = new HardwareJobSchedule();
                            schedule.Priority = HardwareJobPriority.High;
                            HardwareJob job;
                            if (!service.Vend(locations, schedule, out job).Success)
                            {
                                Console.WriteLine("Failed to schedule vend job.");
                                return;
                            }

                            job.Pend();
                            HardwareJobStatus endStatus;
                            Helper.WaitForJob(job, out endStatus);
                            locationList.Clear();
                            if (endStatus != HardwareJobStatus.Completed)
                            {
                                console.WriteLine("Vend job {0} failed with status {1}.", job.ID, endStatus.ToString());
                                return;
                            }

                            if (!job.GetResults(out var _).Success)
                            {
                                console.WriteLine("Failed to get program results.");
                                return;
                            }
                        }
                        else
                        {
                            break;
                        }
                    } while (inventoryLocation != null);
                }
            }
        }

        private void TimerFired(object source, ElapsedEventArgs e)
        {
            Waiter.Set();
        }
    }
}