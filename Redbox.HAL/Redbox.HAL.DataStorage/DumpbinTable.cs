using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage
{
    internal sealed class DumpbinTable : AbstractVistaDBDataTable<IDumpBinInventoryItem>
    {
        internal DumpbinTable(IDataTableDescriptor desc)
            : base(desc, "BarcodesInBin")
        {
        }

        protected override List<IDumpBinInventoryItem> OnLoadEntries()
        {
            var rv = new List<IDumpBinInventoryItem>();
            ExectuteSelectQuery(reader => rv.Add(new DumpbinItem(reader.GetString(0), reader.GetDateTime(1))),
                string.Format("SELECT Barcode, PutTime FROM [{0}]", Name));
            return rv;
        }

        protected override string CreateStatement()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("CREATE TABLE [{0}] ", Name);
            stringBuilder.Append("( [Barcode] VarChar(100) NOT NULL,");
            stringBuilder.Append(" [PutTime] DateTime NOT NULL )");
            return stringBuilder.ToString();
        }

        protected override string UpdateStatement(IDumpBinInventoryItem obj)
        {
            return string.Format("UPDATE {0} SET PutTime = '{1}' where Barcode = '{2}'", Name, obj.PutTime.ToString(),
                obj.ID);
        }

        protected override string InsertStatement(IDumpBinInventoryItem item)
        {
            return string.Format("INSERT INTO [{0}] (Barcode, PutTime) VALUES ('{1}', '{2}')", Name, item.ID,
                item.PutTime.ToString());
        }

        protected override string DeleteStatement(IDumpBinInventoryItem obj)
        {
            return string.Format("delete from {0} where Barcode = '{1}' and PutTime = '{2}'", Name, obj.ID,
                obj.PutTime);
        }
    }
}