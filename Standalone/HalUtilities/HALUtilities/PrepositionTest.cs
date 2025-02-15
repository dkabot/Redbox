using System;
using System.Collections.Generic;
using Redbox.HAL.Client;

namespace HALUtilities
{
    internal sealed class PrepositionTest : IDisposable
    {
        private readonly HardwareJob m_preposJob;

        internal PrepositionTest(int items, HardwareService s, IConsole console)
        {
            var stringList = new List<string>();
            using (var inventorySelector = new RandomInventorySelector(s))
            {
                for (var index = 0; index < items; ++index)
                {
                    var inventoryLocation = inventorySelector.Select();
                    if (inventoryLocation != null)
                        stringList.Add(inventoryLocation.Matrix);
                    else
                        break;
                }

                var hardwareService = s;
                var schedule = new HardwareJobSchedule();
                schedule.Priority = HardwareJobPriority.High;
                var array = stringList.ToArray();
                if (!hardwareService.PreposVend(schedule, array, out m_preposJob).Success)
                    throw new Exception("Can't schedule job.");
                m_preposJob.Pend();
            }
        }

        internal HardwareJob PreposJob => m_preposJob;

        public void Dispose()
        {
        }
    }
}