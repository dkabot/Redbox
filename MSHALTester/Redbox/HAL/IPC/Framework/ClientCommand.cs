using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Compression;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Component.Model.Timers;
using Redbox.IPC.Framework;

namespace Redbox.HAL.IPC.Framework;

public class ClientCommand<T> where T : ClientCommandResult, new()
{
    private const string IPCLogsDirectory = "C:\\Program Files\\Redbox\\KioskLogs\\IPCFramework";
    private readonly IPCProtocol m_protocol;
    private TextWriter m_logFile;

    protected ClientCommand(IPCProtocol protocol, string command)
    {
        m_protocol = protocol;
        CommandText = command;
    }

    protected ClientCommand(IPCProtocol protocol, int? timeout, string command)
        : this(protocol, command)
    {
        Timeout = timeout;
    }

    protected ClientCommand(string command)
    {
        CommandText = command;
    }

    public string CommandText { get; internal set; }

    public TimeSpan? ExecutionTime { get; private set; }

    internal int? Timeout { get; set; }

    private TextWriter ErrorLogFile
    {
        get
        {
            if (m_logFile == null)
            {
                if (!Directory.Exists("C:\\Program Files\\Redbox\\KioskLogs\\IPCFramework"))
                    try
                    {
                        Directory.CreateDirectory("C:\\Program Files\\Redbox\\KioskLogs\\IPCFramework");
                    }
                    catch
                    {
                    }

                var currentProcess = Process.GetCurrentProcess();
                var path = Path.Combine("C:\\Program Files\\Redbox\\KioskLogs\\IPCFramework",
                    string.Format("IPCClientErrors-{0}-{1}.log", currentProcess.ProcessName, currentProcess.Id));
                try
                {
                    m_logFile = new StreamWriter(
                        File.Open(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
                }
                catch
                {
                    m_logFile = StreamWriter.Null;
                }
            }

            return m_logFile;
        }
    }

    public static T ExecuteCommand(IPCProtocol protocol, string command)
    {
        return new ClientCommand<T>(protocol, command).Execute();
    }

    public static T ExecuteCommand(IPCProtocol protocol, int? timeout, string command)
    {
        return new ClientCommand<T>(protocol, timeout, command).Execute();
    }

    public static T ExecuteCommand(IIpcClientSession session, string command)
    {
        return new ClientCommand<T>(command).Execute(session);
    }

    public T Execute()
    {
        using (var clientSession = ClientSessionFactory.GetClientSession(m_protocol))
        {
            return Execute(clientSession);
        }
    }

    public T Execute(IIpcClientSession session)
    {
        using (var executionTimer = new ExecutionTimer())
        {
            var obj = new T();
            obj.CommandText = CommandText;
            var result = obj;
            try
            {
                var timeout = Timeout;
                if (timeout.HasValue)
                {
                    var ipcClientSession = session;
                    timeout = Timeout;
                    var num = timeout.Value;
                    ipcClientSession.Timeout = num;
                }

                if (!session.IsConnected)
                    session.ConnectThrowOnError();
                var stringList = session.ExecuteCommand(CommandText);
                result.Success = session.IsStatusOK(stringList);
                if (stringList.Count > 0)
                {
                    result.StatusMessage = stringList[stringList.Count - 1];
                    stringList.RemoveAt(stringList.Count - 1);
                }

                result.CommandMessages.AddRange(stringList);
                foreach (var convertMessage in ConvertMessages(result))
                    if (convertMessage < result.CommandMessages.Count)
                        result.CommandMessages.RemoveAt(convertMessage);
            }
            catch (SocketException ex)
            {
                ErrorLogFile.WriteLine(
                    "ClientCommand.Execute() caught a socket exception; SocketErrorCode = {0}, Native = {1}",
                    ex.SocketErrorCode, ex.NativeErrorCode);
                ErrorLogFile.Flush();
                OnConnectionFailure(ex, result);
            }
            catch (TimeoutException ex)
            {
                ErrorLogFile.WriteLine("ClientCommand.Execute() caught a Timeout exception ( exception Message = {0} )",
                    ex.Message);
                ErrorLogFile.Flush();
                OnConnectionFailure(ex, result);
            }
            catch (Exception ex)
            {
                OnUnhandledException(ex, result);
            }
            finally
            {
                result.ExecutionTime = executionTimer.Elapsed;
                ExecutionTime = result.ExecutionTime;
            }

            return result;
        }
    }

    public override string ToString()
    {
        return string.Format("Command: {0}", CommandText);
    }

    protected virtual void OnConnectionFailure(Exception e, ClientCommandResult result)
    {
        result.Success = false;
        result.Errors.Add(Error.NewError("J001",
            string.Format("Unable to connect to command service on {0}:{1}.", m_protocol.Host, m_protocol.Port),
            "Check that the host and port values are correct and that the command service is running on the specified host."));
    }

    protected virtual void OnUnhandledException(Exception e, ClientCommandResult result)
    {
        result.Success = false;
        result.Errors.Add(Error.NewError("J999", "An unhandled exception was raised in ClientCommand.Execute.", e));
    }

    internal static List<int> ConvertMessages(T result)
    {
        var intList = new List<int>();
        for (var index1 = 0; index1 < result.CommandMessages.Count; ++index1)
            if (result.CommandMessages[index1].StartsWith("|*"))
            {
                intList.Add(index1);
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(result.CommandMessages[index1]);
                for (var index2 = index1 + 1; index2 < result.CommandMessages.Count; ++index2)
                {
                    intList.Add(index2);
                    stringBuilder.AppendLine(result.CommandMessages[index2]);
                    if (result.CommandMessages[index2].EndsWith("*|"))
                        break;
                }

                stringBuilder.Replace("|*", string.Empty);
                stringBuilder.Replace("*|", string.Empty);
                var str = stringBuilder.ToString();
                var codeFromBrackets = StringExtensions.ExtractCodeFromBrackets(str, "[", "]");
                if (codeFromBrackets != null)
                {
                    var strArray = str.Substring(codeFromBrackets.Length + 2).Split("|".ToCharArray());
                    if (strArray[0].IndexOf("WARNING") != -1)
                        result.Errors.Add(Error.NewWarning(codeFromBrackets, strArray[0].Trim(),
                            strArray.Length == 2 ? strArray[1].Trim() : string.Empty));
                    else
                        result.Errors.Add(Error.NewError(codeFromBrackets, strArray[0].Trim(),
                            strArray.Length == 2 ? strArray[1].Trim() : string.Empty));
                }
            }
            else if (result.CommandMessages[index1].StartsWith("LZMA|"))
            {
                var algorithm = CompressionAlgorithmFactory.GetAlgorithm(CompressionType.LZMA);
                result.CommandMessages[index1] = Encoding.ASCII.GetString(
                    algorithm.Decompress(StringExtensions.Base64ToBytes(result.CommandMessages[index1].Substring(5))));
            }
            else if (result.CommandMessages[index1].StartsWith("GZIP|"))
            {
                var algorithm = CompressionAlgorithmFactory.GetAlgorithm(CompressionType.GZip);
                result.CommandMessages[index1] = Encoding.ASCII.GetString(
                    algorithm.Decompress(StringExtensions.Base64ToBytes(result.CommandMessages[index1].Substring(5))));
            }

        return intList;
    }
}