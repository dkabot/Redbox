using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using Redbox.HAL.IPC.Framework.Pipes;
using System;
using System.Threading;

namespace HALUtilities
{
  internal abstract class AbstractPipeSever
  {
    protected readonly string PipeName;
    protected volatile bool IsAlive;

    protected abstract void OnStop();

    protected abstract void Process(BasePipeChannel channel);

    protected virtual void Run()
    {
      LogHelper.Instance.Log("Abstract[PipeServer] {0} Server running on {1}", (object) DateTime.Now, (object) this.PipeName);
      this.IsAlive = true;
      using (NamedPipeServer namedPipeServer = NamedPipeServer.Create(this.PipeName))
      {
        while (this.IsAlive)
        {
          BasePipeChannel channel = namedPipeServer.WaitForClientConnect();
          if (channel != null)
            ThreadPool.QueueUserWorkItem((WaitCallback) (o => this.Process(channel)));
        }
      }
      LogHelper.Instance.Log("[AbstractPipeServer] {0} Exiting.", (object) DateTime.Now);
    }

    protected AbstractPipeSever(string pipeName) => this.PipeName = pipeName;

    internal static void RunServer(string cmdLineArg, PipeServers serverType, bool debug)
    {
      ConsoleLogger consoleLogger = new ConsoleLogger(debug);
      string optionVal = CommandLineOption.GetOptionVal<string>(cmdLineArg, "HAL7001");
      AbstractPipeSever abstractPipeSever = (AbstractPipeSever) null;
      switch (serverType)
      {
        case PipeServers.Message:
          abstractPipeSever = (AbstractPipeSever) new PipeMessageServer(optionVal);
          break;
        case PipeServers.Basic:
          abstractPipeSever = (AbstractPipeSever) new BasicPipeServer(optionVal);
          break;
      }
      if (abstractPipeSever != null)
      {
        Thread thread = new Thread(new ThreadStart(abstractPipeSever.Run));
        thread.Start();
        Console.WriteLine("Press any key to halt.");
        Console.ReadLine();
        abstractPipeSever.Stop();
        thread.Join(5000);
        Console.WriteLine("Program finished.");
      }
      Environment.Exit(0);
    }

    private void Stop()
    {
      this.IsAlive = false;
      this.OnStop();
    }
  }
}
