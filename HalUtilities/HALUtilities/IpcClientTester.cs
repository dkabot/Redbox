using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;
using Redbox.HAL.Core;
using Redbox.HAL.IPC.Framework.Pipes;
using Redbox.IPC.Framework;

namespace HALUtilities
{
    internal sealed class IpcClientTester : IDisposable
    {
        private readonly IPCProtocol Protocol;
        private readonly HardwareService Service;

        private IpcClientTester(string _protocol)
        {
            Protocol = IPCProtocol.Parse(_protocol);
            Service = new HardwareService(Protocol);
        }

        public void Dispose()
        {
        }

        internal static void RunTest(string[] args)
        {
            var flag = true;
            var pipeName = "HAL7001";
            var num1 = 5;
            var num2 = 65536;
            var defVal = ClientType.None;
            var _protocol = "rcp://127.0.0.1:7001";
            for (var index = 0; index < args.Length; ++index)
                if (args[index] == "-pipeName")
                    pipeName = args[++index];
                else if (args[index].StartsWith("--type"))
                    defVal = CommandLineOption.GetOptionVal(args[index], defVal);
                else if (args[index].StartsWith("--iterations"))
                    num1 = CommandLineOption.GetOptionVal(args[index], num1);
                else if (args[index].StartsWith("--dataSize"))
                    num2 = CommandLineOption.GetOptionVal(args[index], num2);
                else if (args[index].StartsWith("-protocol"))
                    _protocol = args[++index];
                else if (args[index].StartsWith("--debug"))
                    flag = CommandLineOption.GetOptionVal(args[index], flag);
            using (var ipcClientTester = new IpcClientTester(_protocol))
            {
                var consoleLogger = new ConsoleLogger(flag);
                if (ClientType.Message == defVal)
                {
                    ipcClientTester.RunMessageClient(pipeName);
                }
                else if (ClientType.Basic == defVal)
                {
                    ipcClientTester.RunBasic(pipeName);
                }
                else
                {
                    if (ClientType.Redbox != defVal)
                        return;
                    ipcClientTester.RedboxClient(num1, num2);
                }
            }
        }

        private void TestGuid()
        {
            var byteArray = Guid.NewGuid().ToByteArray();
            var numArray = new byte[64];
            Array.Copy(byteArray, numArray, byteArray.Length);
            var guid = new Guid(numArray);
        }

        private void RunMessageClient(string pipeName)
        {
            var ipcMessageFactory = new IPCMessageFactory();
            var channel = new ClientPipeChannel(pipeName, "Client-tester");
            if (!channel.Connect())
                return;
            var msg = ipcMessageFactory.Create(MessageType.Information, MessageSeverity.Low, "Hello world!");
            var flag = ipcMessageFactory.Write(msg, channel);
            Console.WriteLine("[MsgPipeClient] {0} Message sent {1}", DateTime.Now.ToString(),
                flag ? "successfully" : (object)"failed");
            Console.WriteLine(msg.ToString());
            Console.WriteLine("[MsgPipeClient] {0} Received message {1} from server", DateTime.Now,
                ipcMessageFactory.Read(channel).ToString());
        }

        private void RunBasic(string pipeName)
        {
            var cryptoServiceProvider = new RNGCryptoServiceProvider();
            using (var clientPipeChannel = new ClientPipeChannel(pipeName, "basic client"))
            {
                if (!clientPipeChannel.Connect())
                    return;
                Console.WriteLine("Received message {0} from server.",
                    Encoding.ASCII.GetString(clientPipeChannel.Read()));
                var numArray = new byte[65536];
                cryptoServiceProvider.GetBytes(numArray);
                var flag = clientPipeChannel.Write(numArray);
                Console.WriteLine("[PipeClient] {0} Message sent {1}", DateTime.Now.ToString(),
                    flag ? "successfully" : (object)"failed");
                Console.WriteLine("[PipeClient] {0} Received message '{0}' from server", DateTime.Now,
                    Encoding.ASCII.GetString(clientPipeChannel.Read()));
            }
        }

        private void RunDataTest(object o)
        {
            RunDataTest((int)o);
        }

        private string RunDataTest(int size)
        {
            var builder = new StringBuilder();
            using (var executionTimer = new ExecutionTimer())
            {
                var hardwareCommandResult = Service.TestSomeData(size);
                if (hardwareCommandResult.Success)
                {
                    builder.AppendFormat("[Thread-{0}] Test succeeded!", Thread.CurrentThread.Name);
                    builder.AppendLine();
                    builder.AppendLine("  Messages: ");
                    hardwareCommandResult.CommandMessages.ForEach(msg =>
                    {
                        builder.AppendFormat("  {0}", msg);
                        builder.AppendLine();
                    });
                }
                else
                {
                    builder.AppendFormat("[Thread-{0}] ** Test failed **", Thread.CurrentThread.Name);
                    builder.AppendLine();
                    builder.AppendLine("  Messages:");
                    hardwareCommandResult.CommandMessages.ForEach(msg =>
                    {
                        builder.AppendFormat("  {0}", msg);
                        builder.AppendLine();
                    });
                    builder.AppendLine("  Errors:");
                    hardwareCommandResult.Errors.ForEach(error =>
                    {
                        builder.AppendFormat("  {0} {1}: {2}", error.Code, error.Description, error.Details);
                        builder.AppendLine();
                    });
                }

                executionTimer.Stop();
                builder.AppendFormat("Execution time = {0}ms", executionTimer.ElapsedMilliseconds);
                builder.AppendLine();
            }

            return builder.ToString();
        }

        private void RedboxClient(int iterations, int size)
        {
            var array = new Thread[iterations];
            for (var index = 0; index < iterations; ++index)
            {
                var thread = array[index] = new Thread(RunDataTest);
                thread.Name = string.Format("Clientthread-{0}", index);
                thread.Start(size);
            }

            Array.ForEach(array, t => t.Join());
        }

        private enum ClientType
        {
            None,
            Redbox,
            Basic,
            Message
        }
    }
}