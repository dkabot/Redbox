using System;
using System.IO;
using System.Reflection;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework;

public sealed class BatchCommandRunner : ISession, IMessageSink
{
    private readonly StreamReader m_reader;

    public BatchCommandRunner(StreamReader reader)
    {
        m_reader = reader;
        LogDetailedMessages = LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug);
    }

    internal ErrorList Errors { get; } = new();

    public bool Send(string message)
    {
        return true;
    }

    public void Start()
    {
        var builder = new StringBuilder();
        CommandResult commandResult;
        do
        {
            do
            {
                string str;
                do
                {
                    builder.Length = 0;
                    if (Read(builder))
                        str = builder.ToString();
                    else
                        goto label_11;
                } while (str.Length <= 0);

                if (LogDetailedMessages)
                    LogHelper.Instance.Log(str);
                if (str.IndexOf("quit", 0, StringComparison.CurrentCultureIgnoreCase) == -1)
                    commandResult = CommandService.Instance.Execute(this, str);
                else
                    goto label_12;
            } while (commandResult == null);

            ProtocolHelper.FormatErrors(commandResult.Errors, commandResult.Messages);
            if (LogDetailedMessages)
            {
                var type = commandResult.Errors.Count > 0 ? LogEntryType.Error : LogEntryType.Info;
                LogHelper.Instance.Log(commandResult.ToString(), type);
            }
        } while (!commandResult.Errors.ContainsError());

        goto label_10;
        label_11:
        return;
        label_12:
        return;
        label_10:
        Errors.AddRange(commandResult.Errors);
    }

    public event EventHandler Disconnect
    {
        add { }
        remove { }
    }

    public bool LogDetailedMessages { get; set; }

    public static ErrorList ExecuteStartupFiles()
    {
        var files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.txt");
        var errorList = new ErrorList();
        foreach (var fileName in files)
            ExecuteFile(fileName, errorList);
        return errorList;
    }

    public static void ExecuteFile(string fileName, ErrorList errorList)
    {
        using (var reader = new StreamReader(fileName))
        {
            var batchCommandRunner = new BatchCommandRunner(reader);
            batchCommandRunner.Start();
            if (batchCommandRunner.Errors.Count <= 0)
                return;
            errorList.AddRange(batchCommandRunner.Errors);
            batchCommandRunner.Errors.Clear();
        }
    }

    public ErrorList ExecuteFile()
    {
        Start();
        return Errors;
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