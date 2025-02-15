using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Reflection;
using Redbox.Core;
using Redbox.Log.Framework;

namespace InventoryImport
{
    public static class Program
    {
        private static readonly ILogger m_logger = LogHelper.Instance.CreateLog4NetLogger(typeof(Program));

        public static int Main(string[] args)
        {
            m_logger.Configure(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "log4net.config"));
            LogHelper.Instance.Logger = m_logger;
            var path = "C:\\Program Files\\Redbox\\DB\\Kiosk.mdb";
            if (args.Length == 1)
                path = args[0];
            if (!File.Exists(path))
            {
                LogHelper.Instance.Log("The database file '{0}' does not exist.", path);
                return 1;
            }

            var oleDbCommand1 = new OleDbCommand(
                "\r\n            SELECT \r\n                i.Deck, i.Slot, i.Barcode as BarCode, i.DiskType\r\n\t        FROM \r\n                Inventory i LEFT JOIN Title t on t.TitleId =  i.TitleID\r\n            ORDER \r\n                BY Deck, Slot");
            oleDbCommand1.CommandType = CommandType.Text;
            using (var oleDbCommand2 = oleDbCommand1)
            {
                try
                {
                    oleDbCommand2.Connection =
                        new OleDbConnection(string.Format(
                            "Provider=Microsoft.Jet.OLEDB.4.0; Data Source={0}; User Id=; Password=", path));
                    oleDbCommand2.Connection.Open();
                    using (var oleDbDataReader = oleDbCommand2.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (oleDbDataReader.Read())
                        {
                            var disc = new Disc(oleDbDataReader["Deck"].ToString(), oleDbDataReader["Slot"].ToString(),
                                oleDbDataReader["Barcode"].ToString(),
                                int.Parse(oleDbDataReader["DiskType"].ToString()));
                            LogHelper.Instance.Log("Legacy disc record: {0}", disc);
                            HalInventory.Add(disc);
                        }
                    }

                    HalInventory.SetInventory();
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log("An unhandled exception was raised in Program.Main.", ex);
                    return 1;
                }
                finally
                {
                    oleDbCommand2.Connection.Dispose();
                }
            }

            return 0;
        }
    }
}