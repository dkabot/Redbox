using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.IPC.Framework;

namespace Redbox.HAL.IPC.Framework;

public abstract class ClientSession : IIpcClientSession, IDisposable
{
    private readonly AutoResetEvent m_resetEvent = new(false);
    private StringBuilder m_readBuilder;
    private int? m_timeout;

    protected ClientSession(IPCProtocol protocol)
    {
        Protocol = protocol;
    }

    protected abstract Stream Stream { get; }

    protected byte[] ReadBuffer { get; set; }

    private StringBuilder ReadBuilder
    {
        get
        {
            if (m_readBuilder == null)
                m_readBuilder = new StringBuilder();
            return m_readBuilder;
        }
    }

    public virtual void Dispose()
    {
        Close();
        m_resetEvent.Close();
        IsDisposed = true;
        if (Disposed == null)
            return;
        Disposed(this, EventArgs.Empty);
    }

    public void ConnectThrowOnError()
    {
        OnConnectThrowOnError();
    }

    public bool IsStatusOK(List<string> messages)
    {
        var flag = false;
        if (messages.Count > 0)
            flag = messages[messages.Count - 1].StartsWith("203 Command");
        return flag;
    }

    public List<string> ExecuteCommand(string command)
    {
        Write(command);
        return ConsumeMessages();
    }

    public int Timeout
    {
        get => m_timeout ?? 30000;
        set => m_timeout = value;
    }

    public bool IsConnected { get; internal set; }

    public IIpcProtocol Protocol { get; }

    public bool IsDisposed { get; private set; }

    public event Action<string> ServerEvent;

    public void Close()
    {
        if (!IsConnected)
            return;
        try
        {
            ExecuteCommand("quit");
        }
        catch (IOException ex)
        {
        }
        finally
        {
            IsConnected = false;
            CustomClose();
        }
    }

    public event EventHandler Disposed;

    protected abstract void OnConnectThrowOnError();

    protected List<string> ConsumeMessages()
    {
        if (ReadBuilder.Length > 0)
            ReadBuilder.Length = 0;
        var messages = new List<string>();
        Read(messages);
        if (!m_resetEvent.WaitOne(Timeout))
        {
            messages.Clear();
            var errors = new ErrorList();
            errors.Add(Error.NewError("J888", string.Format("Timeout threshold {0} exceeded.", Timeout),
                "Reissue the command when the service is not as busy."));
            ProtocolHelper.FormatErrors(errors, messages);
            messages.Add("545 Command failed.");
        }

        return messages;
    }

    protected internal abstract bool IsConnectionAvailable();

    protected internal abstract void CustomClose();

    protected abstract int GetAvailableData();

    protected abstract bool CanRead();

    private void Read(List<string> messages)
    {
        try
        {
            Stream.BeginRead(ReadBuffer, 0, ReadBuffer.Length, EndReadCallback, messages);
        }
        catch (IOException ex)
        {
            LogHelper.Instance.Log("ClientSession.Read caught an I/O exception; maybe server is down?");
            SetEvent();
        }
    }

    private void Write(string s)
    {
        try
        {
            if (!Stream.CanWrite)
                return;
            if (!s.EndsWith(Environment.NewLine))
                s = string.Format("{0}{1}", s, Environment.NewLine);
            Stream.Write(Encoding.ASCII.GetBytes(s), 0, s.Length);
        }
        catch (IOException ex)
        {
            LogHelper.Instance.Log("ClientSession.Write caught an I/O exception; maybe server is down?");
        }
    }

    private string GetNextBufferLine()
    {
        var str1 = ReadBuilder.ToString();
        var num = str1.IndexOf(Environment.NewLine);
        if (num == -1)
            return string.Empty;
        var str2 = str1.Substring(0, num + Environment.NewLine.Length);
        ReadBuilder.Remove(0, num + Environment.NewLine.Length);
        return str2.Trim();
    }

    private bool BufferHasMoreLines()
    {
        return ReadBuilder.ToString().IndexOf(Environment.NewLine) == -1;
    }

    private void EndReadCallback(IAsyncResult result)
    {
        try
        {
            var count = Stream.EndRead(result);
            if (count == 0)
            {
                SetEvent();
            }
            else
            {
                var asyncState = (List<string>)result.AsyncState;
                var str = Encoding.ASCII.GetString(ReadBuffer, 0, count);
                ReadBuilder.Append(str);
                if (str.IndexOf(Environment.NewLine) == -1)
                    Read(asyncState);
                else if (!ProcessReadBuilder(asyncState))
                    Read(asyncState);
                else
                    SetEvent();
            }
        }
        catch (Exception ex)
        {
            LogHelper.Instance.Log("ClientSession.EndRead caught an exception; maybe server is down?");
            SetEvent();
        }
    }

    private bool ProcessReadBuilder(ICollection<string> messages)
    {
        string nextBufferLine;
        do
        {
            nextBufferLine = GetNextBufferLine();
            if (nextBufferLine == string.Empty && BufferHasMoreLines())
                return false;
            if (nextBufferLine.StartsWith("[MSG]"))
            {
                if (ServerEvent != null)
                {
                    var num = nextBufferLine.IndexOf("]");
                    if (num != -1)
                        ServerEvent(nextBufferLine.Substring(num + 1).Trim());
                }
            }
            else
            {
                messages.Add(nextBufferLine);
            }
        } while (!nextBufferLine.StartsWith("Welcome!") && !nextBufferLine.StartsWith("Goodbye!") &&
                 !nextBufferLine.StartsWith("545 Command") && !nextBufferLine.StartsWith("203 Command"));

        return true;
    }

    private bool SetEvent()
    {
        try
        {
            m_resetEvent.Set();
            return true;
        }
        catch (Exception ex)
        {
            LogHelper.Instance.Log("Caught an exception setting the event.", ex);
            return false;
        }
    }
}