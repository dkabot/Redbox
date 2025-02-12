using Redbox.HAL.Client;
using System;
using System.Collections.Generic;

namespace HALUtilities
{
  internal sealed class PrepositionTest : IDisposable
  {
    private readonly HardwareJob m_preposJob;

    public void Dispose()
    {
    }

    internal HardwareJob PreposJob => this.m_preposJob;

    internal PrepositionTest(int items, HardwareService s, IConsole console)
    {
      List<string> stringList = new List<string>();
      using (RandomInventorySelector inventorySelector = new RandomInventorySelector(s))
      {
        for (int index = 0; index < items; ++index)
        {
          IInventoryLocation inventoryLocation = inventorySelector.Select();
          if (inventoryLocation != null)
            stringList.Add(inventoryLocation.Matrix);
          else
            break;
        }
        HardwareService hardwareService = s;
        HardwareJobSchedule schedule = new HardwareJobSchedule();
        schedule.Priority = HardwareJobPriority.High;
        string[] array = stringList.ToArray();
        if (!hardwareService.PreposVend(schedule, array, out this.m_preposJob).Success)
          throw new Exception("Can't schedule job.");
        this.m_preposJob.Pend();
      }
    }
  }
}
