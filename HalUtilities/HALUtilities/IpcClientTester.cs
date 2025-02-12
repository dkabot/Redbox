using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;
using Redbox.HAL.Core;
using Redbox.HAL.IPC.Framework.Pipes;
using Redbox.IPC.Framework;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace HALUtilities
{
  internal sealed class IpcClientTester : IDisposable
  {
    private readonly HardwareService Service;
    private readonly IPCProtocol Protocol;

    public void Dispose()
    {
    }

    internal static void RunTest(string[] args)
    {
      bool flag = true;
      string pipeName = "HAL7001";
      int num1 = 5;
      int num2 = 65536;
      IpcClientTester.ClientType defVal = IpcClientTester.ClientType.None;
      string _protocol = "rcp://127.0.0.1:7001";
      for (int index = 0; index < args.Length; ++index)
      {
        if (args[index] == "-pipeName")
          pipeName = args[++index];
        else if (args[index].StartsWith("--type"))
          defVal = CommandLineOption.GetOptionVal<IpcClientTester.ClientType>(args[index], defVal);
        else if (args[index].StartsWith("--iterations"))
          num1 = CommandLineOption.GetOptionVal<int>(args[index], num1);
        else if (args[index].StartsWith("--dataSize"))
          num2 = CommandLineOption.GetOptionVal<int>(args[index], num2);
        else if (args[index].StartsWith("-protocol"))
          _protocol = args[++index];
        else if (args[index].StartsWith("--debug"))
          flag = CommandLineOption.GetOptionVal<bool>(args[index], flag);
      }
      using (IpcClientTester ipcClientTester = new IpcClientTester(_protocol))
      {
        ConsoleLogger consoleLogger = new ConsoleLogger(flag);
        if (IpcClientTester.ClientType.Message == defVal)
          ipcClientTester.RunMessageClient(pipeName);
        else if (IpcClientTester.ClientType.Basic == defVal)
        {
          ipcClientTester.RunBasic(pipeName);
        }
        else
        {
          if (IpcClientTester.ClientType.Redbox != defVal)
            return;
          ipcClientTester.RedboxClient(num1, num2);
        }
      }
    }

    private void TestGuid()
    {
      byte[] byteArray = Guid.NewGuid().ToByteArray();
      byte[] numArray = new byte[64];
      Array.Copy((Array) byteArray, (Array) numArray, byteArray.Length);
      Guid guid = new Guid(numArray);
    }

    private void RunMessageClient(string pipeName)
    {
      IPCMessageFactory ipcMessageFactory = new IPCMessageFactory();
      ClientPipeChannel channel = new ClientPipeChannel(pipeName, "Client-tester");
      if (!channel.Connect())
        return;
      IIPCMessage msg = ipcMessageFactory.Create(MessageType.Information, MessageSeverity.Low, "Hello world!");
      bool flag = ipcMessageFactory.Write(msg, (IIPCChannel) channel);
      Console.WriteLine("[MsgPipeClient] {0} Message sent {1}", (object) DateTime.Now.ToString(), flag ? (object) "successfully" : (object) "failed");
      Console.WriteLine(msg.ToString());
      Console.WriteLine("[MsgPipeClient] {0} Received message {1} from server", (object) DateTime.Now, (object) ipcMessageFactory.Read((IIPCChannel) channel).ToString());
    }

    private void RunBasic(string pipeName)
    {
      RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider();
      using (ClientPipeChannel clientPipeChannel = new ClientPipeChannel(pipeName, "basic client"))
      {
        if (!clientPipeChannel.Connect())
          return;
        Console.WriteLine("Received message {0} from server.", (object) Encoding.ASCII.GetString(clientPipeChannel.Read()));
        byte[] numArray = new byte[65536];
        cryptoServiceProvider.GetBytes(numArray);
        bool flag = clientPipeChannel.Write(numArray);
        Console.WriteLine("[PipeClient] {0} Message sent {1}", (object) DateTime.Now.ToString(), flag ? (object) "successfully" : (object) "failed");
        Console.WriteLine("[PipeClient] {0} Received message '{0}' from server", (object) DateTime.Now, (object) Encoding.ASCII.GetString(clientPipeChannel.Read()));
      }
    }

    private void RunDataTest(object o) => this.RunDataTest((int) o);

    private string RunDataTest(int size)
    {
      StringBuilder builder = new StringBuilder();
      using (ExecutionTimer executionTimer = new ExecutionTimer())
      {
        HardwareCommandResult hardwareCommandResult = this.Service.TestSomeData(size);
        if (hardwareCommandResult.Success)
        {
          builder.AppendFormat("[Thread-{0}] Test succeeded!", (object) Thread.CurrentThread.Name);
          builder.AppendLine();
          builder.AppendLine("  Messages: ");
          hardwareCommandResult.CommandMessages.ForEach((Action<string>) (msg =>
          {
            builder.AppendFormat("  {0}", (object) msg);
            builder.AppendLine();
          }));
        }
        else
        {
          builder.AppendFormat("[Thread-{0}] ** Test failed **", (object) Thread.CurrentThread.Name);
          builder.AppendLine();
          builder.AppendLine("  Messages:");
          hardwareCommandResult.CommandMessages.ForEach((Action<string>) (msg =>
          {
            builder.AppendFormat("  {0}", (object) msg);
            builder.AppendLine();
          }));
          builder.AppendLine("  Errors:");
          hardwareCommandResult.Errors.ForEach((Action<Error>) (error =>
          {
            builder.AppendFormat("  {0} {1}: {2}", (object) error.Code, (object) error.Description, (object) error.Details);
            builder.AppendLine();
          }));
        }
        executionTimer.Stop();
        builder.AppendFormat("Execution time = {0}ms", (object) executionTimer.ElapsedMilliseconds);
        builder.AppendLine();
      }
      return builder.ToString();
    }

    private void RedboxClient(int iterations, int size)
    {
      Thread[] array = new Thread[iterations];
      for (int index = 0; index < iterations; ++index)
      {
        Thread thread = array[index] = new Thread(new ParameterizedThreadStart(this.RunDataTest));
        thread.Name = string.Format("Clientthread-{0}", (object) index);
        thread.Start((object) size);
      }
      Array.ForEach<Thread>(array, (Action<Thread>) (t => t.Join()));
    }

    private IpcClientTester(string _protocol)
    {
      this.Protocol = IPCProtocol.Parse(_protocol);
      this.Service = new HardwareService(this.Protocol);
    }

    private enum ClientType
    {
      None,
      Redbox,
      Basic,
      Message,
    }
  }
}
