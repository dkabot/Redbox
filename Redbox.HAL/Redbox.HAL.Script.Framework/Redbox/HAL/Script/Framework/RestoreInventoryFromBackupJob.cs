using System;
using System.IO;
using System.Xml;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "restore-inventory-from-backup")]
    internal sealed class RestoreInventoryFromBackupJob : NativeJobAdapter
    {
        internal RestoreInventoryFromBackupJob(ExecutionResult r, ExecutionContext c)
            : base(r, c)
        {
        }

        protected override void ExecuteInner()
        {
            var applicationLog = ApplicationLog.ConfigureLog(Context, true, "Service", true, null);
            if (!TestInventoryStore())
                return;
            var inventoryHelper = BackupHelper.GetInventoryHelper();
            ServiceLocator.Instance.GetService<IRuntimeService>();
            var youngest = inventoryHelper.GetYoungest();
            if (youngest == null)
            {
                applicationLog.WriteFormatted("There are no backup inventory files to import.");
                Context.CreateInfoResult("InventoryImportFailure", "There are no backup files available.");
                AddError("No backups available.");
            }
            else
            {
                applicationLog.WriteFormatted("Reset inventory from file '{0}'", youngest.FullPath);
                try
                {
                    var xml = File.ReadAllText(youngest.FullPath);
                    var document = new XmlDocument();
                    document.LoadXml(xml);
                    if (document.DocumentElement == null)
                    {
                        applicationLog.WriteFormatted("Reset failed - XML load failed.");
                        Context.CreateInfoResult("InventoryImportFailure", "Invalid inventory file.");
                        Result.Errors.Add(Error.NewError("I001", "Invalid document.",
                            "The specified document is invalid."));
                    }
                    else
                    {
                        ServiceLocator.Instance.GetService<IInventoryService>().ResetState(document, Result.Errors);
                        if (Result.Errors.Count > 0)
                            applicationLog.WriteFormatted("Reset inventory FAILED to import inventory.");
                        else
                            applicationLog.WriteFormatted("Reset inventory was successful.");
                    }
                }
                catch (Exception ex)
                {
                    Context.CreateInfoResult("InventoryImportFailure", "Invalid inventory file.");
                    Result.Errors.Add(Error.NewError("I001", "Invalid document.", ex));
                }
            }
        }
    }
}