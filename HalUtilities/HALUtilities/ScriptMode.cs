using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Services;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace HALUtilities
{
    internal class ScriptMode : IDisposable
    {
        private readonly ClientHelper Helper;
        private readonly IConsole m_console;
        private readonly HardwareService Service;

        internal ScriptMode(HardwareService service)
        {
            m_console = new DefaultConsole();
            Service = service;
            Helper = new ClientHelper(service);
        }

        public void Dispose()
        {
            m_console.Dispose();
            Helper.Dispose();
        }

        internal int Run(string[] programArgs)
        {
            ImmediateCommand.Service = Service;
            for (var index = 0; index < programArgs.Length; ++index)
                if (programArgs[index].StartsWith("--script"))
                {
                    var optionVal = CommandLineOption.GetOptionVal(programArgs[index], "=", string.Empty);
                    if (optionVal == string.Empty)
                    {
                        m_console.WriteLine("An invalid script option was passed; please correct. Thank you.");
                        return 1;
                    }

                    if (!File.Exists(optionVal))
                    {
                        m_console.WriteLine("The specified file '{0}' doesn't exist. Try again.", optionVal);
                        return 1;
                    }

                    ExecuteScriptCommands(optionVal);
                    return 0;
                }

            m_console.WriteLine("HAL Interactive Console, use ? for help.");
            var flag = true;
            while (flag)
            {
                m_console.Write("> ");
                flag = ExecuteStatement(Console.ReadLine());
                HardwareJob job;
                Stack<string> stack;
                if (Service.GetJob(Constants.ExecutionContexts.ImmediateModeContext, out job).Success &&
                    job.GetStack(out stack).Success)
                    FormatStack(stack, m_console);
            }

            return 0;
        }

        private static void ShowResults(HardwareCommandResult result, IConsole console)
        {
            if (result == null)
                return;
            if (!result.Success && result.Errors.Count > 0)
            {
                console.WriteLine("\nClient Errors:");
                result.Errors.ForEach(each =>
                {
                    console.WriteLine(each);
                    console.WriteLine("Details: {0}\n", each.Details);
                });
            }

            if (result.CommandMessages.Count > 0)
            {
                console.WriteLine("Command Messages:");
                for (var index = 0; index < result.CommandMessages.Count; ++index)
                    console.WriteLine(result.CommandMessages[index]);
            }

            console.Write("Command ");
            console.Write(result.Success ? "successfully executed" : "failed during execution");
            console.WriteLine(", execution time = {0}", result.ExecutionTime);
        }

        private void FormatStack(Stack<string> stack, IConsole console)
        {
            var count = stack.Count;
            foreach (var str in stack)
                console.WriteLine("{0}: {1}", count--, str);
        }

        private bool ProcessJob(HardwareJob job, IConsole console, out ProgramResult[] programResults)
        {
            programResults = new ProgramResult[0];
            console.WriteLine("Job '{0}' successfully scheduled, resuming...", job.ID);
            job.Connect();
            var result = job.Resume();
            ShowResults(result, console);
            if (!result.Success)
                return false;
            console.WriteLine("Waiting for job '{0}' to complete...", job.ID);
            Helper.WaitForJob(job, out var _);
            job.Disconnect();
            ShowResults(result, console);
            if (!result.Success)
                return false;
            console.WriteLine("-----------------------------------------------------");
            console.WriteLine("Job results:");
            ErrorList errors1;
            var errors2 = job.GetErrors(out errors1);
            ShowResults(errors2, console);
            if (!errors2.Success)
                return false;
            errors1.ForEach(each =>
            {
                console.WriteLine(each);
                console.WriteLine("Details: {0}\n", each.Details);
            });
            if (errors1.ContainsError())
                return false;
            Stack<string> stack1;
            var stack2 = job.GetStack(out stack1);
            ShowResults(stack2, console);
            if (!stack2.Success)
                return false;
            var results = job.GetResults(out programResults);
            ShowResults(results, console);
            if (!results.Success)
                return false;
            Array.ForEach(programResults, each => console.WriteLine(each));
            FormatStack(stack1, console);
            return true;
        }

        private void ExecuteScriptCommands(string file)
        {
            using (var textReader = (TextReader)new StreamReader(file))
            {
                while (true)
                {
                    var statement = textReader.ReadLine();
                    if (statement != null)
                        ExecuteStatement(statement);
                    else
                        break;
                }
            }
        }

        private bool ExecuteStatement(string statement)
        {
            if (string.IsNullOrEmpty(statement))
                return true;
            if (statement.StartsWith("EXIT", StringComparison.InvariantCultureIgnoreCase))
                return false;
            var command = ImmediateCommand.GetCommand(statement);
            if (command == null)
                return true;
            var immediateCommandResult = command.Execute();
            if (immediateCommandResult == null)
                return true;
            ShowResults(immediateCommandResult.CommandResult, m_console);
            if (immediateCommandResult.Message != null)
                m_console.Write(immediateCommandResult.Message);
            return true;
        }

        private static class ImmediateCommands
        {
            public const string ExitCommand = "EXIT";
            public const string ImmediateCommandPrompt = "> ";
        }
    }
}