using Redbox.Compression;
using Redbox.Core;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Redbox.IPC.Framework
{
    internal class ClientCommand<T> where T : ClientCommandResult, new()
    {
        private IPCProtocol m_protocol;

        public static T ExecuteCommand(ClientSession session, bool disposeWhenDone, string command)
        {
            return new ClientCommand<T>(session, disposeWhenDone, command).Execute();
        }

        public T Execute()
        {
            using (ExecutionTimer executionTimer = new ExecutionTimer())
            {
                T obj = new T();
                obj.CommandText = this.CommandText;
                T result = obj;
                bool flag = this.Session == null || this.DisposeWhenDone;
                try
                {
                    this.Session = this.Session ?? ClientSession.GetClientSession(this.Protocol, new int?(this.CommandTimeout));
                    if (!this.Session.IsConnected)
                        this.Session.ConnectThrowOnError();
                    List<string> stringList1 = this.Session.ExecuteCommand(this.CommandText);
                    result.Success = this.Session.IsStatusOk(stringList1);
                    if (stringList1.Count > 0)
                    {
                        T local = result;
                        List<string> stringList2 = stringList1;
                        string str = stringList2[stringList2.Count - 1];
                        local.StatusMessage = str;
                        List<string> stringList3 = stringList1;
                        stringList3.RemoveAt(stringList3.Count - 1);
                    }
                    result.CommandMessages.AddRange((IEnumerable<string>)stringList1);
                    foreach (int convertMessage in ClientCommand<T>.ConvertMessages(result))
                    {
                        if (convertMessage < result.CommandMessages.Count)
                            result.CommandMessages.RemoveAt(convertMessage);
                    }
                }
                catch (SocketException ex)
                {
                    this.OnConnectionFailure((Exception)ex, (ClientCommandResult)result);
                }
                catch (Exception ex)
                {
                    this.OnUnhandledException(ex, (ClientCommandResult)result);
                }
                finally
                {
                    result.ExecutionTime = executionTimer.Elapsed;
                    this.ExecutionTime = new TimeSpan?(result.ExecutionTime);
                    try
                    {
                        if (flag)
                        {
                            if (this.Session != null)
                                this.Session.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Success = false;
                        result.Errors.Add(Error.NewError("J999", "An unhandled exception was raised in ClientCommand.Execute.", ex));
                    }
                }
                return result;
            }
        }

        public override string ToString() => string.Format("Command: {0}", (object)this.CommandText);

        public string CommandText { get; internal set; }

        public int CommandTimeout { get; internal set; }

        public IPCProtocol Protocol { get; internal set; }

        public TimeSpan? ExecutionTime { get; private set; }

        protected ClientCommand(IPCProtocol protocol, string command)
        {
            this.m_protocol = protocol;
            this.CommandText = command;
        }

        protected ClientCommand(ClientSession session, bool disposeWhenDone, string command)
        {
            this.Session = session;
            this.CommandText = command;
            this.DisposeWhenDone = disposeWhenDone;
            if (this.Session == null)
                return;
            this.m_protocol = this.Session.Protocol;
        }

        protected virtual void OnConnectionFailure(Exception e, ClientCommandResult result)
        {
            result.Success = false;
            result.Errors.Add(Error.NewError("J001", string.Format("Unable to connect to command service on {0}://{1}:{2}.", (object)this.m_protocol.Scheme, (object)this.m_protocol.Host, (object)this.m_protocol.Port), e));
        }

        protected virtual void OnUnhandledException(Exception e, ClientCommandResult result)
        {
            result.Success = false;
            result.Errors.Add(Error.NewError("J999", "An unhandled exception was raised in ClientCommand.Execute.", e));
        }

        internal static List<int> ConvertMessages(T result)
        {
            List<int> intList = new List<int>();
            for (int index1 = 0; index1 < result.CommandMessages.Count; ++index1)
            {
                if (result.CommandMessages[index1].StartsWith("|*"))
                {
                    intList.Add(index1);
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine(result.CommandMessages[index1]);
                    for (int index2 = index1 + 1; index2 < result.CommandMessages.Count; ++index2)
                    {
                        intList.Add(index2);
                        stringBuilder.AppendLine(result.CommandMessages[index2]);
                        if (result.CommandMessages[index2].EndsWith("*|"))
                            break;
                    }
                    stringBuilder.Replace("|*", string.Empty);
                    stringBuilder.Replace("*|", string.Empty);
                    string str = stringBuilder.ToString();
                    string codeFromBrackets = str.ExtractCodeFromBrackets("[", "]");
                    if (codeFromBrackets != null)
                    {
                        string[] strArray = str.Substring(codeFromBrackets.Length + 2).Split("|".ToCharArray());
                        if (strArray[0].IndexOf("WARNING") != -1)
                            result.Errors.Add(Error.NewWarning(codeFromBrackets, strArray[0].Trim(), strArray.Length == 2 ? strArray[1].Trim() : string.Empty));
                        else
                            result.Errors.Add(Error.NewError(codeFromBrackets, strArray[0].Trim(), strArray.Length == 2 ? strArray[1].Trim() : string.Empty));
                    }
                }
                else if (result.CommandMessages[index1].StartsWith("LZMA|"))
                {
                    CompressionAlgorithm algorithm = CompressionAlgorithm.GetAlgorithm(CompressionType.LZMA);
                    result.CommandMessages[index1] = Encoding.ASCII.GetString(algorithm.Decompress(result.CommandMessages[index1].Substring(5).Base64ToBytes()));
                }
                else if (result.CommandMessages[index1].StartsWith("GZIP|"))
                {
                    CompressionAlgorithm algorithm = CompressionAlgorithm.GetAlgorithm(CompressionType.GZip);
                    result.CommandMessages[index1] = Encoding.ASCII.GetString(algorithm.Decompress(result.CommandMessages[index1].Substring(5).Base64ToBytes()));
                }
            }
            return intList;
        }

        internal bool DisposeWhenDone { get; set; }

        internal ClientSession Session { get; set; }
    }
}
