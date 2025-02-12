using Redbox.HAL.Client;
using Redbox.HAL.Client.Services;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace HALUtilities
{
  internal class ScriptMode : IDisposable
  {
    private readonly IConsole m_console;
    private readonly HardwareService Service;
    private readonly ClientHelper Helper;

    public void Dispose()
    {
      this.m_console.Dispose();
      this.Helper.Dispose();
    }

    internal ScriptMode(HardwareService service)
    {
      this.m_console = (IConsole) new DefaultConsole();
      this.Service = service;
      this.Helper = new ClientHelper(service);
    }

    internal int Run(string[] programArgs)
    {
      ImmediateCommand.Service = this.Service;
      for (int index = 0; index < programArgs.Length; ++index)
      {
        if (programArgs[index].StartsWith("--script"))
        {
          string optionVal = CommandLineOption.GetOptionVal<string>(programArgs[index], "=", string.Empty);
          if (optionVal == string.Empty)
          {
            this.m_console.WriteLine("An invalid script option was passed; please correct. Thank you.");
            return 1;
          }
          if (!File.Exists(optionVal))
          {
            this.m_console.WriteLine("The specified file '{0}' doesn't exist. Try again.", (object) optionVal);
            return 1;
          }
          this.ExecuteScriptCommands(optionVal);
          return 0;
        }
      }
      this.m_console.WriteLine("HAL Interactive Console, use ? for help.");
      bool flag = true;
      while (flag)
      {
        this.m_console.Write("> ");
        flag = this.ExecuteStatement(Console.ReadLine());
        HardwareJob job;
        Stack<string> stack;
        if (this.Service.GetJob(Constants.ExecutionContexts.ImmediateModeContext, out job).Success && job.GetStack(out stack).Success)
          this.FormatStack(stack, this.m_console);
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
        result.Errors.ForEach((Action<Error>) (each =>
        {
          console.WriteLine((object) each);
          console.WriteLine("Details: {0}\n", (object) each.Details);
        }));
      }
      if (result.CommandMessages.Count > 0)
      {
        console.WriteLine("Command Messages:");
        for (int index = 0; index < result.CommandMessages.Count; ++index)
          console.WriteLine(result.CommandMessages[index]);
      }
      console.Write("Command ");
      console.Write(result.Success ? "successfully executed" : "failed during execution");
      console.WriteLine(", execution time = {0}", (object) result.ExecutionTime);
    }

    private void FormatStack(Stack<string> stack, IConsole console)
    {
      int count = stack.Count;
      foreach (string str in stack)
        console.WriteLine("{0}: {1}", (object) count--, (object) str);
    }

    private bool ProcessJob(HardwareJob job, IConsole console, out ProgramResult[] programResults)
    {
      programResults = new ProgramResult[0];
      console.WriteLine("Job '{0}' successfully scheduled, resuming...", (object) job.ID);
      job.Connect();
      HardwareCommandResult result = job.Resume();
      ScriptMode.ShowResults(result, console);
      if (!result.Success)
        return false;
      console.WriteLine("Waiting for job '{0}' to complete...", (object) job.ID);
      this.Helper.WaitForJob(job, out HardwareJobStatus _);
      job.Disconnect();
      ScriptMode.ShowResults(result, console);
      if (!result.Success)
        return false;
      console.WriteLine("-----------------------------------------------------");
      console.WriteLine("Job results:");
      ErrorList errors1;
      HardwareCommandResult errors2 = job.GetErrors(out errors1);
      ScriptMode.ShowResults(errors2, console);
      if (!errors2.Success)
        return false;
      errors1.ForEach((Action<Error>) (each =>
      {
        console.WriteLine((object) each);
        console.WriteLine("Details: {0}\n", (object) each.Details);
      }));
      if (errors1.ContainsError())
        return false;
      Stack<string> stack1;
      HardwareCommandResult stack2 = job.GetStack(out stack1);
      ScriptMode.ShowResults(stack2, console);
      if (!stack2.Success)
        return false;
      HardwareCommandResult results = job.GetResults(out programResults);
      ScriptMode.ShowResults(results, console);
      if (!results.Success)
        return false;
      Array.ForEach<ProgramResult>(programResults, (Action<ProgramResult>) (each => console.WriteLine((object) each)));
      this.FormatStack(stack1, console);
      return true;
    }

    private void ExecuteScriptCommands(string file)
    {
      using (TextReader textReader = (TextReader) new StreamReader(file))
      {
        while (true)
        {
          string statement = textReader.ReadLine();
          if (statement != null)
            this.ExecuteStatement(statement);
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
      ImmediateCommand command = ImmediateCommand.GetCommand(statement);
      if (command == null)
        return true;
      ImmediateCommandResult immediateCommandResult = command.Execute();
      if (immediateCommandResult == null)
        return true;
      ScriptMode.ShowResults(immediateCommandResult.CommandResult, this.m_console);
      if (immediateCommandResult.Message != null)
        this.m_console.Write(immediateCommandResult.Message);
      return true;
    }

    private static class ImmediateCommands
    {
      public const string ExitCommand = "EXIT";
      public const string ImmediateCommandPrompt = "> ";
    }
  }
}
