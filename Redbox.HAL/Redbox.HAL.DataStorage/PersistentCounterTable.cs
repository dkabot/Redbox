using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage
{
    internal sealed class PersistentCounterTable : AbstractVistaDBDataTable<IPersistentCounter>
    {
        internal PersistentCounterTable(IDataTableDescriptor d)
            : base(d, "Counters")
        {
        }

        protected override string PrimaryKeyToken => "counters_PK";

        protected override string PrimaryKeyColumns => "( Name )";

        protected override List<IPersistentCounter> OnLoadEntries()
        {
            var list = new List<IPersistentCounter>();
            ExectuteSelectQuery(reader => list.Add(new PersistentCounter
            {
                Name = reader.GetString(0),
                Value = reader.GetInt32(1)
            }), string.Format("SELECT Name, Value FROM [{0}]", Name));
            return list;
        }

        protected override string UpdateStatement(IPersistentCounter item)
        {
            return string.Format("UPDATE {0} SET VALUE = '{1}' WHERE NAME = '{2}'", Name, item.Value, item.Name);
        }

        protected override string InsertStatement(IPersistentCounter item)
        {
            return string.Format("INSERT INTO [{0}] (Name, Value) VALUES ('{1}', '{2}')", Name, item.Name, item.Value);
        }

        protected override string DeleteStatement(IPersistentCounter obj)
        {
            return string.Format("delete from {0} where Name = '{1}'", Name, obj.Name);
        }

        protected override string CreateStatement()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("CREATE TABLE [{0}] ", Name);
            stringBuilder.Append("( [Name] VarChar(100) NOT NULL,");
            stringBuilder.Append(" [Value] Int NOT NULL,");
            stringBuilder.Append(" [LastUpdateTime] DateTime,");
            stringBuilder.AppendFormat(" CONSTRAINT {0} PRIMARY KEY {1} )", PrimaryKeyToken, PrimaryKeyColumns);
            return stringBuilder.ToString();
        }
    }
}