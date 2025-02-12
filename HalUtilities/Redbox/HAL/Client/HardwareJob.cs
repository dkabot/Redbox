using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.IPC.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Redbox.HAL.Client
{
  public sealed class HardwareJob
  {
    private HardwareJob.ConnectionInfo Connection;
    private readonly HardwareService Service;

    public HardwareCommandResult Signal(string msg)
    {
      return this.ExecuteCommand(string.Format("JOB signal job: '{0}' value: '{1}'", (object) this.ID, (object) msg));
    }

    public HardwareCommandResult Pend()
    {
      return this.ExecuteCommand(string.Format("JOB pend job: '{0}'", (object) this.ID));
    }

    public HardwareCommandResult Trash()
    {
      return this.ExecuteCommand(string.Format("JOB trash job: '{0}'", (object) this.ID));
    }

    public HardwareCommandResult SetPriority(HardwareJobPriority priority)
    {
      return this.ExecuteCommand(string.Format("JOB set-priority job: '{0}' value: '{1}'", (object) this.ID, (object) priority));
    }

    public HardwareCommandResult SetLabel(string label)
    {
      return this.ExecuteCommand(string.Format("JOB set-label job: '{0}' value: '{1}'", (object) this.ID, (object) label));
    }

    public HardwareCommandResult SetStartTime(DateTime startTime)
    {
      return this.ExecuteCommand(string.Format("JOB set-start-time job: '{0}' value: '{1}'", (object) this.ID, (object) startTime));
    }

    public HardwareCommandResult Resume()
    {
      return this.ExecuteCommand(string.Format("JOB resume job: '{0}'", (object) this.ID));
    }

    public HardwareCommandResult Suspend()
    {
      return this.ExecuteCommand(string.Format("JOB suspend job: '{0}'", (object) this.ID));
    }

    public HardwareCommandResult Connect()
    {
      IIpcClientSession session = this.Service.GetSession();
      session.ServerEvent += (Action<string>) (message =>
      {
        string[] strArray = message.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        DateTime result = DateTime.Now;
        if (strArray.Length > 1)
          DateTime.TryParse(strArray[0], out result);
        string eventMessage = strArray.Length > 1 ? strArray[1] : strArray[0];
        if (eventMessage.StartsWith(">STATUS CHANGE<"))
        {
          HardwareJobStatus ignoringCase = Enum<HardwareJobStatus>.ParseIgnoringCase(eventMessage.Substring(eventMessage.IndexOf("<") + 1).Trim(), HardwareJobStatus.Completed);
          if (this.StatusChanged != null)
            this.StatusChanged(this, ignoringCase);
        }
        if (this.EventRaised == null)
          return;
        this.EventRaised(this, result, eventMessage);
      });
      HardwareCommandResult hardwareCommandResult = ClientCommand<HardwareCommandResult>.ExecuteCommand(session, string.Format("JOB connect job: '{0}'", (object) this.ID));
      this.Connection = new HardwareJob.ConnectionInfo(session);
      this.Connection.EventThread = new Thread(new ParameterizedThreadStart(this.EventPoll))
      {
        Name = string.Format("Hardware Job {0} Event Thread", (object) this.ID),
        IsBackground = true
      };
      this.Connection.Start();
      return hardwareCommandResult;
    }

    public HardwareCommandResult Terminate()
    {
      return this.ExecuteCommand(string.Format("JOB terminate job: '{0}'", (object) this.ID));
    }

    public void Disconnect()
    {
      if (this.Connection == null)
        return;
      this.Connection.Disconnect();
      this.Connection = (HardwareJob.ConnectionInfo) null;
    }

    public HardwareCommandResult ClearStack()
    {
      return this.ExecuteCommand(string.Format("STACK clear job: '{0}'", (object) this.ID));
    }

    public HardwareCommandResult ClearSymbols()
    {
      return this.ExecuteCommand(string.Format("SYMBOL clear job: '{0}'", (object) this.ID));
    }

    public HardwareCommandResult GetErrors(out ErrorList errors)
    {
      HardwareCommandResult errors1 = this.ExecuteCommand(string.Format("JOB get-errors job: '{0}'", (object) this.ID));
      errors = errors1.Errors;
      return errors1;
    }

    public HardwareCommandResult GetMessages(out string[] messages)
    {
      List<string> stringList = new List<string>();
      HardwareCommandResult messages1 = this.ExecuteCommand(string.Format("JOB get-messages job: '{0}'", (object) this.ID));
      if (messages1.Success)
      {
        foreach (string commandMessage in messages1.CommandMessages)
          stringList.Add(commandMessage);
      }
      messages = stringList.ToArray();
      return messages1;
    }

    public HardwareCommandResult GetResults(out ProgramResult[] results)
    {
      HardwareCommandResult results1 = this.ExecuteCommand(string.Format("JOB get-results job: '{0}'", (object) this.ID));
      List<ProgramResult> list = new List<ProgramResult>();
      using (new DisposeableList<ProgramResult>((IList<ProgramResult>) list))
      {
        if (results1.Success)
          results1.CommandMessages.ForEach((Action<string>) (each =>
          {
            ProgramResult programResult = ProgramResult.FromString(each);
            if (programResult == null)
              return;
            list.Add(programResult);
          }));
        results = list.ToArray();
        return results1;
      }
    }

    public HardwareCommandResult RemoveResults()
    {
      return this.ExecuteCommand(string.Format("STACK remove-results job: '{0}'", (object) this.ID));
    }

    public HardwareCommandResult GetStack(out Stack<string> stack)
    {
      stack = new Stack<string>();
      HardwareCommandResult stack1 = this.ExecuteCommand(string.Format("STACK show job: '{0}'", (object) this.ID));
      if (stack1.Success)
      {
        for (int index = stack1.CommandMessages.Count - 1; index >= 0; --index)
          stack.Push(stack1.CommandMessages[index].Substring(stack1.CommandMessages[index].IndexOf(":") + 1).Trim());
      }
      return stack1;
    }

    public HardwareCommandResult GetSymbols(out IDictionary<string, string> symbols)
    {
      symbols = (IDictionary<string, string>) new Dictionary<string, string>();
      HardwareCommandResult symbols1 = this.ExecuteCommand(string.Format("SYMBOL show job: '{0}'", (object) this.ID));
      if (symbols1.Success)
      {
        foreach (string commandMessage in symbols1.CommandMessages)
        {
          string[] strArray = commandMessage.Split(":".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);
          symbols[strArray[0]] = strArray.Length == 2 ? strArray[1].Trim() : string.Empty;
        }
      }
      return symbols1;
    }

    public HardwareCommandResult Pop<T>(out T value)
    {
      value = default (T);
      HardwareCommandResult hardwareCommandResult = this.ExecuteCommand(string.Format("STACK pop job: '{0}'", (object) this.ID));
      if (hardwareCommandResult.Success)
        value = ConversionHelper.ChangeType<T>((object) hardwareCommandResult.CommandMessages[0]);
      return hardwareCommandResult;
    }

    public HardwareCommandResult Push(params object[] values)
    {
      if (values == null)
      {
        HardwareCommandResult hardwareCommandResult = new HardwareCommandResult();
        hardwareCommandResult.Success = false;
        return hardwareCommandResult;
      }
      HardwareCommandResult hardwareCommandResult1 = (HardwareCommandResult) null;
      foreach (object obj1 in values)
      {
        object obj2 = obj1;
        if (obj2 is string)
          obj2 = (object) string.Format("\"{0}\"", obj1);
        hardwareCommandResult1 = this.ExecuteCommand(string.Format("STACK push value: '{0}' job: '{1}'", obj2, (object) this.ID));
        if (!hardwareCommandResult1.Success)
          return hardwareCommandResult1;
      }
      return hardwareCommandResult1;
    }

    public HardwareCommandResult SetDebugState(bool enabled)
    {
      return this.ExecuteCommand(string.Format("DEBUG set job: '{0}' enabled: {1}", (object) this.ID, (object) enabled));
    }

    public HardwareCommandResult GetDebugState(out bool debuggerEnabled)
    {
      debuggerEnabled = false;
      HardwareCommandResult debugState = this.ExecuteCommand(string.Format("DEBUG set job: '{0}'", (object) this.ID));
      bool result;
      if (debugState.Success && bool.TryParse(debugState.CommandMessages[0], out result))
        debuggerEnabled = result;
      return debugState;
    }

    public HardwareCommandResult SetDebugTrace(bool enabled)
    {
      return this.ExecuteCommand(string.Format("DEBUG trace job: '{0}' enabled: {1}", (object) this.ID, (object) enabled));
    }

    public HardwareCommandResult GetDebugTrace(out bool traceEnabled)
    {
      traceEnabled = false;
      HardwareCommandResult debugTrace = this.ExecuteCommand(string.Format("DEBUG trace job: '{0}'", (object) this.ID));
      bool result;
      if (debugTrace.Success && bool.TryParse(debugTrace.CommandMessages[0], out result))
        traceEnabled = result;
      return debugTrace;
    }

    public HardwareCommandResult AddBreakPoint(string scriptName, int lineNumber)
    {
      return this.ExecuteCommand(string.Format("DEBUG add-breakpoint job: '{0}' script: '{1}' line: {2}", (object) this.ID, (object) scriptName, (object) lineNumber));
    }

    public HardwareCommandResult RemoveBreakPoint(string scriptName, int lineNumber)
    {
      return this.ExecuteCommand(string.Format("DEBUG remove-breakpoint job: '{0}' script: '{1}' line: {2}", (object) this.ID, (object) scriptName, (object) lineNumber));
    }

    public HardwareCommandResult ClearBreakPoints(string scriptName)
    {
      return this.ExecuteCommand(string.Format("DEBUG clear-breakpoints job: '{0}' script: '{1}'", (object) this.ID, (object) scriptName));
    }

    public HardwareCommandResult GetBreakPoints(string scriptName, out int[] lineNumbers)
    {
      lineNumbers = new int[0];
      HardwareCommandResult breakPoints = this.ExecuteCommand(string.Format("DEBUG get-breakpoints job: '{0}' script: '{1}'", (object) this.ID, (object) scriptName));
      if (breakPoints.Success)
        lineNumbers = breakPoints.CommandMessages.ConvertAll<int>((Converter<string, int>) (each => int.Parse(each))).ToArray();
      return breakPoints;
    }

    public HardwareCommandResult BreakAll()
    {
      return this.ExecuteCommand(string.Format("DEBUG break job: '{0}'", (object) this.ID));
    }

    public HardwareCommandResult Continue()
    {
      return this.ExecuteCommand(string.Format("DEBUG continue job: '{0}'", (object) this.ID));
    }

    public HardwareCommandResult StepInto()
    {
      return this.ExecuteCommand(string.Format("DEBUG step-into job: '{0}'", (object) this.ID));
    }

    public HardwareCommandResult StepOver()
    {
      return this.ExecuteCommand(string.Format("DEBUG step-over job: '{0}'", (object) this.ID));
    }

    public HardwareCommandResult WaitForCompletion()
    {
      return this.ExecuteCommand(string.Format("JOB wait-for-completion job: '{0}'", (object) this.ID));
    }

    public HardwareCommandResult WaitForCompletion(int timeout)
    {
      return this.Service.ExecuteCommand(string.Format("JOB wait-for-completion job: '{0}'", (object) this.ID), timeout);
    }

    public override string ToString() => string.Format("{0}", (object) this.ID);

    public bool Merge(HardwareJob job)
    {
      bool flag = false;
      if (job.Label != null && job.Label != this.Label)
      {
        this.Label = job.Label;
        flag = true;
      }
      if (job.ProgramName != null && job.ProgramName != this.ProgramName)
      {
        this.ProgramName = job.ProgramName;
        flag = true;
      }
      if (job.StartTime.HasValue)
      {
        DateTime? startTime1 = job.StartTime;
        DateTime? startTime2 = this.StartTime;
        if ((startTime1.HasValue == startTime2.HasValue ? (startTime1.HasValue ? (startTime1.GetValueOrDefault() != startTime2.GetValueOrDefault() ? 1 : 0) : 0) : 1) != 0)
        {
          this.StartTime = job.StartTime;
          flag = true;
        }
      }
      if (job.ExecutionTime.HasValue)
      {
        TimeSpan? executionTime1 = job.ExecutionTime;
        TimeSpan? executionTime2 = this.ExecutionTime;
        if ((executionTime1.HasValue == executionTime2.HasValue ? (executionTime1.HasValue ? (executionTime1.GetValueOrDefault() != executionTime2.GetValueOrDefault() ? 1 : 0) : 0) : 1) != 0)
        {
          this.ExecutionTime = job.ExecutionTime;
          flag = true;
        }
      }
      if (job.Status != this.Status)
      {
        this.Status = job.Status;
        flag = true;
      }
      if (job.Priority != this.Priority)
      {
        this.Priority = job.Priority;
        flag = true;
      }
      if (job.ConnectionState != this.ConnectionState)
      {
        this.ConnectionState = job.ConnectionState;
        flag = true;
      }
      if (job.EnableDebugging != this.EnableDebugging)
      {
        this.EnableDebugging = job.EnableDebugging;
        flag = true;
      }
      if (job.TraceExecution != this.TraceExecution)
      {
        this.TraceExecution = job.TraceExecution;
        flag = true;
      }
      return flag;
    }

    public string ID { get; internal set; }

    public string Label { get; internal set; }

    public string ProgramName { get; internal set; }

    public DateTime? StartTime { get; internal set; }

    public bool TraceExecution { get; internal set; }

    public bool EnableDebugging { get; internal set; }

    public TimeSpan? ExecutionTime { get; internal set; }

    public bool IsLocallyConnected => this.Connection != null;

    public HardwareJobStatus Status { get; internal set; }

    public HardwareJobPriority Priority { get; internal set; }

    public event HardwareEvent EventRaised;

    public HardwareJobConnectionState ConnectionState { get; internal set; }

    public event HardwareStatusChangeEvent StatusChanged;

    internal static HardwareJob Parse(HardwareService service, string jobData)
    {
      ReadOnlyCollection<string> properties = ProtocolHelper.ParseProperties(jobData);
      if (properties.Count < 8)
        return (HardwareJob) null;
      HardwareJob hardwareJob = new HardwareJob(service)
      {
        ID = properties[0],
        Label = properties[1],
        ProgramName = properties[2],
        Priority = Enum<HardwareJobPriority>.ParseIgnoringCase(properties[3], HardwareJobPriority.Normal),
        Status = Enum<HardwareJobStatus>.ParseIgnoringCase(properties[5], HardwareJobStatus.Suspended),
        ConnectionState = Enum<HardwareJobConnectionState>.ParseIgnoringCase(properties[7], HardwareJobConnectionState.Disconnected)
      };
      if (properties.Count == 10)
      {
        hardwareJob.EnableDebugging = properties[8] == "1";
        hardwareJob.TraceExecution = properties[9] == "1";
      }
      DateTime result1;
      if (DateTime.TryParse(properties[4], out result1))
        hardwareJob.StartTime = new DateTime?(result1);
      TimeSpan result2;
      if (TimeSpan.TryParse(properties[6], out result2))
        hardwareJob.ExecutionTime = new TimeSpan?(result2);
      return hardwareJob;
    }

    internal void EventPoll(object o)
    {
      using (HardwareJob.ConnectionInfo connectionInfo = o as HardwareJob.ConnectionInfo)
      {
        do
        {
          if (!ClientCommand<HardwareCommandResult>.ExecuteCommand(connectionInfo.Session, "JOB scheduler-status").Success && this.EventRaised != null)
            this.EventRaised(this, DateTime.Now, ">IPC EXCEPTION<");
        }
        while (!connectionInfo.WaitDisconnect());
      }
      this.ExecuteCommand(string.Format("JOB disconnect job: '{0}'", (object) this.ID));
    }

    private HardwareCommandResult ExecuteCommand(string command)
    {
      return this.Service.ExecuteCommand(command);
    }

    private HardwareJob(HardwareService service) => this.Service = service;

    private class ConnectionInfo : IDisposable
    {
      private bool m_disposed;
      private readonly ManualResetEvent ConnectEvent = new ManualResetEvent(false);

      public void Dispose()
      {
        if (this.m_disposed)
          return;
        this.m_disposed = true;
        this.ConnectEvent.Close();
        this.Session.Dispose();
      }

      internal IIpcClientSession Session { get; private set; }

      internal Thread EventThread { get; set; }

      internal void Start() => this.EventThread.Start((object) this);

      internal void Disconnect() => this.ConnectEvent.Set();

      internal bool WaitDisconnect() => this.WaitDisconnect(250);

      internal bool WaitDisconnect(int ms) => this.ConnectEvent.WaitOne(ms);

      internal ConnectionInfo(IIpcClientSession session) => this.Session = session;
    }
  }
}
