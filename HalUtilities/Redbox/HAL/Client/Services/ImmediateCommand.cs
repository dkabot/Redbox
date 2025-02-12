using System;
using System.Collections.Generic;
using System.Text;

namespace Redbox.HAL.Client.Services
{
  public class ImmediateCommand
  {
    private List<string> m_arguments;
    private static IDictionary<string, ImmediateCommand> m_commands;

    public static ImmediateCommand GetCommand(string statement)
    {
      foreach (ImmediateCommand command in (IEnumerable<ImmediateCommand>) ImmediateCommand.Commands.Values)
      {
        if (!string.IsNullOrEmpty(command.Token) && statement.StartsWith(command.Token, StringComparison.InvariantCultureIgnoreCase))
        {
          command.Arguments.Clear();
          if (!string.IsNullOrEmpty(command.Token))
          {
            int startIndex = statement.IndexOf(command.Token);
            string str = statement.Remove(startIndex, 1);
            if (!string.IsNullOrEmpty(str))
              command.Arguments.Add(str);
          }
          return command;
        }
      }
      ExecuteStatementImmediateCommand command1 = new ExecuteStatementImmediateCommand();
      command1.Arguments.Add(statement);
      return (ImmediateCommand) command1;
    }

    public ImmediateCommandResult Execute()
    {
      ImmediateCommandResult result = new ImmediateCommandResult();
      this.OnExecute(result);
      return result;
    }

    public override string ToString()
    {
      return string.Format("{0,-4} {1,-16} {2}", (object) this.Token, (object) this.ExpectedArguments, (object) this.Help);
    }

    public static HardwareService Service { get; set; }

    protected ImmediateCommand()
    {
    }

    protected virtual void OnExecute(ImmediateCommandResult result)
    {
    }

    protected internal virtual string Token => string.Empty;

    protected virtual string Help => string.Empty;

    protected virtual string ExpectedArguments => string.Empty;

    internal static string FormatMessageFromJobResult(HardwareCommandResult result)
    {
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < result.CommandMessages.Count - 1; ++index)
        stringBuilder.AppendLine(result.CommandMessages[index]);
      return stringBuilder.ToString();
    }

    internal static IDictionary<string, ImmediateCommand> Commands
    {
      get
      {
        if (ImmediateCommand.m_commands == null)
          ImmediateCommand.m_commands = (IDictionary<string, ImmediateCommand>) new Dictionary<string, ImmediateCommand>();
        return ImmediateCommand.m_commands;
      }
    }

    internal static bool InImmediateProgramMode { get; set; }

    internal List<string> Arguments
    {
      get
      {
        if (this.m_arguments == null)
          this.m_arguments = new List<string>();
        return this.m_arguments;
      }
    }

    static ImmediateCommand()
    {
      foreach (Type type in typeof (ImmediateCommand).Assembly.GetTypes())
      {
        if (type.IsSubclassOf(typeof (ImmediateCommand)) && Activator.CreateInstance(type) is ImmediateCommand instance)
          ImmediateCommand.Commands[instance.Token] = instance;
      }
    }
  }
}
