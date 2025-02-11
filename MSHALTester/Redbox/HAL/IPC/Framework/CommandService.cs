using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;
using Redbox.HAL.Core;

namespace Redbox.HAL.IPC.Framework;

public class CommandService
{
    private CommandService()
    {
        LogHelper.Instance.Log("Initializing CommandService.");
        CommandRepository.DiscoverInstalledCommands();
    }

    public static CommandService Instance => Singleton<CommandService>.Instance;

    public void Register(IEnumerable<Type> types)
    {
        CommandRepository.Register(types);
    }

    public CommandResult Execute(ISession session, string input)
    {
        using (var executionTimer = new ExecutionTimer())
        {
            var result = new CommandResult();
            var context = new CommandContext
            {
                Session = session,
                MessageSink = session
            };
            try
            {
                CommandTokenizer tokenizer;
                if (!GetTokenizer(input, result, context, out tokenizer))
                    return result;
                var mnemonic = tokenizer.Tokens.GetMnemonic();
                var command = CommandRepository.GetCommand(mnemonic.Value);
                if (command == null)
                {
                    result.Errors.Add(Error.NewError("S514",
                        string.Format("Command {0} not recognized.", mnemonic.Value),
                        "Use HELP command to learn more about available commands."));
                    result.ExtendedErrorMessage =
                        string.Format("Command '{0}' not recognized. Use HELP to learn more about available commands.",
                            mnemonic.Value);
                    return result;
                }

                if (command.FormMethodCache.Count == 0)
                {
                    result.Errors.Add(Error.NewError("S322",
                        string.Format("Command {0} does not have any valid executable forms.", mnemonic.Value),
                        "Please contact the support personnel for this command service and notify them of this error."));
                }
                else
                {
                    var symbols = tokenizer.Tokens.GetSymbols();
                    if ((symbols.Count == 0 && command.HasDefault()) || command.HasOnlyDefault())
                    {
                        command.InvokeDefault(result, context, tokenizer);
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

                    var method = command.GetMethod(symbols[0].Value);
                    if (method == null)
                    {
                        result.Errors.Add(Error.NewError("S001",
                            string.Format("An unknown form was used for the {0} comand.", mnemonic.Value.ToUpper()),
                            string.Format(
                                "Use the HELP command to learn how to properly invoke the {0} command and its forms.",
                                mnemonic.Value.ToUpper())));
                        return result;
                    }

                    if (!method.ValidateParameters(result, tokenizer))
                        return result;
                    method.Invoke(result, context, tokenizer, command.GetInstance());
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(Error.NewError("S999", "An unhandled exception was raised in CommandService.Execute.",
                    ex));
            }
            finally
            {
                result.ExecutionTime = executionTimer.Elapsed;
                result.Success = !context.Errors.ContainsError() && !result.Errors.ContainsError();
                result.Messages.AddRange(context.Messages);
                result.Errors.AddRange(context.Errors);
                Statistics.Instance.TrackCommandStatistics(result.ExecutionTime);
                context.Messages.Clear();
                context.Errors.Clear();
            }

            return result;
        }
    }

    private static bool GetTokenizer(
        string input,
        CommandResult result,
        CommandContext context,
        out CommandTokenizer tokenizer)
    {
        input = ServiceLocator.Instance.GetService<IRuntimeService>().ExpandConstantMacros(input);
        tokenizer = new CommandTokenizer(0, input);
        tokenizer.Tokenize();
        if (tokenizer.Tokens.HasOnlyComments())
            return false;
        if (tokenizer.Errors.ContainsError() || tokenizer.Tokens.GetMnemonic() == null)
        {
            result.Errors.AddRange(tokenizer.Errors);
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