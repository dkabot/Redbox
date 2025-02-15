using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Attributes;
using Redbox.HAL.IPC.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Service.Win32.Commands
{
    [Command("config")]
    [Description(
        "The CONFIG command is reponsible for loading, retrieving and setting configuration for individual HAL driver systems.")]
    public sealed class ConfigurationCommand
    {
        [CommandForm(Name = "save")]
        [Usage("config save [path: path-to-config]")]
        [Description(
            "Forces the HAL Service to save its current state to the HAL.xml configuration file or the file specified by the optional path parameter.")]
        public void SaveConfiguration(CommandContext context, [CommandKeyValue(IsRequired = false)] string path)
        {
            if (string.IsNullOrEmpty(path))
                path = "$(RunningPath)\\HAL.xml";
            path = ServiceLocator.Instance.GetService<IRuntimeService>().ExpandConstantMacros(path);
            if (!File.Exists(path))
                context.Errors.Add(Error.NewError("S001", string.Format("The file '{0}' does not exist.", path),
                    "Use $(RunningPath) macro to easily reference relative paths from the service working path."));
            else
                ServiceLocator.Instance.GetService<IConfigurationService>().Save(context.Errors);
        }

        [CommandForm(Name = "get")]
        [Usage("config get name: config-name")]
        public void GetConfiguration(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                context.Errors.Add(Error.NewError("S001", "The parameter name: is required.",
                    "Specify the name: parameter when calling the CONFIG GET command form."));
            }
            else
            {
                var str = ServiceLocator.Instance.GetService<IConfigurationService>().FormatAsXml(name);
                if (string.IsNullOrEmpty(str))
                    context.Errors.Add(Error.NewError("S001",
                        string.Format("The requested configuration name '{0}' is not known.", name),
                        "Specify one of the valid configuration names."));
                else
                    ProtocolHelper.FormatCompressedString(str, context);
            }
        }

        [CommandForm(Name = "set")]
        [Usage("config set name: config-name data: base64-encoded-config")]
        public void SetConfiguration(CommandContext context, [CommandKeyValue(IsRequired = true)] string name,
            [CommandKeyValue(IsRequired = true, BinaryEncoding = BinaryEncoding.Base64,
                CompressionType = CompressionType.LZMA)]
            byte[] data)
        {
            if (data == null)
            {
                context.Errors.Add(Error.NewError("S001", "The parameter data: is required.",
                    "Specify the data: parameter when calling the CONFIG SET command form."));
            }
            else if (string.IsNullOrEmpty(name))
            {
                context.Errors.Add(Error.NewError("S001", "The parameter name: is required.",
                    "Specify the name: parameter when calling the CONFIG SET command form."));
            }
            else
            {
                var data1 = Encoding.ASCII.GetString(data);
                ServiceLocator.Instance.GetService<IConfigurationService>().UpdateFromXml(name, data1, context.Errors);
            }
        }

        [CommandForm(Name = "get-inventory-state")]
        [Usage("config get-inventory-state")]
        public void GetInventoryState(CommandContext context)
        {
            var service = ServiceLocator.Instance.GetService<IInventoryService>();
            if (service.CheckIntegrity() != ErrorCodes.Success)
                context.Errors.Add(Error.NewError("S005", "Inventory error.",
                    "The inventory table is in an invalid state."));
            else
                using (var w = new MemoryStream())
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
                    ProtocolHelper.FormatCompressedString(Encoding.ASCII.GetString(w.ToArray()), context);
                }
        }

        [CommandForm(Name = "set-inventory-state")]
        [Usage("config set-inventory-state data: base64-encoded-state")]
        public void SetInventoryState(CommandContext context,
            [CommandKeyValue(IsRequired = true, BinaryEncoding = BinaryEncoding.Base64,
                CompressionType = CompressionType.LZMA)]
            byte[] data)
        {
            if (data == null)
            {
                context.Errors.Add(Error.NewError("S001", "The parameter data: is required.",
                    "Specify the data: parameter when calling the CONFIG set-inventory-state command form."));
            }
            else
            {
                var service = ServiceLocator.Instance.GetService<IRuntimeService>();
                var uniqueFile = service.GenerateUniqueFile("xml");
                try
                {
                    File.WriteAllText(Path.Combine(service.DataPath, uniqueFile), Encoding.ASCII.GetString(data));
                    var executionContext = ServiceLocator.Instance.GetService<IExecutionService>()
                        .ScheduleJob("reset-inventory-job", string.Empty, DateTime.Now, ProgramPriority.Low);
                    executionContext.Push(uniqueFile, StackEnd.Top);
                    LogHelper.Instance.Log("Reset-Inventory job ( job ID = {0} ).", executionContext.ID);
                    executionContext.Pend();
                    executionContext.WaitForCompletion();
                    context.Messages.Add(executionContext.GetStatus().ToString());
                }
                catch
                {
                    context.Messages.Add("ERRORED");
                    LogHelper.Instance.Log("Reset-inventory job failed; file = {0}", uniqueFile);
                }
            }
        }
    }
}