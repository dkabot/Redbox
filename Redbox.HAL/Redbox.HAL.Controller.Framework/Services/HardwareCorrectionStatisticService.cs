using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework.Services
{
    internal sealed class HardwareCorrectionStatisticService : IHardwareCorrectionStatisticService
    {
        private readonly IDataTableService DataTableService;
        private readonly IDataTable<IHardwareCorrectionStatistic> StatsTable;
        private readonly ITableTypeFactory TypeFactory;

        internal HardwareCorrectionStatisticService(IDataTableService dts)
        {
            DataTableService = dts;
            TypeFactory = ServiceLocator.Instance.GetService<ITableTypeFactory>();
            StatsTable = DataTableService.GetTable<IHardwareCorrectionStatistic>();
            if (StatsTable.Exists)
                return;
            LogHelper.Instance.Log(
                "[HardwareCorrectionStatisticsService] Stats table doesn't exist; create returned {0}",
                StatsTable.Create() ? "SUCCESS" : (object)"FAILURE");
        }

        public bool Insert(HardwareCorrectionEventArgs args, IExecutionContext context)
        {
            return Insert(args.Statistic, context, args.CorrectionOk);
        }

        public bool Insert(
            HardwareCorrectionStatistic type,
            IExecutionContext context,
            bool correctionOk)
        {
            return Insert(type, context, correctionOk, DateTime.Now);
        }

        public bool Insert(
            HardwareCorrectionStatistic type,
            IExecutionContext context,
            bool correctionOk,
            DateTime ts)
        {
            return !ControllerConfiguration.Instance.TrackHardwareCorrections ||
                   StatsTable.Insert(TypeFactory.NewStatistic(type, context.ProgramName, correctionOk, ts));
        }

        public bool RemoveAll()
        {
            try
            {
                StatsTable.DeleteAll();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("[HardwareCorrectionStatisticService] Failed to delete all correction entries.",
                    ex);
                return false;
            }
        }

        public bool RemoveAll(HardwareCorrectionStatistic stat)
        {
            try
            {
                var stats = GetStats(stat);
                using (new DisposeableList<IHardwareCorrectionStatistic>(stats))
                {
                    return StatsTable.Delete(stats);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("[HardwareCorrectionStatisticService] Failed to delete HardwareCorrection info.",
                    ex);
                return false;
            }
        }

        public List<IHardwareCorrectionStatistic> GetStats()
        {
            return StatsTable.LoadEntries();
        }

        public List<IHardwareCorrectionStatistic> GetStats(HardwareCorrectionStatistic key)
        {
            var list = StatsTable.LoadEntries();
            using (new DisposeableList<IHardwareCorrectionStatistic>(list))
            {
                return list.FindAll(each => each.Statistic == key);
            }
        }
    }
}