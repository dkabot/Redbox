using System;
using System.IO.Ports;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Core;

internal abstract class RedboxSerialPort : ICommPort, IDisposable
{
    protected readonly SerialPort Port;
    protected readonly IRuntimeService RuntimeService;
    private bool Disposed;
    private string m_displayName;

    protected RedboxSerialPort(SerialPort port)
    {
        Port = port != null ? port : throw new ArgumentNullException(nameof(port));
        RuntimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
        WritePause = 10;
        ReadTimeout = -1;
        WriteTimeout = 5000;
        OpenPause = 3000;
        m_displayName = Port.PortName;
    }

    public CommPortReadModes Mode { get; internal set; }

    public bool IsOpen => Port != null && PortIsOpen();

    public string PortName => Port != null ? Port.PortName : throw new NullReferenceException();

    public bool EnableDebugging { get; set; }

    public byte[] WriteTerminator { get; set; }

    public int OpenPause { get; set; }

    public int WritePause { get; set; }

    public string DisplayName
    {
        get => m_displayName;
        set
        {
            if (string.IsNullOrEmpty(value))
                m_displayName = (string)Port.PortName.Clone();
            else
                m_displayName = string.Format("{0} ( {1} )", Port.PortName, value);
        }
    }

    public int ReadTimeout { get; set; }

    public int WriteTimeout { get; set; }

    public int? ReceiveBufferSize { get; set; }

    public Predicate<IChannelResponse> ValidateResponse { get; set; }

    public void Dispose()
    {
        if (EnableDebugging)
            LogHelper.Instance.Log("[RedboxSerialPort, {0}] dispose port", DisplayName);
        Dispose(true);
    }

    public void Configure(ICommChannelConfiguration configuration)
    {
        ReceiveBufferSize = configuration.ReceiveBufferSize;
        WriteTerminator = configuration.WriteTerminator;
        WritePause = configuration.WritePause;
        WriteTimeout = configuration.WriteTimeout;
        OpenPause = configuration.OpenPause;
        ReadTimeout = configuration.ReadTimeout;
        if (configuration.ReceiveBufferSize.HasValue)
            Port.ReadBufferSize = ReceiveBufferSize.Value;
        OnConfigure();
    }

    public bool Open()
    {
        if (Port == null)
        {
            LogHelper.Instance.Log("[RedboxSerialPort] no port is configured.");
            return false;
        }

        if (PortIsOpen())
            return true;
        OnPreOpenPort();
        try
        {
            Port.Open();
            Port.ReadTimeout = ReadTimeout;
            Port.WriteTimeout = WriteTimeout;
            Port.RtsEnable = true;
            Port.DtrEnable = true;
            RuntimeService.Wait(OpenPause);
            LogHelper.Instance.Log("[RedboxSerialPort, {0}] Port is open in mode {1}.", DisplayName, Mode);
            OnPortOpen();
        }
        catch (Exception ex)
        {
            LogHelper.Instance.Log(string.Format("[RedbosSerialPort, {0}] Open() caught an exception.", DisplayName),
                ex);
            return false;
        }

        if (ResetPortBuffers(true))
            return true;
        Close();
        return false;
    }

    public bool Close()
    {
        if (!Port.IsOpen)
            return false;
        try
        {
            Port.RtsEnable = true;
            Port.DtrEnable = false;
            Port.Close();
            LogHelper.Instance.Log("[RedboxSerialPort, {0}] Port is closed.", DisplayName);
            OnPortClosed();
            return true;
        }
        catch (Exception ex)
        {
            LogHelper.Instance.Log(string.Format("[RedboxSerialPort, {0}] Close caught an exception.", DisplayName),
                ex);
            return false;
        }
    }

    public IChannelResponse SendRecv(byte[] bytes, int readTimeout)
    {
        ResetPortBuffers(true);
        if (!Port.IsOpen)
        {
            LogHelper.Instance.Log(LogEntryType.Error, "[RedboxSerialPort, {0}] Send/Recv: the port is not open.",
                DisplayName);
            return new ChannelResponse
            {
                Error = ErrorCodes.CommunicationError
            };
        }

        if (EnableDebugging)
            LogHelper.Instance.Log("[RedboxSerialPort, {0}] Send command {1}", DisplayName, bytes.AsString());
        var response = OnSendReceive(bytes, readTimeout);
        LogResponse(response);
        return response;
    }

    public IChannelResponse SendRecv(string command, int readTimeout)
    {
        ResetPortBuffers(true);
        if (!Port.IsOpen)
        {
            LogHelper.Instance.Log(LogEntryType.Error, "[RedboxSerialPort, {0}] Send/Recv: the port is not open.",
                DisplayName);
            return new ChannelResponse
            {
                Error = ErrorCodes.CommunicationError
            };
        }

        if (EnableDebugging)
            LogHelper.Instance.Log("[RedboxSerialPort, {0}] Send command {1}", DisplayName, command);
        var response = OnSendReceive(Encoding.ASCII.GetBytes(command), readTimeout);
        LogResponse(response);
        return response;
    }

    ~RedboxSerialPort()
    {
        if (EnableDebugging)
            LogHelper.Instance.Log("[RedboxSerialPort, {0}] finalize port ", DisplayName);
        Dispose(false);
    }

    protected virtual void OnPreOpenPort()
    {
    }

    protected virtual void OnPortOpen()
    {
    }

    protected virtual void OnConfigure()
    {
    }

    protected virtual void OnPortClosed()
    {
    }

    protected virtual void OnDispose()
    {
    }

    protected abstract IChannelResponse OnSendReceive(byte[] command, int timeout);

    protected bool ResetPortBuffers(bool resetOut)
    {
        try
        {
            if (!Port.IsOpen)
            {
                LogHelper.Instance.Log("[RedbosSerialPort, {0}] ResetPortBuffers: port is closed.", DisplayName);
                return false;
            }

            Port.DiscardInBuffer();
            if (resetOut)
                Port.DiscardOutBuffer();
            return true;
        }
        catch (Exception ex)
        {
            LogHelper.Instance.Log(string.Format("[RedbosSerialPort] Reset buffer caught an exception.", DisplayName),
                ex);
            return false;
        }
    }

    protected bool PortIsOpen()
    {
        try
        {
            return Port.IsOpen;
        }
        catch (Exception ex)
        {
            LogHelper.Instance.Log(LogEntryType.Error,
                string.Format("[RedboxCommPort, {0}] IsOpen generated an unhandled exception.", DisplayName), ex);
            return false;
        }
    }

    private void LogResponse(IChannelResponse response)
    {
        if (!EnableDebugging && response.CommOk)
            return;
        LogHelper.Instance.Log("[RedboxCommPort, {0}] Read {1}, bytes in buffer {2}", DisplayName,
            !response.CommOk ? "timed out" : (object)"ok", response.RawResponse.Length);
        response.RawResponse.Dump();
    }

    private void Dispose(bool fromDispose)
    {
        if (Disposed)
            return;
        Disposed = true;
        OnDispose();
        Close();
        Port.Dispose();
        GC.SuppressFinalize(this);
    }
}