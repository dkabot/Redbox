using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Attributes;
using Redbox.HAL.IPC.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Service.Win32.Commands
{
    [Command("program")]
    public sealed class ProgramCommand
    {
        [CommandForm(Name = "compile")]
        [Usage(
            "PROGRAM compile path: 'C:\\temp\\my-script.hs' [name: program-name] [requires-client-connection: true|false]")]
        public void Compile(
            CommandContext context,
            [CommandKeyValue(IsRequired = true)] string path,
            string name,
            [CommandKeyValue(KeyName = "requires-client-connection")]
            bool? requiresClientConnection)
        {
            if (string.IsNullOrEmpty(path))
                context.Errors.Add(Error.NewError("S001", "The path: parameter is required.",
                    "Issue the PROGRAM compile command again passing in a valid path: parameter."));
            else
                CompileProgram(context.Errors, name, path);
        }

        [CommandForm(Name = "remove")]
        [Usage("PROGRAM remove name: progam-name")]
        public void Remove(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
        {
            ServiceLocator.Instance.GetService<IExecutionService>().RemoveProgram(name);
        }

        [CommandForm(Name = "get")]
        [Usage("PROGRAM get name: program-name")]
        public void Get(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
        {
            var service = ServiceLocator.Instance.GetService<IRuntimeService>();
            ProtocolHelper.FormatCompressedBytes(
                service.ReadToBuffer(
                    service.ExpandConstantMacros(string.Format("$(RunningPath)\\..\\scripts\\{0}.hs", name))), context);
        }

        [CommandForm(Name = "set")]
        [Usage("PROGRAM set name: program-name data: base4-encoded-script [requires-client-connection: true|false]")]
        public void Set(
            CommandContext context,
            [CommandKeyValue(IsRequired = true)] string name,
            [CommandKeyValue(IsRequired = true, BinaryEncoding = BinaryEncoding.Base64,
                CompressionType = CompressionType.LZMA)]
            byte[] data,
            [CommandKeyValue(KeyName = "requires-client-connection")]
            bool? requiresClientConnection)
        {
            var path = ServiceLocator.Instance.GetService<IRuntimeService>()
                .ExpandConstantMacros(string.Format("$(RunningPath)\\..\\scripts\\{0}.hs", name));
            File.WriteAllBytes(path, data);
            CompileProgram(context.Errors, name, path);
        }

        [CommandForm(Name = "properties")]
        [Usage("PROGRAM properties name: program-name requires-client-connection: true|false")]
        public void Properties(CommandContext context, [CommandKeyValue(IsRequired = true)] string name,
            [CommandKeyValue(KeyName = "requires-client-connection")] bool requiresClientConnection)
        {
            context.Errors.Add(Error.NewError("S002", "Unsupported call.",
                "The requires-client-connection form is no longer supported."));
        }

        [CommandForm(Name = "list")]
        [Usage("PROGRAM list")]
        public void List(CommandContext context)
        {
            var service = ServiceLocator.Instance.GetService<IExecutionService>();
            context.Messages.AddRange(service.GetProgramList());
        }

        private static void CompileProgram(ErrorList errors, string programName, string path)
        {
            if (!File.Exists(path))
            {
                errors.Add(Error.NewError("S001", string.Format("The file '{0}' does not exist.", path),
                    "Check the path of the script file and reissue the compile command."));
            }
            else
            {
                if (programName == null)
                    programName = Path.GetFileNameWithoutExtension(path);
                using (var memoryStream =
                       new MemoryStream(ServiceLocator.Instance.GetService<IRuntimeService>().ReadToBuffer(path)))
                {
                    var executionResult = ServiceLocator.Instance.GetService<IExecutionService>()
                        .Compile(memoryStream, programName);
                    errors.AddRange(executionResult.Errors);
                }
            }
        }
    }
}