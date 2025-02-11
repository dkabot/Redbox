using System;
using System.Collections.Generic;
using System.IO.Ports;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.Core;

public sealed class PortManager : IPortManagerService
{
    private const string MsgHeader = "[Port Scanner]";
    private readonly List<string> InUsePorts = new();
    private readonly object ScanLock = new();

    public bool Register(ICommPort port)
    {
        lock (ScanLock)
        {
            return RegisterUnderLock(port);
        }
    }

    public void Dispose(ICommPort port)
    {
        lock (ScanLock)
        {
            InUsePorts.Remove(port.PortName.ToUpper());
            port.Dispose();
        }
    }

    public ICommChannelConfiguration CreateConfiguration()
    {
        return new RedboxSerialPortConfiguration();
    }

    public ICommPort Create(SerialPort port)
    {
        return Create(port, CommPortReadModes.Async);
    }

    public ICommPort Create(SerialPort port, CommPortReadModes mode)
    {
        var port1 = port != null ? OnCreate(port, mode) : throw new ArgumentException(nameof(port));
        if (port1 != null)
            Register(port1);
        return port1;
    }

    public ICommPort Scan(
        ICommChannelConfiguration conf,
        Predicate<ICommPort> probe,
        CommPortReadModes mode)
    {
        return Scan(null, conf, probe, mode);
    }

    public ICommPort Scan(
        string tryFirst,
        ICommChannelConfiguration conf,
        Predicate<ICommPort> probe,
        CommPortReadModes mode)
    {
        lock (ScanLock)
        {
            using (var executionTimer = new ExecutionTimer())
            {
                var port = (ICommPort)null;
                if (tryFirst != null)
                    port = Probe(tryFirst, conf, probe, mode);
                if (port == null)
                    port = ProbePorts(conf, probe, mode);
                executionTimer.Stop();
                LogHelper.Instance.Log("[Port Scan] Time to scan ports: {0}ms", executionTimer.ElapsedMilliseconds);
                if (port != null)
                    RegisterUnderLock(port);
                return port;
            }
        }
    }

    private RedboxSerialPort Probe(
        string portName,
        ICommChannelConfiguration conf,
        Predicate<ICommPort> probe,
        CommPortReadModes mode)
    {
        if (InUsePorts.Find(each => each.Equals(portName, StringComparison.CurrentCultureIgnoreCase)) != null)
        {
            LogHelper.Instance.Log("{0} The port {1} is reported in-use by the port manager.", "[Port Scanner]",
                portName);
            return null;
        }

        var redboxSerialPort = OnCreate(new SerialPort(portName, 115200, Parity.None, 8, StopBits.One), mode);
        redboxSerialPort.Configure(conf);
        LogHelper.Instance.Log("{0} try port {1}", "[Port Scanner]", portName);
        if (!redboxSerialPort.Open())
        {
            LogHelper.Instance.Log("{0} Could not open port {1}", "[Port Scanner]", portName);
            redboxSerialPort.Dispose();
            return null;
        }

        if (probe(redboxSerialPort))
        {
            LogHelper.Instance.Log("{0} Probe port {1} returned true", "[Port Scanner]", portName);
            return redboxSerialPort;
        }

        redboxSerialPort.Dispose();
        return null;
    }

    private bool RegisterUnderLock(ICommPort port)
    {
        var key = port.PortName.ToUpper();
        if (InUsePorts.Find(each => each.Equals(key, StringComparison.CurrentCultureIgnoreCase)) != null)
            return false;
        InUsePorts.Add(key);
        return true;
    }

    private RedboxSerialPort OnCreate(SerialPort port, CommPortReadModes mode)
    {
        var redboxSerialPort = (RedboxSerialPort)null;
        switch (mode)
        {
            case CommPortReadModes.Async:
                var asyncReadPort = new AsyncReadPort(port);
                asyncReadPort.Mode = CommPortReadModes.Async;
                redboxSerialPort = asyncReadPort;
                break;
            case CommPortReadModes.Callback:
                var callbackReadPort = new CallbackReadPort(port);
                callbackReadPort.Mode = CommPortReadModes.Callback;
                redboxSerialPort = callbackReadPort;
                break;
        }

        return redboxSerialPort;
    }

    private RedboxSerialPort ProbePorts(
        ICommChannelConfiguration conf,
        Predicate<ICommPort> probe,
        CommPortReadModes mode)
    {
        foreach (var portName in SerialPort.GetPortNames())
        {
            var redboxSerialPort = Probe(portName, conf, probe, mode);
            if (redboxSerialPort != null)
                return redboxSerialPort;
        }

        return null;
    }
}