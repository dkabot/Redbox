using Redbox.HAL.Client;
using Redbox.HAL.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;

namespace HALUtilities
{
  internal sealed class VendTest : IDisposable
  {
    private readonly AutoResetEvent Waiter = new AutoResetEvent(false);
    private readonly HardwareService Service;
    private readonly ClientHelper Helper;
    private readonly int Iterations;

    public void Dispose() => this.Helper.Dispose();

    internal VendTest(int iter, HardwareService service)
    {
      this.Iterations = iter;
      this.Helper = new ClientHelper(service);
      this.Service = service;
    }

    internal void Execute(IConsole console, int? startAt)
    {
      if (startAt.HasValue && startAt.Value != -1)
      {
        new RedboxTimer("Vend Test", new ElapsedEventHandler(this.TimerFired)).ScheduleAtNext(startAt.Value, 0);
        this.Waiter.WaitOne();
      }
      console.WriteLine("Starting test at {0} on {1}", (object) DateTime.Now.ToShortTimeString(), (object) DateTime.Now.ToShortDateString());
      List<Location> locationList = new List<Location>();
      for (int index = 0; index < this.Iterations; ++index)
      {
        console.WriteLine("Starting iteration {0}", (object) index);
        using (RandomInventorySelector inventorySelector = new RandomInventorySelector(this.Service))
        {
          IInventoryLocation inventoryLocation;
          do
          {
            inventoryLocation = inventorySelector.Select();
            if (inventoryLocation != null)
            {
              locationList.Add(inventoryLocation.Location);
              HardwareService service = this.Service;
              List<Location> locations = locationList;
              HardwareJobSchedule schedule = new HardwareJobSchedule();
              schedule.Priority = HardwareJobPriority.High;
              HardwareJob job;
              if (!service.Vend(locations, schedule, out job).Success)
              {
                Console.WriteLine("Failed to schedule vend job.");
                return;
              }
              job.Pend();
              HardwareJobStatus endStatus;
              this.Helper.WaitForJob(job, out endStatus);
              locationList.Clear();
              if (endStatus != HardwareJobStatus.Completed)
              {
                console.WriteLine("Vend job {0} failed with status {1}.", (object) job.ID, (object) endStatus.ToString());
                return;
              }
              if (!job.GetResults(out ProgramResult[] _).Success)
              {
                console.WriteLine("Failed to get program results.");
                return;
              }
            }
            else
              break;
          }
          while (inventoryLocation != null);
        }
      }
    }

    private void TimerFired(object source, ElapsedEventArgs e) => this.Waiter.Set();
  }
}
