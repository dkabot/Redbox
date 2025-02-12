using System;
using System.Collections.Generic;
using System.Text;

namespace Redbox.HAL.Client.Services
{
    public class ImmediateCommand
    {
        private static IDictionary<string, ImmediateCommand> m_commands;
        private List<string> m_arguments;

        static ImmediateCommand()
        {
            foreach (var type in typeof(ImmediateCommand).Assembly.GetTypes())
                if (type.IsSubclassOf(typeof(ImmediateCommand)) &&
                    Activator.CreateInstance(type) is ImmediateCommand instance)
                    Commands[instance.Token] = instance;
        }

        protected ImmediateCommand()
        {
        }

        public static HardwareService Service { get; set; }

        protected internal virtual string Token => string.Empty;

        protected virtual string Help => string.Empty;

        protected virtual string ExpectedArguments => string.Empty;

        internal static IDictionary<string, ImmediateCommand> Commands
        {
            get
            {
                if (m_commands == null)
                    m_commands = new Dictionary<string, ImmediateCommand>();
                return m_commands;
            }
        }

        internal static bool InImmediateProgramMode { get; set; }

        internal List<string> Arguments
        {
            get
            {
                if (m_arguments == null)
                    m_arguments = new List<string>();
                return m_arguments;
            }
        }

        public static ImmediateCommand GetCommand(string statement)
        {
            foreach (var command in Commands.Values)
                if (!string.IsNullOrEmpty(command.Token) &&
                    statement.StartsWith(command.Token, StringComparison.InvariantCultureIgnoreCase))
                {
                    command.Arguments.Clear();
                    if (!string.IsNullOrEmpty(command.Token))
                    {
                        var startIndex = statement.IndexOf(command.Token);
                        var str = statement.Remove(startIndex, 1);
                        if (!string.IsNullOrEmpty(str))
                            command.Arguments.Add(str);
                    }

                    return command;
                }

            var command1 = new ExecuteStatementImmediateCommand();
            command1.Arguments.Add(statement);
            return command1;
        }

        public ImmediateCommandResult Execute()
        {
            var result = new ImmediateCommandResult();
            OnExecute(result);
            return result;
        }

        public override string ToString()
        {
            return string.Format("{0,-4} {1,-16} {2}", Token, ExpectedArguments, Help);
        }

        protected virtual void OnExecute(ImmediateCommandResult result)
        {
        }

        internal static string FormatMessageFromJobResult(HardwareCommandResult result)
        {
            var stringBuilder = new StringBuilder();
            for (var index = 0; index < result.CommandMessages.Count - 1; ++index)
                stringBuilder.AppendLine(result.CommandMessages[index]);
            return stringBuilder.ToString();
        }
    }
}