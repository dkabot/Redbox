using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using Redbox.Command.Tokenizer;
using Redbox.Core;
using Redbox.Macros;
using Redbox.Tokenizer.Framework;

namespace Redbox.IPC.Framework
{
    public class CommandService
    {
        private static readonly PropertyDictionary m_properties = new PropertyDictionary();
        private readonly Dictionary<string, List<byte[]>> m_accessFilter;

        private CommandService()
        {
            LogHelper.Instance.Log("Initializing CommandService.", LogEntryType.Info);
            foreach (var installedCommand in CommandRepository.DiscoverInstalledCommands())
                LogHelper.Instance.Log("Loaded command: " + installedCommand, LogEntryType.Info);
            m_accessFilter = new Dictionary<string, List<byte[]>>();
        }

        public static CommandService Instance => Singleton<CommandService>.Instance;

        public PropertyDictionary Properties => m_properties;

        public void SetAccesFilter(string ruleFile)
        {
            if (!File.Exists(ruleFile))
                return;
            using (var streamReader = new StreamReader(ruleFile))
            {
                string str;
                while ((str = streamReader.ReadLine()) != null)
                {
                    var strArray = str.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    SetAccessFilter(strArray[0].Trim(), strArray[1].Trim());
                }
            }
        }

        public void SetAccessFilter(string command, List<string> filters)
        {
            command = command.ToLower();
            if (!m_accessFilter.ContainsKey(command))
                m_accessFilter[command] = new List<byte[]>();
            m_accessFilter[command].AddRange(filters.ConvertAll(str =>
            {
                if (str == "*")
                    return new byte[0];
                var strArray = str.Split(new string[1] { "." }, StringSplitOptions.RemoveEmptyEntries);
                var byteList = new List<byte>();
                for (var index = 0; index < strArray.Length && !(strArray[index] == "*"); ++index)
                    byteList.Add(Convert.ToByte(strArray[index]));
                return byteList.ToArray();
            }));
            m_accessFilter[command].Sort((lhs, rhs) => lhs.Length.CompareTo(rhs.Length));
        }

        public void SetAccessFilter(string command, string filter)
        {
            SetAccessFilter(command, new List<string>
            {
                filter
            });
        }

        public string GetServerBusyResult(ExecutionTimer timer, int maximumThreads)
        {
            Interlocked.Increment(ref Statistics.Instance.NumberServerBusyResponses);
            var commandResult = new CommandResult();
            LogHelper.Instance.Log("Server too busy to handle request, maximum available threads: {0}.",
                maximumThreads);
            commandResult.Errors.Add(Error.NewError("J777", "Server too busy to service request.",
                "Resubmit the command request when the server isn't so busy."));
            commandResult.ExecutionTime = timer.Elapsed;
            commandResult.Success = false;
            ProtocolHelper.FormatErrors(commandResult.Errors, commandResult.Messages);
            return commandResult.ToString();
        }

        public CommandResult Execute(ISession session, string input)
        {
            return Execute(session, string.Empty, input);
        }

        public CommandResult Execute(ISession session, string originIP, string input)
        {
            return Execute(session, string.Empty, input, null, false, null);
        }

        public CommandResult Execute(
            ISession session,
            string originIP,
            string input,
            List<string> sourceFilters,
            bool enableFilters,
            Action<string, string> paramAction)
        {
            using (var executionTimer = new ExecutionTimer())
            {
                var result = new CommandResult();
                var context = new CommandContext
                {
                    Session = session,
                    MessageSink = session
                };
                var command1 = (string)null;
                try
                {
                    CommandTokenizer tokenizer;
                    if (!GetTokenizer(input, result, context, out tokenizer))
                        return result;
                    var mnemonic = tokenizer.Tokens.GetMnemonic();
                    var command2 = CommandRepository.GetCommand(mnemonic.Value);
                    if (command2 == null)
                    {
                        LogHelper.Instance.Log(string.Format("Command {0} not recognized.", mnemonic.Value));
                        result.Errors.Add(Error.NewError("S514", string.Format("Invalid Command: {0}", mnemonic.Value),
                            ""));
                        return result;
                    }

                    if (!command2.IsInFilter(enableFilters, sourceFilters))
                    {
                        LogHelper.Instance.Log(string.Format("Command {0} is not in filters.", mnemonic.Value));
                        result.Errors.Add(Error.NewError("S514", string.Format("Invalid Command: {0}", mnemonic.Value),
                            ""));
                    }

                    if (command2.FormMethodCache.Count == 0)
                    {
                        LogHelper.Instance.Log(string.Format("Command {0} does not have any valid executable forms.",
                            mnemonic.Value));
                        result.Errors.Add(Error.NewError("S514", string.Format("Invalid Command: {0}", mnemonic.Value),
                            ""));
                        return result;
                    }

                    if (!string.IsNullOrEmpty(originIP) && !AllowOriginForCommand(originIP, mnemonic.Value))
                    {
                        LogHelper.Instance.Log(string.Format("Command {0} does allow execution from {1}.",
                            mnemonic.Value, originIP));
                        result.Errors.Add(Error.NewError("S514", string.Format("Invalid Command: {0}", mnemonic.Value),
                            ""));
                        return result;
                    }

                    var symbols = tokenizer.Tokens.GetSymbols();
                    if ((symbols.Count == 0 && command2.HasDefault()) || command2.HasOnlyDefault())
                    {
                        LogHelper.Instance.Log(input, LogEntryType.Info);
                        command2.InvokeDefault(result, context, tokenizer, enableFilters, sourceFilters);
                        return result;
                    }

                    if (symbols.Count == 0)
                    {
                        result.Errors.Add(Error.NewError("S001",
                            string.Format("No form symbol was specified for the {0} comand.", mnemonic.Value.ToUpper()),
                            string.Format(
                                "Use the HELP command to learn how to properly invoke the {0} command and its forms.",
                                mnemonic.Value.ToUpper())));
                        return result;
                    }

                    var method = command2.GetMethod(symbols[0].Value);
                    if (method == null)
                    {
                        result.Errors.Add(Error.NewError("S001",
                            string.Format("An unknown form was used for the {0} comand.", mnemonic.Value.ToUpper()),
                            string.Format(
                                "Use the HELP command to learn how to properly invoke the {0} command and its forms.",
                                mnemonic.Value.ToUpper())));
                        return result;
                    }

                    if (!method.ValidateParameters(result, tokenizer, paramAction))
                    {
                        if (method.Loggable)
                            LogHelper.Instance.Log(input, LogEntryType.Info);
                        return result;
                    }

                    if (method.Loggable)
                        LogHelper.Instance.Log(input, LogEntryType.Info);
                    command1 = string.Format("{0} {1}", mnemonic.Value.ToUpper(), symbols[0].Value.ToLower());
                    PerformanceCounterHelper.Instance.IncrementCommandPerSecond(command1);
                    method.Invoke(result, context, tokenizer, command2.GetInstance(), enableFilters, sourceFilters);
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log("CommandService.Execute error", ex);
                    result.Errors.Add(Error.NewError("S999",
                        "An unhandled exception was raised in CommandService.Execute.", ex));
                }
                finally
                {
                    result.ExecutionTime = executionTimer.Elapsed;
                    result.Success = !context.Errors.ContainsError() && !result.Errors.ContainsError();
                    result.Messages.AddRange(context.Messages);
                    result.Errors.AddRange(context.Errors);
                    Statistics.Instance.TrackCommandStatistics(result.ExecutionTime);
                    if (command1 != null)
                        PerformanceCounterHelper.Instance.IncrementCommandExecutionTime(command1,
                            executionTimer.ElapsedTicks);
                    LogHelper.Instance.Log(result.ToString(IsResultLoggingEnabled()));
                    if (result.Errors.Count > 0)
                    {
                        var builder = new StringBuilder();
                        result.Errors.ForEach(error =>
                        {
                            builder.AppendFormat("{0} {1}", error.Code, error.Description);
                            builder.AppendFormat(error.Details);
                        });
                        LogHelper.Instance.Log(builder.ToString());
                    }
                }

                return result;
            }
        }

        private bool IsResultLoggingEnabled()
        {
            return ConfigurationManager.AppSettings["IpcLogResults"] != null &&
                   bool.Parse(ConfigurationManager.AppSettings["IpcLogResults"]);
        }

        private bool AllowOriginForCommand(string origin, string command)
        {
            LogHelper.Instance.Log(string.Format("Checking access rules for {0}.", origin), LogEntryType.Debug);
            command = command.ToLower();
            if (!m_accessFilter.ContainsKey(command))
                return true;
            var strArray = origin.Split('.');
            var numArray1 = new byte[strArray.Length];
            for (var index = 0; index < strArray.Length; ++index)
                numArray1[index] = Convert.ToByte(strArray[index]);
            foreach (var numArray2 in m_accessFilter[command])
            {
                var flag = true;
                for (var index = 0; index < numArray2.Length; ++index)
                    if (numArray2[index] != numArray1[index])
                    {
                        flag = false;
                        break;
                    }

                if (flag)
                    return true;
            }

            return false;
        }

        private static bool GetTokenizer(
            string input,
            CommandResult result,
            CommandContext context,
            out CommandTokenizer tokenizer)
        {
            input = m_properties.ExpandProperties(input, Location.UnknownLocation);
            tokenizer = new CommandTokenizer(0, input);
            tokenizer.Tokenize();
            if (tokenizer.Tokens.HasOnlyComments())
                return false;
            if (tokenizer.Errors.ContainsError() || tokenizer.Tokens.GetMnemonic() == null)
            {
                foreach (var error in tokenizer.Errors)
                    result.Errors.Add(error.IsWarning
                        ? Error.NewWarning(error.Code, error.Description, error.Details)
                        : Error.NewError(error.Code, error.Description, error.Details));
                result.ExtendedErrorMessage =
                    string.Format("Parsing of input '{0}' failed. Correct the command syntax and try again.", input);
                return false;
            }

            foreach (var symbol in tokenizer.Tokens.GetSymbols())
                context.Parameters[symbol.Value] = symbol.Value;
            foreach (var keyValuePair1 in tokenizer.Tokens.GetKeyValuePairs())
                if (keyValuePair1.ConvertValue() is KeyValuePair keyValuePair2)
                    context.Parameters[string.Format("{0}:", keyValuePair2.Key)] = keyValuePair2.Value;
            return true;
        }
    }
}