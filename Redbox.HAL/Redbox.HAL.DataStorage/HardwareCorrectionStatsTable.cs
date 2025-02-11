using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage
{
    internal sealed class HardwareCorrectionStatsTable :
        AbstractVistaDBDataTable<IHardwareCorrectionStatistic>
    {
        internal HardwareCorrectionStatsTable(IDataTableDescriptor d)
            : base(d, "HardwareCorrection_v2")
        {
        }

        protected override List<IHardwareCorrectionStatistic> OnLoadEntries()
        {
            var list = new List<IHardwareCorrectionStatistic>();
            ExectuteSelectQuery(reader => list.Add(new HardwareCorrectionStat
            {
                ProgramName = reader.GetString(0),
                CorrectionTime = reader.GetDateTime(1),
                CorrectionOk = reader.GetBoolean(2),
                Statistic = (HardwareCorrectionStatistic)reader.GetInt32(3)
            }), string.Format("SELECT Job, CorrectionTime, Success, Type FROM [{0}]", Name));
            return list;
        }

        protected override string CreateStatement()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("CREATE TABLE [{0}] ", Name);
            stringBuilder.Append("( [Type] Int NOT NULL,");
            stringBuilder.Append(" [Job] VarChar(100) NOT NULL,");
            stringBuilder.Append(" [CorrectionTime] DateTime NOT NULL,");
            stringBuilder.Append(" [Success] Bit NOT NULL )");
            return stringBuilder.ToString();
        }

        protected override string UpdateStatement(IHardwareCorrectionStatistic obj)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("UPDATE {0} SET", Name);
            stringBuilder.AppendFormat(" Success = '{0}'", obj.CorrectionOk ? 1 : 0);
            stringBuilder.AppendFormat(" where Job = '{0}' and CorrectionTime = '{1}' and Type = '{2}'",
                obj.ProgramName, obj.CorrectionTime.ToString(), (int)obj.Statistic);
            return stringBuilder.ToString();
        }

        protected override string InsertStatement(IHardwareCorrectionStatistic p)
        {
            return string.Format(
                "INSERT INTO [{0}] (Type, Job, CorrectionTime, Success) VALUES ('{1}', '{2}', '{3}', '{4}')", Name,
                (int)p.Statistic, p.ProgramName, p.CorrectionTime, p.CorrectionOk ? 1 : 0);
        }

        protected override string DeleteStatement(IHardwareCorrectionStatistic obj)
        {
            return string.Format("delete from {0} where Type = '{1}' and CorrectionTime = '{2}' and Job = '{3}'", Name,
                (int)obj.Statistic, obj.CorrectionTime, obj.ProgramName);
        }
    }
}