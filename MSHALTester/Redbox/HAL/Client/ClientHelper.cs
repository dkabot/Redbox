using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Client;

public sealed class ClientHelper : IDisposable
{
    private readonly NullSink NullInstance = new();
    private readonly IClientOutputSink Sink;
    private bool Disposed;

    public ClientHelper(IClientOutputSink sink, HardwareService service)
    {
        Sink = sink == null ? NullInstance : sink;
        Service = service;
    }

    public ClientHelper(HardwareService service)
        : this(null, service)
    {
    }

    public HardwareService Service { get; private set; }

    public void Dispose()
    {
        if (Disposed)
            return;
        Disposed = true;
    }

    public bool BootstrapInitRunning()
    {
        if (Service != null)
            return BootstrapInitRunningChecked();
        Sink.WriteMessage("The service is not configured.");
        return false;
    }

    public bool WaitforInit()
    {
        return WaitforInit(500);
    }

    public bool WaitforInit(int pause)
    {
        Sink.WriteMessage("Checking for bootstrap init.");
        var dateTime = DateTime.Now;
        var timeSpan = new TimeSpan(0, 0, 5);
        while (BootstrapInitRunningChecked())
        {
            var now = DateTime.Now;
            if (now.Subtract(timeSpan) >= dateTime)
            {
                dateTime = now;
                Sink.WriteMessage("{0}: still waiting for bootstrap init.", now.ToLongTimeString());
            }

            Thread.Sleep(pause);
        }

        return true;
    }

    public HardwareService Connect()
    {
        return ConnectInner(Constants.HALIPCStrings.TcpServer);
    }

    public HardwareService Connect(string ipc)
    {
        return !string.IsNullOrEmpty(ipc)
            ? ConnectInner(ipc)
            : throw new UriFormatException("The URI string is null or empty - please re-configure.");
    }

    public bool TestCommunication()
    {
        if (Service == null)
            ConnectInner(Constants.HALIPCStrings.TcpServer);
        return TestServiceConnection(Service);
    }

    public bool WaitForJob(HardwareJob job, out HardwareJobStatus endStatus)
    {
        endStatus = HardwareJobStatus.Completed;
        try
        {
            var _s0 = HardwareJobStatus.Completed;
            var waitForJob = true;
            job.StatusChanged += (j, status) =>
            {
                _s0 = status;
                waitForJob = status != HardwareJobStatus.Stopped && status != HardwareJobStatus.Errored &&
                             status != HardwareJobStatus.Garbage && status != HardwareJobStatus.Completed &&
                             status != 0;
            };
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
        return Service != null && Service.ExecuteImmediate(inst, out var _).Success;
    }

    public T ExecuteImmediateAndGetResult<T>(HardwareService service, string inst)
    {
        HardwareJob job;
        var hardwareCommandResult = service.ExecuteImmediate(inst, out job);
        return hardwareCommandResult == null || !hardwareCommandResult.Success
            ? default
            : ConversionHelper.ChangeType<T>(GetStackEntriesInner(job, 1)[0]);
    }

    private HardwareService ConnectInner(string ipcUrl)
    {
        try
        {
            var service = new HardwareService(IPCProtocol.Parse(ipcUrl));
            return TestServiceConnection(service) ? service : null;
        }
        catch (UriFormatException ex)
        {
            return Service = null;
        }
    }

    private bool TestServiceConnection(HardwareService service)
    {
        if (service == null)
            return false;
        var hardwareCommandResult = service.ExecuteServiceCommand("SERVICE test-comm", 5000);
        return hardwareCommandResult.Success && hardwareCommandResult.CommandMessages[0] == "ACK";
    }

    private string FindLocalIP()
    {
        foreach (var address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            if (address.AddressFamily == AddressFamily.InterNetwork)
                return address.ToString();
        return null;
    }

    private string[] GetStackEntriesInner(HardwareJob job, int depth)
    {
        var stackEntriesInner = new string[depth];
        for (var index = 0; index < depth; ++index)
            stackEntriesInner[index] = string.Empty;
        Stack<string> stack;
        if (job.GetStack(out stack).Success && stack.Count >= depth)
            for (var index = 0; index < depth; ++index)
                stackEntriesInner[index] = stack.Pop();
        return stackEntriesInner;
    }

    private bool BootstrapInitRunningChecked()
    {
        string status;
        if (!Service.GetInitStatus(out status).Success)
        {
            Sink.WriteMessage("Unable to determine init status.");
            return false;
        }

        if (!("COMPLETED" == status) && !("ERRORED" == status))
            return true;
        Sink.WriteMessage("Boot init ended with status: {0}", status);
        return false;
    }
}