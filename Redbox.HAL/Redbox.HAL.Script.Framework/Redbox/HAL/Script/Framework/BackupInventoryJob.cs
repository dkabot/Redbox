using System.IO;
using System.Text;
using System.Xml;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "backup-inventory")]
    internal sealed class BackupInventoryJob : NativeJobAdapter
    {
        internal BackupInventoryJob(ExecutionResult r, ExecutionContext c)
            : base(r, c)
        {
        }

        protected override void ExecuteInner()
        {
            var service = ServiceLocator.Instance.GetService<IInventoryService>();
            if (service.CheckIntegrity() == ErrorCodes.Success)
            {
                var inventoryHelper = BackupHelper.GetInventoryHelper();
                using (var w = new FileStream(inventoryHelper.MakeNewBackup(), FileMode.OpenOrCreate))
                {
                    var writer = new XmlTextWriter(w, Encoding.ASCII)
                    {
                        Formatting = Formatting.Indented
                    };
                    writer.WriteStartDocument();
                    writer.WriteStartElement("inventory-items");
                    service.GetState(writer);
                    ServiceLocator.Instance.GetService<IDumpbinService>().GetState(writer);
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                }

                inventoryHelper.Trim();
            }
            else
            {
                var writer = ApplicationLog.ConfigureLog(Context, true, "Service", true, null);
                writer.WriteFormatted("Integrity check on the datastore failed.");
                Result.Errors.Dump(writer);
                AddError("Integrity check failure.");
            }
        }
    }
}