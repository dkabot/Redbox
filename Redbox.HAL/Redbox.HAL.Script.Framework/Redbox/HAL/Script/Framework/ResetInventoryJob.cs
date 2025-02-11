using System;
using System.IO;
using System.Xml;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "reset-inventory-job", Operand = "RESET-INVENTORY-JOB")]
    internal sealed class ResetInventoryJob : NativeJobAdapter
    {
        internal ResetInventoryJob(ExecutionResult r, ExecutionContext c)
            : base(r, c)
        {
        }

        protected override void ExecuteInner()
        {
            var str = Context.PopTop<string>();
            if (!File.Exists(str))
            {
                str = Path.Combine(ServiceLocator.Instance.GetService<IRuntimeService>().DataPath, str);
                if (!File.Exists(str))
                {
                    AddError("The file {0} doesn't exist.");
                    return;
                }
            }

            LogHelper.Instance.WithContext("Reset inventory from file '{0}'", str);
            try
            {
                var xml = File.ReadAllText(str);
                var document = new XmlDocument();
                try
                {
                    document.LoadXml(xml);
                    if (document.DocumentElement == null)
                    {
                        Result.Errors.Add(Error.NewError("I001", "Invalid document.",
                            "The specified document is invalid."));
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Result.Errors.Add(Error.NewError("I001", "Invalid document.", ex));
                    return;
                }

                ServiceLocator.Instance.GetService<IInventoryService>().ResetState(document, Result.Errors);
                ServiceLocator.Instance.GetService<IDumpbinService>().ResetState(document, Result.Errors);
                if (Result.Errors.Count <= 0)
                    return;
                LogHelper.Instance.Log("RESET INVENTORY Failed to import inventory.", LogEntryType.Error);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Reset-inventory caught an exception.", ex);
                Result.Errors.Add(Error.NewError("I003", "Reset inventory failure.", ex));
            }
        }
    }
}