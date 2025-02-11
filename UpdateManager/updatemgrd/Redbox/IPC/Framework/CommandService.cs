using Redbox.Command.Tokenizer;
using Redbox.Core;
using Redbox.Macros;
using Redbox.Tokenizer.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;

namespace Redbox.IPC.Framework
{
    internal class CommandService
    {
        private readonly Dictionary<string, List<byte[]>> m_accessFilter;
        private static readonly PropertyDictionary m_properties = new PropertyDictionary();

        public static CommandService Instance => Singleton<CommandService>.Instance;

        public void SetAccesFilter(string ruleFile)
        {
            if (!File.Exists(ruleFile))
                return;
            using (StreamReader streamReader = new StreamReader(ruleFile))
            {
                string str;
                while ((str = streamReader.ReadLine()) != null)
                {
                    string[] strArray = str.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    this.SetAccessFilter(strArray[0].Trim(), strArray[1].Trim());
                }
            }
        }

        public void SetAccessFilter(string command, List<string> filters)
        {
            command = command.ToLower();
            if (!this.m_accessFilter.ContainsKey(command))
                this.m_accessFilter[command] = new List<byte[]>();
            this.m_accessFilter[command].AddRange((IEnumerable<byte[]>)filters.ConvertAll<byte[]>((Converter<string, byte[]>)(str =>
            {
                if (str == "*")
                    return new byte[0];
                string[] strArray = str.Split(new string[1] { "." }, StringSplitOptions.RemoveEmptyEntries);
                List<byte> byteList = new List<byte>();
                for (int index = 0; index < strArray.Length && !(strArray[index] == "*"); ++index)
                    byteList.Add(Convert.ToByte(strArray[index]));
                return byteList.ToArray();
            })));
            this.m_accessFilter[command].Sort((Comparison<byte[]>)((lhs, rhs) => lhs.Length.CompareTo(rhs.Length)));
        }

        public void SetAccessFilter(string command, string filter)
        {
            this.SetAccessFilter(command, new List<string>()
      {
        filter
      });
        }

        public string GetServerBusyResult(ExecutionTimer timer, int maximumThreads)
        {
            Interlocked.Increment(ref Statistics.Instance.NumberServerBusyResponses);
            CommandResult commandResult = new CommandResult();
            LogHelper.Instance.Log("Server too busy to handle request, maximum available threads: {0}.", (object)maximumThreads);
            commandResult.Errors.Add(Error.NewError("J777", "Server too busy to service request.", "Resubmit the command request when the server isn't so busy."));
            commandResult.ExecutionTime = timer.Elapsed;
            commandResult.Success = false;
            ProtocolHelper.FormatErrors(commandResult.Errors, (IList<string>)commandResult.Messages);
            return commandResult.ToString();
        }

        public CommandResult Execute(ISession session, string input)
        {
            return this.Execute(session, string.Empty, input);
        }

        public CommandResult Execute(ISession session, string originIP, string input)
        {
            return this.Execute(session, string.Empty, input, (List<string>)null, false, (Action<string, string>)null);
        }

        public CommandResult Execute(
          ISession session,
          string originIP,
          string input,
          List<string> sourceFilters,
          bool enableFilters,
          Action<string, string> paramAction)
        {
            using (ExecutionTimer executionTimer = new ExecutionTimer())
            {
                CommandResult result = new CommandResult();
                CommandContext context = new CommandContext()
                {
                    Session = session,
                    MessageSink = (IMessageSink)session
                };
                string command1 = (string)null;
                try
                {
                    CommandTokenizer tokenizer;
                    if (!CommandService.GetTokenizer(input, result, context, out tokenizer))
                        return result;
                    SimpleToken mnemonic = tokenizer.Tokens.GetMnemonic();
                    CommandInstance command2 = CommandRepository.GetCommand(mnemonic.Value);
                    if (command2 == null)
                    {
                        LogHelper.Instance.Log(string.Format("Command {0} not recognized.", (object)mnemonic.Value));
                        result.Errors.Add(Error.NewError("S514", string.Format("Invalid Command: {0}", (object)mnemonic.Value), ""));
                        return result;
                    }
                    if (!command2.IsInFilter(enableFilters, sourceFilters))
                    {
                        LogHelper.Instance.Log(string.Format("Command {0} is not in filters.", (object)mnemonic.Value));
                        result.Errors.Add(Error.NewError("S514", string.Format("Invalid Command: {0}", (object)mnemonic.Value), ""));
                    }
                    if (command2.FormMethodCache.Count == 0)
                    {
                        LogHelper.Instance.Log(string.Format("Command {0} does not have any valid executable forms.", (object)mnemonic.Value));
                        result.Errors.Add(Error.NewError("S514", string.Format("Invalid Command: {0}", (object)mnemonic.Value), ""));
                        return result;
                    }
                    if (!string.IsNullOrEmpty(originIP) && !this.AllowOriginForCommand(originIP, mnemonic.Value))
                    {
                        LogHelper.Instance.Log(string.Format("Command {0} does allow execution from {1}.", (object)mnemonic.Value, (object)originIP));
                        result.Errors.Add(Error.NewError("S514", string.Format("Invalid Command: {0}", (object)mnemonic.Value), ""));
                        return result;
                    }
                    SimpleTokenList symbols = tokenizer.Tokens.GetSymbols();
                    if (symbols.Count == 0 && command2.HasDefault() || command2.HasOnlyDefault())
                    {
                        LogHelper.Instance.Log(input, LogEntryType.Info);
                        command2.InvokeDefault(result, context, tokenizer, enableFilters, sourceFilters);
                        return result;
                    }
                    if (symbols.Count == 0)
                    {
                        result.Errors.Add(Error.NewError("S001", string.Format("No form symbol was specified for the {0} comand.", (object)mnemonic.Value.ToUpper()), string.Format("Use the HELP command to learn how to properly invoke the {0} command and its forms.", (object)mnemonic.Value.ToUpper())));
                        return result;
                    }
                    FormMethod method = command2.GetMethod(symbols[0].Value);
                    if (method == null)
                    {
                        result.Errors.Add(Error.NewError("S001", string.Format("An unknown form was used for the {0} comand.", (object)mnemonic.Value.ToUpper()), string.Format("Use the HELP command to learn how to properly invoke the {0} command and its forms.", (object)mnemonic.Value.ToUpper())));
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
                    command1 = string.Format("{0} {1}", (object)mnemonic.Value.ToUpper(), (object)symbols[0].Value.ToLower());
                    PerformanceCounterHelper.Instance.IncrementCommandPerSecond(command1);
                    method.Invoke(result, context, tokenizer, command2.GetInstance(), enableFilters, sourceFilters);
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log("CommandService.Execute error", ex);
                    result.Errors.Add(Error.NewError("S999", "An unhandled exception was raised in CommandService.Execute.", ex));
                }
                finally
                {
                    result.ExecutionTime = executionTimer.Elapsed;
                    result.Success = !context.Errors.ContainsError() && !result.Errors.ContainsError();
                    result.Messages.AddRange((IEnumerable<string>)context.Messages);
                    result.Errors.AddRange((IEnumerable<Error>)context.Errors);
                    Statistics.Instance.TrackCommandStatistics(result.ExecutionTime);
                    if (command1 != null)
                        PerformanceCounterHelper.Instance.IncrementCommandExecutionTime(command1, executionTimer.ElapsedTicks);
                    LogHelper.Instance.Log(result.ToString(this.IsResultLoggingEnabled()));
                    if (result.Errors.Count > 0)
                    {
                        StringBuilder builder = new StringBuilder();
                        result.Errors.ForEach((Action<Error>)(error =>
                        {
                            builder.AppendFormat("{0} {1}", (object)error.Code, (object)error.Description);
                            builder.AppendFormat(error.Details);
                        }));
                        LogHelper.Instance.Log(builder.ToString());
                    }
                }
                return result;
            }
        }

        public PropertyDictionary Properties => CommandService.m_properties;

        private CommandService()
        {
            LogHelper.Instance.Log("Initializing CommandService.", LogEntryType.Info);
            foreach (string installedCommand in CommandRepository.DiscoverInstalledCommands())
                LogHelper.Instance.Log("Loaded command: " + installedCommand, LogEntryType.Info);
            this.m_accessFilter = new Dictionary<string, List<byte[]>>();
        }

        private bool IsResultLoggingEnabled()
        {
            return ConfigurationManager.AppSettings["IpcLogResults"] != null && bool.Parse(ConfigurationManager.AppSettings["IpcLogResults"]);
        }

        private bool AllowOriginForCommand(string origin, string command)
        {
            LogHelper.Instance.Log(string.Format("Checking access rules for {0}.", (object)origin), LogEntryType.Debug);
            command = command.ToLower();
            if (!this.m_accessFilter.ContainsKey(command))
                return true;
            string[] strArray = origin.Split('.');
            byte[] numArray1 = new byte[strArray.Length];
            for (int index = 0; index < strArray.Length; ++index)
                numArray1[index] = Convert.ToByte(strArray[index]);
            foreach (byte[] numArray2 in this.m_accessFilter[command])
            {
                bool flag = true;
                for (int index = 0; index < numArray2.Length; ++index)
                {
                    if ((int)numArray2[index] != (int)numArray1[index])
                    {
                        flag = false;
                        break;
                    }
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
            input = CommandService.m_properties.ExpandProperties(input, Location.UnknownLocation);
            tokenizer = new CommandTokenizer(0, input);
            tokenizer.Tokenize();
            if (tokenizer.Tokens.HasOnlyComments())
                return false;
            if (tokenizer.Errors.ContainsError() || tokenizer.Tokens.GetMnemonic() == null)
            {
                foreach (Redbox.Tokenizer.Framework.Error error in (List<Redbox.Tokenizer.Framework.Error>)tokenizer.Errors)
                    result.Errors.Add(error.IsWarning ? Error.NewWarning(error.Code, error.Description, error.Details) : Error.NewError(error.Code, error.Description, error.Details));
                result.ExtendedErrorMessage = string.Format("Parsing of input '{0}' failed. Correct the command syntax and try again.", (object)input);
                return false;
            }
            foreach (SimpleToken symbol in (List<SimpleToken>)tokenizer.Tokens.GetSymbols())
                context.Parameters[symbol.Value] = symbol.Value;
            foreach (SimpleToken keyValuePair1 in (List<SimpleToken>)tokenizer.Tokens.GetKeyValuePairs())
            {
                if (keyValuePair1.ConvertValue() is KeyValuePair keyValuePair2)
                    context.Parameters[string.Format("{0}:", (object)keyValuePair2.Key)] = keyValuePair2.Value;
            }
            return true;
        }
    }
}
