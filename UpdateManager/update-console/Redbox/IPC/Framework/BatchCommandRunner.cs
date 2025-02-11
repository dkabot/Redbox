using Redbox.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Redbox.IPC.Framework
{
    internal class BatchCommandRunner : ISession, IMessageSink
    {
        private Action<string, string> _paramAction;
        private Action<ISession> _beforeCommandAction;
        private List<string> _filters = new List<string>();
        private bool _enableFilters;
        private ParameterDictionary m_context;
        private readonly StreamReader m_reader;
        private IDictionary<string, object> m_properties;

        public BatchCommandRunner(StreamReader reader) => this.m_reader = reader;

        public bool Send(string message) => true;

        public void SetParamAction(Action<string, string> paramAction)
        {
            this._paramAction = paramAction;
        }

        public void SetBeforeCommandAction(Action<ISession> beforeCommandAction)
        {
            this._beforeCommandAction = beforeCommandAction;
        }

        public void SetFilters(List<string> filters)
        {
            this._filters.Clear();
            filters.ForEach(new Action<string>(this._filters.Add));
        }

        public List<string> Filters => this._filters;

        public bool EnableFilters
        {
            get => this._enableFilters;
            set => this._enableFilters = value;
        }

        public bool IsConnected() => true;

        public static void ExecuteStartupFiles()
        {
            foreach (string file in Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.txt"))
                BatchCommandRunner.ExecuteFile(file);
        }

        public static void ExecuteFile(string fileName)
        {
            using (StreamReader reader = new StreamReader(fileName))
                new BatchCommandRunner(reader).Start();
        }

        public void Start()
        {
            StringBuilder builder = new StringBuilder();
            while (true)
            {
                CommandResult commandResult;
                do
                {
                    string str;
                    do
                    {
                        builder.Length = 0;
                        if (this.Read(builder))
                            str = builder.ToString();
                        else
                            goto label_8;
                    }
                    while (str.Length <= 0);
                    LogHelper.Instance.Log(str, LogEntryType.Info);
                    if (str.IndexOf("quit", 0, StringComparison.CurrentCultureIgnoreCase) == -1)
                    {
                        if (this._beforeCommandAction != null)
                            this._beforeCommandAction((ISession)this);
                        commandResult = CommandService.Instance.Execute((ISession)this, string.Empty, str, this.Filters, this.EnableFilters, this._paramAction);
                    }
                    else
                        goto label_9;
                }
                while (commandResult == null);
                ProtocolHelper.FormatErrors(commandResult.Errors, (IList<string>)commandResult.Messages);
                LogHelper.Instance.Log(commandResult.ToString(), LogEntryType.Info);
            }
        label_8:
            return;
        label_9:;
        }

        public ParameterDictionary Context
        {
            get
            {
                if (this.m_context == null)
                    this.m_context = new ParameterDictionary();
                return this.m_context;
            }
        }

        public IDictionary<string, object> Properties
        {
            get
            {
                if (this.m_properties == null)
                    this.m_properties = (IDictionary<string, object>)new Dictionary<string, object>();
                return this.m_properties;
            }
        }

        public event EventHandler Disconnect;

        private bool Read(StringBuilder builder)
        {
            string str = this.m_reader.ReadLine();
            if (str == null)
                return false;
            builder.Append(str);
            return true;
        }
    }
}
