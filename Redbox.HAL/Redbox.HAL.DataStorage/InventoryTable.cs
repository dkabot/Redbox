using System;
using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage
{
    internal sealed class InventoryTable : AbstractVistaDBDataTable<ILocation>
    {
        internal InventoryTable(IDataTableDescriptor descriptor)
            : base(descriptor, "Inventory_v1")
        {
        }

        protected override string PrimaryKeyToken => "inventory_PK";

        protected override string PrimaryKeyColumns => "( Deck, Slot )";

        protected override List<ILocation> OnLoadEntries()
        {
            var list = new List<ILocation>();
            ExectuteSelectQuery(reader =>
                {
                    var nullable = new DateTime?();
                    if (!reader.IsDBNull(2))
                        nullable = reader.GetDateTime(2);
                    list.Add(new Location(reader.GetInt32(6), reader.GetInt32(0))
                    {
                        ID = reader.GetString(1),
                        ReturnDate = nullable,
                        Excluded = reader.GetBoolean(3),
                        StuckCount = reader.GetInt32(4),
                        Flags = (MerchFlags)reader.GetInt64(5)
                    });
                },
                string.Format(
                    "SELECT Slot, ID, LastReturnTime, Excluded, EmptyStuckCount, MerchFlags, Deck FROM [{0}] ",
                    Name));
            return list;
        }

        protected override string UpdateStatement(ILocation l)
        {
            return string.Format(
                "UPDATE {0} SET ID = '{1}', LastReturnTime = {2}, Excluded = '{3}', EmptyStuckCount = '{4}', MerchFlags = '{5}' WHERE  Deck = '{6}' AND Slot = '{7}'",
                Name, l.ID, FormatReturnTime(l), l.Excluded ? 1 : 0, l.StuckCount, (int)l.Flags, l.Deck, l.Slot);
        }

        protected override string InsertStatement(ILocation l)
        {
            return string.Format(
                "INSERT INTO [{0}] (Deck, Slot, ID, LastReturnTime, Excluded, EmptyStuckCount, MerchFlags) VALUES ('{1}', '{2}', '{3}', {4}, '{5}', '{6}', '{7}')",
                Name, l.Deck, l.Slot, l.ID, FormatReturnTime(l), l.Excluded ? 1 : 0, l.StuckCount, (int)l.Flags);
        }

        protected override string DeleteStatement(ILocation obj)
        {
            return string.Format("delete from {0} where Deck = '{1}' and Slot = '{2}'", Name, obj.Deck, obj.Slot);
        }

        protected override string CreateStatement()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("CREATE TABLE [{0}] ", Name);
            stringBuilder.Append("( [Deck] Int NOT NULL,");
            stringBuilder.Append(" [Slot] Int NOT NULL,");
            stringBuilder.Append(" [ID] VarChar(100) NOT NULL,");
            stringBuilder.Append(" [LastReturnTime] DateTime,");
            stringBuilder.Append(" [Excluded] Bit NOT NULL,");
            stringBuilder.Append(" [EmptyStuckCount] Int NOT NULL,");
            stringBuilder.Append(" [MerchFlags] Int NOT NULL,");
            stringBuilder.AppendFormat(" CONSTRAINT {0} PRIMARY KEY {1} )", PrimaryKeyToken, PrimaryKeyColumns);
            return stringBuilder.ToString();
        }

        private string FormatReturnTime(ILocation location)
        {
            return location.ReturnDate.HasValue ? string.Format("'{0}'", location.ReturnDate.Value) : "NULL";
        }
    }
}