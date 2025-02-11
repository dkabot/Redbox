using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class KioskFunctionCheckService : IKioskFunctionCheckService
    {
        private readonly IDataTable<IKioskFunctionCheckData> Table;

        internal KioskFunctionCheckService()
        {
            Table = ServiceLocator.Instance.GetService<IDataTableService>().GetTable<IKioskFunctionCheckData>();
            if (Table.Exists)
                return;
            LogHelper.Instance.Log("[KioskFunctionCheckService] KFC table doesn't exist; create returned {0}",
                Table.Create() ? "SUCCESS" : (object)"FAILURE");
        }

        public bool Add(IKioskFunctionCheckData data)
        {
            return Table.Insert(data);
        }

        public IList<IKioskFunctionCheckData> Load()
        {
            return Table.LoadEntries();
        }

        public int CleanOldEntries()
        {
            return Table.Clean();
        }
    }
}