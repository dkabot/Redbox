using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Redbox.Core;

namespace Redbox.IPC.Framework
{
    public class BatchCommandRunner : ISession, IMessageSink
    {
        private readonly StreamReader m_reader;
        private Action<ISession> _beforeCommandAction;
        private Action<string, string> _paramAction;
        private ParameterDictionary m_context;
        private IDictionary<string, object> m_properties;

        public BatchCommandRunner(StreamReader reader)
        {
            m_reader = reader;
        }

        public List<string> Filters { get; } = new List<string>();

        public bool Send(string message)
        {
            return true;
        }

        public void SetParamAction(Action<string, string> paramAction)
        {
            _paramAction = paramAction;
        }

        public void SetBeforeCommandAction(Action<ISession> beforeCommandAction)
        {
            _beforeCommandAction = beforeCommandAction;
        }

        public void SetFilters(List<string> filters)
        {
            Filters.Clear();
            filters.ForEach(Filters.Add);
        }

        public bool EnableFilters { get; set; }

        public bool IsConnected()
        {
            return true;
        }

        public void Start()
        {
            var builder = new StringBuilder();
            while (true)
            {
                CommandResult commandResult;
                do
                {
                    string str;
                    do
                    {
                        builder.Length = 0;
                        if (Read(builder))
                            str = builder.ToString();
                        else
                            goto label_8;
                    } while (str.Length <= 0);

                    LogHelper.Instance.Log(str, LogEntryType.Info);
                    if (str.IndexOf("quit", 0, StringComparison.CurrentCultureIgnoreCase) == -1)
                    {
                        if (_beforeCommandAction != null)
                            _beforeCommandAction(this);
                        commandResult = CommandService.Instance.Execute(this, string.Empty, str, Filters, EnableFilters,
                            _paramAction);
                    }
                    else
                    {
                        goto label_9;
                    }
                } while (commandResult == null);

                ProtocolHelper.FormatErrors(commandResult.Errors, commandResult.Messages);
                LogHelper.Instance.Log(commandResult.ToString(), LogEntryType.Info);
            }

            label_8:
            return;
            label_9: ;
        }

        public ParameterDictionary Context
        {
            get
            {
                if (m_context == null)
                    m_context = new ParameterDictionary();
                return m_context;
            }
        }

        public IDictionary<string, object> Properties
        {
            get
            {
                if (m_properties == null)
                    m_properties = new Dictionary<string, object>();
                return m_properties;
            }
        }

        public event EventHandler Disconnect;

        public static void ExecuteStartupFiles()
        {
            foreach (var file in Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                         "*.txt"))
                ExecuteFile(file);
        }

        public static void ExecuteFile(string fileName)
        {
            using (var reader = new StreamReader(fileName))
            {
                new BatchCommandRunner(reader).Start();
            }
        }

        private bool Read(StringBuilder builder)
        {
            var str = m_reader.ReadLine();
            if (str == null)
                return false;
            builder.Append(str);
            return true;
        }
    }
}