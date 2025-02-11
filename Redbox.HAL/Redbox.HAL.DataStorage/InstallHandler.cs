using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage
{
    internal sealed class InstallHandler : IPackageInstallHandler
    {
        public void OnRegister()
        {
        }

        public void OnUpgrade(ErrorList errors, TextWriter writer)
        {
            var service = ServiceLocator.Instance.GetService<IDataTableService>();
            var table = service.GetTable<IDumpBinInventoryItem>();
            if (table.Exists)
            {
                writer.WriteLine("[InventoryService.Upgrade] The bin table already exists.");
            }
            else if (!table.Create())
            {
                errors.Add(Error.NewError("I0777", "Create table error.",
                    "Unable to create dumpbin table in haldata.vdb3."));
            }
            else
            {
                var legacyTable = service.GetLegacyTable<IDumpBinInventoryItem>();
                if (legacyTable.Exists)
                {
                    var binInventoryItemList = legacyTable.LoadEntries();
                    using (new DisposeableList<IDumpBinInventoryItem>(binInventoryItemList))
                    {
                        if (table.Insert(binInventoryItemList))
                            writer.WriteLine("[InventoryService.OnUpgrade] Successfully imported {0} dumpbin entries.",
                                binInventoryItemList.Count);
                        else
                            errors.Add(Error.NewError("I667", "Import error.",
                                "Unable to move dumpbin table data from halcounters."));
                    }

                    legacyTable.Drop();
                }
            }

            OnNewInstall(errors, writer);
        }

        public void OnNewInstall(ErrorList errors, TextWriter writer)
        {
            var service = ServiceLocator.Instance.GetService<IDataTableService>();
            TestAndCreate(service.GetTable<IKioskFunctionCheckData>(), errors, writer);
            TestAndCreate(service.GetTable<IHardwareCorrectionStatistic>(), errors, writer);
            var legacyTable = service.GetLegacyTable<IHardwareCorrectionStatistic>();
            if (legacyTable == null || !legacyTable.Exists)
                return;
            legacyTable.Drop();
        }

        private void TestAndCreate<T>(IDataTable<T> table, ErrorList errors, TextWriter writer)
        {
            if (!table.Exists)
            {
                if (!table.Create())
                    errors.Add(Error.NewError("I0777", "Create table error.",
                        string.Format("Unable to create table '{0}'", table.Name)));
                else
                    writer.WriteLine("Successfully created table '{0}'", table.Name);
            }
            else
            {
                writer.WriteLine("The table '{0}' already exists.", table.Name);
            }
        }
    }
}