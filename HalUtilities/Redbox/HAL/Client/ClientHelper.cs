using Redbox.HAL.Component.Model.Extensions;
using Redbox.IPC.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Redbox.HAL.Client
{
  public sealed class ClientHelper : IDisposable
  {
    private bool Disposed;
    private readonly IClientOutputSink Sink;
    private readonly NullSink NullInstance = new NullSink();

    public void Dispose()
    {
      if (this.Disposed)
        return;
      this.Disposed = true;
    }

    public bool BootstrapInitRunning()
    {
      if (this.Service != null)
        return this.BootstrapInitRunningChecked();
      this.Sink.WriteMessage("The service is not configured.");
      return false;
    }

    public bool WaitforInit() => this.WaitforInit(500);

    public bool WaitforInit(int pause)
    {
      this.Sink.WriteMessage("Checking for bootstrap init.");
      DateTime dateTime = DateTime.Now;
      TimeSpan timeSpan = new TimeSpan(0, 0, 5);
      while (this.BootstrapInitRunningChecked())
      {
        DateTime now = DateTime.Now;
        if (now.Subtract(timeSpan) >= dateTime)
        {
          dateTime = now;
          this.Sink.WriteMessage("{0}: still waiting for bootstrap init.", (object) now.ToLongTimeString());
        }
        Thread.Sleep(pause);
      }
      return true;
    }

    public HardwareService Connect() => this.ConnectInner(Constants.HALIPCStrings.TcpServer);

    public HardwareService Connect(string ipc)
    {
      return !string.IsNullOrEmpty(ipc) ? this.ConnectInner(ipc) : throw new UriFormatException("The URI string is null or empty - please re-configure.");
    }

    public bool TestCommunication()
    {
      if (this.Service == null)
        this.ConnectInner(Constants.HALIPCStrings.TcpServer);
      return this.TestServiceConnection(this.Service);
    }

    public bool WaitForJob(HardwareJob job, out HardwareJobStatus endStatus)
    {
      endStatus = HardwareJobStatus.Completed;
      try
      {
        HardwareJobStatus _s0 = HardwareJobStatus.Completed;
        bool waitForJob = true;
        job.StatusChanged += (HardwareStatusChangeEvent) ((j, status) =>
        {
          _s0 = status;
          waitForJob = status != HardwareJobStatus.Stopped && status != HardwareJobStatus.Errored && status != HardwareJobStatus.Garbage && status != HardwareJobStatus.Completed && status != 0;
        });
        if (!job.Connect().Success || !job.Pend().Success)
          return false;
        while (waitForJob)
          Thread.Sleep(250);
        endStatus = _s0;
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
      finally
      {
        job.Disconnect();
      }
    }

    public bool ExecuteImmediate(string inst)
    {
      return this.Service != null && this.Service.ExecuteImmediate(inst, out HardwareJob _).Success;
    }

    public T ExecuteImmediateAndGetResult<T>(HardwareService service, string inst)
    {
      HardwareJob job;
      HardwareCommandResult hardwareCommandResult = service.ExecuteImmediate(inst, out job);
      return hardwareCommandResult == null || !hardwareCommandResult.Success ? default (T) : ConversionHelper.ChangeType<T>((object) this.GetStackEntriesInner(job, 1)[0]);
    }

    public ClientHelper(IClientOutputSink sink, HardwareService service)
    {
      this.Sink = sink == null ? (IClientOutputSink) this.NullInstance : sink;
      this.Service = service;
    }

    public ClientHelper(HardwareService service)
      : this((IClientOutputSink) null, service)
    {
    }

    public HardwareService Service { get; private set; }

    private HardwareService ConnectInner(string ipcUrl)
    {
      try
      {
        HardwareService service = new HardwareService(IPCProtocol.Parse(ipcUrl));
        return this.TestServiceConnection(service) ? service : (HardwareService) null;
      }
      catch (UriFormatException ex)
      {
        return this.Service = (HardwareService) null;
      }
    }

    private bool TestServiceConnection(HardwareService service)
    {
      if (service == null)
        return false;
      HardwareCommandResult hardwareCommandResult = service.ExecuteServiceCommand("SERVICE test-comm", 5000);
      return hardwareCommandResult.Success && hardwareCommandResult.CommandMessages[0] == "ACK";
    }

    private string FindLocalIP()
    {
      foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
      {
        if (address.AddressFamily == AddressFamily.InterNetwork)
          return address.ToString();
      }
      return (string) null;
    }

    private string[] GetStackEntriesInner(HardwareJob job, int depth)
    {
      string[] stackEntriesInner = new string[depth];
      for (int index = 0; index < depth; ++index)
        stackEntriesInner[index] = string.Empty;
      Stack<string> stack;
      if (job.GetStack(out stack).Success && stack.Count >= depth)
      {
        for (int index = 0; index < depth; ++index)
          stackEntriesInner[index] = stack.Pop();
      }
      return stackEntriesInner;
    }

    private bool BootstrapInitRunningChecked()
    {
      string status;
      if (!this.Service.GetInitStatus(out status).Success)
      {
        this.Sink.WriteMessage("Unable to determine init status.");
        return false;
      }
      if (!("COMPLETED" == status) && !("ERRORED" == status))
        return true;
      this.Sink.WriteMessage("Boot init ended with status: {0}", (object) status);
      return false;
    }
  }
}
