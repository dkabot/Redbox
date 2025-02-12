using System;
using System.Collections.Generic;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;
using Redbox.HAL.IPC.Framework;

namespace Redbox.HAL.Client
{
    public sealed class HardwareJob
    {
        private readonly HardwareService Service;

        private HardwareJob(HardwareService service)
        {
            Service = service;
        }

        public string ID { get; internal set; }

        public string Label { get; internal set; }

        public string ProgramName { get; internal set; }

        public DateTime? StartTime { get; internal set; }

        public bool TraceExecution { get; internal set; }

        public bool EnableDebugging { get; internal set; }

        public TimeSpan? ExecutionTime { get; internal set; }

        public bool IsLocallyConnected { get; private set; }

        public HardwareJobStatus Status { get; internal set; }

        public HardwareJobPriority Priority { get; internal set; }

        public HardwareJobConnectionState ConnectionState { get; internal set; }

        public HardwareCommandResult Signal(string msg)
        {
            return ExecuteCommand(string.Format("JOB signal job: '{0}' value: '{1}'", ID, msg));
        }

        public HardwareCommandResult Pend()
        {
            return ExecuteCommand(string.Format("JOB pend job: '{0}'", ID));
        }

        public HardwareCommandResult Trash()
        {
            return ExecuteCommand(string.Format("JOB trash job: '{0}'", ID));
        }

        public HardwareCommandResult SetPriority(HardwareJobPriority priority)
        {
            return ExecuteCommand(string.Format("JOB set-priority job: '{0}' value: '{1}'", ID, priority));
        }

        public HardwareCommandResult SetLabel(string label)
        {
            return ExecuteCommand(string.Format("JOB set-label job: '{0}' value: '{1}'", ID, label));
        }

        public HardwareCommandResult SetStartTime(DateTime startTime)
        {
            return ExecuteCommand(string.Format("JOB set-start-time job: '{0}' value: '{1}'", ID, startTime));
        }

        public HardwareCommandResult Resume()
        {
            return ExecuteCommand(string.Format("JOB resume job: '{0}'", ID));
        }

        public HardwareCommandResult Suspend()
        {
            return ExecuteCommand(string.Format("JOB suspend job: '{0}'", ID));
        }

        public HardwareCommandResult Connect()
        {
            var session = Service.GetSession();
            session.ServerEvent += message =>
            {
                var strArray = message.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var result = DateTime.Now;
                if (strArray.Length > 1)
                    DateTime.TryParse(strArray[0], out result);
                var eventMessage = strArray.Length > 1 ? strArray[1] : strArray[0];
                if (eventMessage.StartsWith(">STATUS CHANGE<"))
                {
                    var str = eventMessage;
                    var ignoringCase =
                        Enum<HardwareJobStatus>.ParseIgnoringCase(str.Substring(str.IndexOf("<") + 1).Trim(),
                            HardwareJobStatus.Completed);
                    if (StatusChanged != null)
                        StatusChanged(this, ignoringCase);
                }

                if (EventRaised == null)
                    return;
                EventRaised(this, result, eventMessage);
            };
            var hardwareCommandResult =
                ClientCommand<HardwareCommandResult>.ExecuteCommand(session,
                    string.Format("JOB connect job: '{0}'", ID));
            IsLocallyConnected = true;
            new Thread(EventPoll)
            {
                Name = string.Format("Hardware Job {0} Event Thread", ID),
                IsBackground = true
            }.Start(session);
            return hardwareCommandResult;
        }

        public HardwareCommandResult Terminate()
        {
            return ExecuteCommand(string.Format("JOB terminate job: '{0}'", ID));
        }

        public void Disconnect()
        {
            IsLocallyConnected = false;
        }

        public HardwareCommandResult ClearStack()
        {
            return ExecuteCommand(string.Format("STACK clear job: '{0}'", ID));
        }

        public HardwareCommandResult ClearSymbols()
        {
            return ExecuteCommand(string.Format("SYMBOL clear job: '{0}'", ID));
        }

        public HardwareCommandResult GetErrors(out ErrorList errors)
        {
            var errors1 = ExecuteCommand(string.Format("JOB get-errors job: '{0}'", ID));
            errors = errors1.Errors;
            return errors1;
        }

        public HardwareCommandResult GetMessages(out string[] messages)
        {
            var stringList = new List<string>();
            var messages1 = ExecuteCommand(string.Format("JOB get-messages job: '{0}'", ID));
            if (messages1.Success)
                foreach (var commandMessage in messages1.CommandMessages)
                    stringList.Add(commandMessage);
            messages = stringList.ToArray();
            return messages1;
        }

        public HardwareCommandResult GetResults(out ProgramResult[] results)
        {
            var results1 = ExecuteCommand(string.Format("JOB get-results job: '{0}'", ID));
            var programResultList = new List<ProgramResult>();
            try
            {
                if (results1.Success)
                    foreach (var commandMessage in results1.CommandMessages)
                    {
                        var programResult = ProgramResult.FromString(commandMessage);
                        if (programResult != null)
                            programResultList.Add(programResult);
                    }

                results = programResultList.ToArray();
                return results1;
            }
            finally
            {
                programResultList.Clear();
            }
        }

        public HardwareCommandResult RemoveResults()
        {
            return ExecuteCommand(string.Format("STACK remove-results job: '{0}'", ID));
        }

        public HardwareCommandResult GetStack(out Stack<string> stack)
        {
            stack = new Stack<string>();
            var stack1 = ExecuteCommand(string.Format("STACK show job: '{0}'", ID));
            if (stack1.Success)
                for (var index = stack1.CommandMessages.Count - 1; index >= 0; --index)
                    stack.Push(stack1.CommandMessages[index].Substring(stack1.CommandMessages[index].IndexOf(":") + 1)
                        .Trim());
            return stack1;
        }

        public HardwareCommandResult GetSymbols(out IDictionary<string, string> symbols)
        {
            symbols = new Dictionary<string, string>();
            var symbols1 = ExecuteCommand(string.Format("SYMBOL show job: '{0}'", ID));
            if (symbols1.Success)
                foreach (var commandMessage in symbols1.CommandMessages)
                {
                    var strArray = commandMessage.Split(":".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);
                    symbols[strArray[0]] = strArray.Length == 2 ? strArray[1].Trim() : string.Empty;
                }

            return symbols1;
        }

        public HardwareCommandResult Pop<T>(out T value)
        {
            value = default;
            var hardwareCommandResult = ExecuteCommand(string.Format("STACK pop job: '{0}'", ID));
            if (hardwareCommandResult.Success)
                value = (T)ConversionHelper.ChangeType(hardwareCommandResult.CommandMessages[0], typeof(T));
            return hardwareCommandResult;
        }

        public HardwareCommandResult Push(params object[] values)
        {
            if (values == null)
            {
                var hardwareCommandResult = new HardwareCommandResult();
                hardwareCommandResult.Success = false;
                return hardwareCommandResult;
            }

            var hardwareCommandResult1 = (HardwareCommandResult)null;
            foreach (var obj1 in values)
            {
                var obj2 = obj1;
                if (obj2 is string)
                    obj2 = string.Format("\"{0}\"", obj1);
                hardwareCommandResult1 = ExecuteCommand(string.Format("STACK push value: '{0}' job: '{1}'", obj2, ID));
                if (!hardwareCommandResult1.Success)
                    return hardwareCommandResult1;
            }

            return hardwareCommandResult1;
        }

        public HardwareCommandResult SetDebugState(bool enabled)
        {
            return ExecuteCommand(string.Format("DEBUG set job: '{0}' enabled: {1}", ID, enabled));
        }

        public HardwareCommandResult GetDebugState(out bool debuggerEnabled)
        {
            debuggerEnabled = false;
            var debugState = ExecuteCommand(string.Format("DEBUG set job: '{0}'", ID));
            bool result;
            if (debugState.Success && bool.TryParse(debugState.CommandMessages[0], out result))
                debuggerEnabled = result;
            return debugState;
        }

        public HardwareCommandResult SetDebugTrace(bool enabled)
        {
            return ExecuteCommand(string.Format("DEBUG trace job: '{0}' enabled: {1}", ID, enabled));
        }

        public HardwareCommandResult GetDebugTrace(out bool traceEnabled)
        {
            traceEnabled = false;
            var debugTrace = ExecuteCommand(string.Format("DEBUG trace job: '{0}'", ID));
            bool result;
            if (debugTrace.Success && bool.TryParse(debugTrace.CommandMessages[0], out result))
                traceEnabled = result;
            return debugTrace;
        }

        public HardwareCommandResult AddBreakPoint(string scriptName, int lineNumber)
        {
            return ExecuteCommand(string.Format("DEBUG add-breakpoint job: '{0}' script: '{1}' line: {2}", ID,
                scriptName, lineNumber));
        }

        public HardwareCommandResult RemoveBreakPoint(string scriptName, int lineNumber)
        {
            return ExecuteCommand(string.Format("DEBUG remove-breakpoint job: '{0}' script: '{1}' line: {2}", ID,
                scriptName, lineNumber));
        }

        public HardwareCommandResult ClearBreakPoints(string scriptName)
        {
            return ExecuteCommand(string.Format("DEBUG clear-breakpoints job: '{0}' script: '{1}'", ID, scriptName));
        }

        public HardwareCommandResult GetBreakPoints(string scriptName, out int[] lineNumbers)
        {
            lineNumbers = new int[0];
            var breakPoints =
                ExecuteCommand(string.Format("DEBUG get-breakpoints job: '{0}' script: '{1}'", ID, scriptName));
            if (breakPoints.Success)
                lineNumbers = breakPoints.CommandMessages.ConvertAll(each => int.Parse(each)).ToArray();
            return breakPoints;
        }

        public HardwareCommandResult BreakAll()
        {
            return ExecuteCommand(string.Format("DEBUG break job: '{0}'", ID));
        }

        public HardwareCommandResult Continue()
        {
            return ExecuteCommand(string.Format("DEBUG continue job: '{0}'", ID));
        }

        public HardwareCommandResult StepInto()
        {
            return ExecuteCommand(string.Format("DEBUG step-into job: '{0}'", ID));
        }

        public HardwareCommandResult StepOver()
        {
            return ExecuteCommand(string.Format("DEBUG step-over job: '{0}'", ID));
        }

        public HardwareCommandResult WaitForCompletion()
        {
            return ExecuteCommand(string.Format("JOB wait-for-completion job: '{0}'", ID));
        }

        public HardwareCommandResult WaitForCompletion(int timeout)
        {
            return Service.ExecuteCommand(string.Format("JOB wait-for-completion job: '{0}'", ID), timeout);
        }

        public override string ToString()
        {
            return string.Format("{0}", ID);
        }

        public bool Merge(HardwareJob job)
        {
            var flag = false;
            if (job.Label != null && job.Label != Label)
            {
                Label = job.Label;
                flag = true;
            }

            if (job.ProgramName != null && job.ProgramName != ProgramName)
            {
                ProgramName = job.ProgramName;
                flag = true;
            }

            if (job.StartTime.HasValue)
            {
                var startTime1 = job.StartTime;
                var startTime2 = StartTime;
                if ((startTime1.HasValue == startTime2.HasValue
                        ? startTime1.HasValue
                            ? startTime1.GetValueOrDefault() != startTime2.GetValueOrDefault() ? 1 : 0
                            : 0
                        : 1) != 0)
                {
                    StartTime = job.StartTime;
                    flag = true;
                }
            }

            if (job.ExecutionTime.HasValue)
            {
                var executionTime1 = job.ExecutionTime;
                var executionTime2 = ExecutionTime;
                if ((executionTime1.HasValue == executionTime2.HasValue
                        ? executionTime1.HasValue
                            ? executionTime1.GetValueOrDefault() != executionTime2.GetValueOrDefault() ? 1 : 0
                            : 0
                        : 1) != 0)
                {
                    ExecutionTime = job.ExecutionTime;
                    flag = true;
                }
            }

            if (job.Status != Status)
            {
                Status = job.Status;
                flag = true;
            }

            if (job.Priority != Priority)
            {
                Priority = job.Priority;
                flag = true;
            }

            if (job.ConnectionState != ConnectionState)
            {
                ConnectionState = job.ConnectionState;
                flag = true;
            }

            if (job.EnableDebugging != EnableDebugging)
            {
                EnableDebugging = job.EnableDebugging;
                flag = true;
            }

            if (job.TraceExecution != TraceExecution)
            {
                TraceExecution = job.TraceExecution;
                flag = true;
            }

            return flag;
        }

        public event HardwareEvent EventRaised;

        public event HardwareStatusChangeEvent StatusChanged;

        internal static HardwareJob Parse(HardwareService service, string jobData)
        {
            var properties = ProtocolHelper.ParseProperties(jobData);
            if (properties.Count < 8)
                return null;
            var hardwareJob = new HardwareJob(service)
            {
                ID = properties[0],
                Label = properties[1],
                ProgramName = properties[2],
                Priority = Enum<HardwareJobPriority>.ParseIgnoringCase(properties[3], HardwareJobPriority.Normal),
                Status = Enum<HardwareJobStatus>.ParseIgnoringCase(properties[5], HardwareJobStatus.Suspended),
                ConnectionState =
                    Enum<HardwareJobConnectionState>.ParseIgnoringCase(properties[7],
                        HardwareJobConnectionState.Disconnected)
            };
            if (properties.Count == 10)
            {
                hardwareJob.EnableDebugging = properties[8] == "1";
                hardwareJob.TraceExecution = properties[9] == "1";
            }

            DateTime result1;
            if (DateTime.TryParse(properties[4], out result1))
                hardwareJob.StartTime = result1;
            TimeSpan result2;
            if (TimeSpan.TryParse(properties[6], out result2))
                hardwareJob.ExecutionTime = result2;
            return hardwareJob;
        }

        internal void EventPoll(object o)
        {
            var session = o as IIpcClientSession;
            while (IsLocallyConnected)
            {
                if (!ClientCommand<HardwareCommandResult>.ExecuteCommand(session, "JOB scheduler-status").Success &&
                    EventRaised != null)
                    EventRaised(this, DateTime.Now, ">IPC EXCEPTION<");
                Thread.Sleep(250);
            }

            var command = string.Format("JOB disconnect job: '{0}'", ID);
            session.Dispose();
            ExecuteCommand(command);
        }

        private HardwareCommandResult ExecuteCommand(string command)
        {
            return Service.ExecuteCommand(command);
        }
    }
}