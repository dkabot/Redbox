using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Redbox.Compression;
using Redbox.Core;

namespace Redbox.IPC.Framework
{
    public class ClientCommand<T> where T : ClientCommandResult, new()
    {
        private readonly IPCProtocol m_protocol;

        protected ClientCommand(IPCProtocol protocol, string command)
        {
            m_protocol = protocol;
            CommandText = command;
        }

        protected ClientCommand(ClientSession session, bool disposeWhenDone, string command)
        {
            Session = session;
            CommandText = command;
            DisposeWhenDone = disposeWhenDone;
            if (Session == null)
                return;
            m_protocol = Session.Protocol;
        }

        public string CommandText { get; internal set; }

        public int CommandTimeout { get; internal set; }

        public IPCProtocol Protocol { get; internal set; }

        public TimeSpan? ExecutionTime { get; private set; }

        internal bool DisposeWhenDone { get; set; }

        internal ClientSession Session { get; set; }

        public static T ExecuteCommand(ClientSession session, bool disposeWhenDone, string command)
        {
            return new ClientCommand<T>(session, disposeWhenDone, command).Execute();
        }

        public T Execute()
        {
            using (var executionTimer = new ExecutionTimer())
            {
                var obj = new T();
                obj.CommandText = CommandText;
                var result = obj;
                var flag = Session == null || DisposeWhenDone;
                try
                {
                    Session = Session ?? ClientSession.GetClientSession(Protocol, CommandTimeout);
                    if (!Session.IsConnected)
                        Session.ConnectThrowOnError();
                    var stringList = Session.ExecuteCommand(CommandText);
                    result.Success = Session.IsStatusOk(stringList);
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
                    try
                    {
                        if (flag)
                            if (Session != null)
                                Session.Dispose();
                    }
                    catch (Exception ex)
                    {
                        result.Success = false;
                        result.Errors.Add(Error.NewError("J999",
                            "An unhandled exception was raised in ClientCommand.Execute.", ex));
                    }
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
                string.Format("Unable to connect to command service on {0}://{1}:{2}.", m_protocol.Scheme,
                    m_protocol.Host, m_protocol.Port), e));
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
                    var codeFromBrackets = str.ExtractCodeFromBrackets("[", "]");
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
                    var algorithm = CompressionAlgorithm.GetAlgorithm(CompressionType.LZMA);
                    result.CommandMessages[index1] =
                        Encoding.ASCII.GetString(
                            algorithm.Decompress(result.CommandMessages[index1].Substring(5).Base64ToBytes()));
                }
                else if (result.CommandMessages[index1].StartsWith("GZIP|"))
                {
                    var algorithm = CompressionAlgorithm.GetAlgorithm(CompressionType.GZip);
                    result.CommandMessages[index1] =
                        Encoding.ASCII.GetString(
                            algorithm.Decompress(result.CommandMessages[index1].Substring(5).Base64ToBytes()));
                }

            return intList;
        }
    }
}