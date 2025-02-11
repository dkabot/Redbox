using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Component.Model.Threading;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public class ExecutionContext :
        IMessageSink,
        ISerializable,
        IExecutionContext,
        IHardwareCorrectionObserver
    {
        private const string ResultPrefix = "RESULT";
        private const string EmptyItemToken = "~";
        internal readonly ApplicationLog AppLog;
        private readonly List<string> CachedResults = new List<string>();
        private readonly List<string> CachedSignals = new List<string>();
        private readonly EventWaitHandle CompletionWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        private readonly ReaderWriterLockSlim ContextLock = new ReaderWriterLockSlim();
        private readonly Deque<object> DataStack = new Deque<object>();
        internal readonly Deque<ScriptMessage> Messages = new Deque<ScriptMessage>();
        private readonly ReaderWriterLockSlim ObserverLock = new ReaderWriterLockSlim();

        internal readonly IDictionary<string, ProgramInstruction> ProgramCache =
            new Dictionary<string, ProgramInstruction>();

        internal readonly Registers Registers;
        private readonly ReaderWriterLockSlim SignalsLock = new ReaderWriterLockSlim();
        internal readonly ExecutionStatus Status = new ExecutionStatus();
        private readonly IDictionary<string, object> SymbolTable = new Dictionary<string, object>();
        private bool IsCooperative;
        private bool IsHydrating;
        private DateTime? LastEventTime;
        private ConnectionState m_connectionState = ConnectionState.Disconnected;
        private string m_contextDirectory;
        private bool m_disposed;
        private string m_label;
        private ProgramPriority m_priority;
        private ExecutionResult m_result;
        private DateTime? m_startTime;
        internal bool ShouldCleanup = true;
        private Action<string> SignalHandler;

        protected ExecutionContext(SerializationInfo info, StreamingContext context)
        {
            IsHydrating = true;
            var flag = true;
            try
            {
                flag = info.GetBoolean(nameof(IsCooperative));
                IsCooperative = flag;
            }
            catch (SerializationException ex)
            {
                LogHelper.Instance.Log("Unable to hydrate context due to serialization exception", ex);
                IsCooperative = true;
            }

            m_connectionState = ConnectionState.Disconnected;
            m_result = new ExecutionResult();
            Registers = new Registers();
            m_label = info.GetString(nameof(Label));
            ProgramName = info.GetString(nameof(ProgramName));
            ID = info.GetString(nameof(ID));
            m_priority =
                Enum<ProgramPriority>.ParseIgnoringCase(info.GetString(nameof(Priority)), ProgramPriority.Normal);
            Status.NextValue =
                Enum<ExecutionContextStatus>.ParseIgnoringCase(info.GetString(nameof(Status)),
                    ExecutionContextStatus.Stopped);
            var nextValue = Status.NextValue;
            var executionContextStatus = ExecutionContextStatus.Running;
            if ((nextValue.GetValueOrDefault() == executionContextStatus) & nextValue.HasValue)
                Status.NextValue = ExecutionContextStatus.Suspended;
            Status.Promote();
            CreatedOn = info.GetDateTime(nameof(CreatedOn));
            if (info.GetBoolean("HasStartTime"))
                m_startTime = info.GetDateTime(nameof(StartTime));
            if (info.GetBoolean("HasExecutionTime"))
            {
                var s = info.GetString("ExecutionTime");
                if (!string.IsNullOrEmpty(s))
                    m_result.ExecutionTime = TimeSpan.Parse(s);
            }

            if (info.GetBoolean("HasExecutionStartTime"))
                ExecutionStart = info.GetDateTime("ExecutionStartTime");
            var str1 = info.GetString("Errors");
            if (!string.IsNullOrEmpty(str1))
            {
                var errors = str1.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (errors != null && errors.Length != 0)
                    m_result.Errors.AddRange(ErrorList.NewFromStrings(errors));
            }

            var str2 = info.GetString(nameof(Messages));
            if (!string.IsNullOrEmpty(str2))
                Array.ForEach(str2.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries), item =>
                {
                    if (string.IsNullOrEmpty(item))
                        return;
                    Messages.PushBottom(ScriptMessage.Parse(item));
                });
            var str3 = info.GetString("Results");
            if (!string.IsNullOrEmpty(str3))
                Array.ForEach(str3.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries), item =>
                {
                    if (string.IsNullOrEmpty(item))
                        return;
                    CachedResults.Add(item);
                });
            ParentID = info.GetString(nameof(ParentID));
            if (flag)
            {
                var strArray = info.GetString("Programs")
                    .Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var binaryFormatter = new BinaryFormatter();
                foreach (var str4 in strArray)
                {
                    var path = Path.Combine(ContextDirectory, str4);
                    try
                    {
                        using (var serializationStream = File.OpenRead(path))
                        {
                            ProgramCache[str4] = (ProgramInstruction)binaryFormatter.Deserialize(serializationStream);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log("Unable to deserialize program.", ex);
                    }
                }
            }

            if (info.GetBoolean("HasSymbolTable"))
                SymbolTable =
                    (IDictionary<string, object>)info.GetValue(nameof(SymbolTable), typeof(Dictionary<string, object>));
            var strArray1 = info.GetString("Stack").Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (var index = strArray1.Length - 1; index >= 0; --index)
            {
                var strArray2 = strArray1[index].Split("=".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);
                if (strArray2.Length >= 2)
                    OnPush(ConversionHelper.ChangeType(strArray2[1], Type.GetType(strArray2[0])), StackEnd.Top);
            }

            if (flag)
            {
                var strArray3 = info.GetString("CallFrames")
                    .Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                for (var index = strArray3.Length - 1; index >= 0; --index)
                {
                    var strArray4 = strArray3[index].Split('|');
                    var key = strArray4[3];
                    var parentContextLineNumber = int.Parse(strArray4[4]);
                    var callFrame = new CallFrame
                    {
                        PC = int.Parse(strArray4[0]),
                        X = int.Parse(strArray4[1]),
                        Y = int.Parse(strArray4[2]),
                        ProgramName = key
                    };
                    var program = ProgramCache.ContainsKey(key) ? ProgramCache[key] : null;
                    callFrame.LoadInstructionPointer(parentContextLineNumber, program);
                    Registers.CallStack.Push(callFrame);
                }
            }

            AppLog = ApplicationLog.Deserialize(info, this);
            IsHydrating = false;
        }

        public ExecutionContext(
            string id,
            string programName,
            DateTime? startTime,
            ProgramPriority p,
            string label)
        {
            IsHydrating = true;
            ID = id;
            CreatedOn = DateTime.Now;
            ProgramName = programName;
            Registers = new Registers();
            m_result = new ExecutionResult();
            m_connectionState = ConnectionState.Disconnected;
            AppLog = new ApplicationLog(this);
            m_startTime = startTime;
            m_priority = p;
            m_label = label;
            IsHydrating = false;
        }

        internal string ParentID { get; set; }

        internal bool EligibleForScheduling => Status == ExecutionContextStatus.Pending;

        internal ExecutionContextStatus? DeferredStatusChangeRequested { get; set; }

        internal string ContextDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(m_contextDirectory))
                    m_contextDirectory =
                        Path.Combine(
                            ServiceLocator.Instance.GetService<IRuntimeService>().RuntimePath("data\\contexts"), ID);
                return m_contextDirectory;
            }
        }

        public void HardwareCorrectionStart(HardwareCorrectionEventArgs s)
        {
            var hardwareCorrectionEvent = StartHardwareCorrectionEvent;
            if (hardwareCorrectionEvent == null)
                return;
            hardwareCorrectionEvent(s);
        }

        public void HardwareCorrectionEnd(HardwareCorrectionEventArgs s)
        {
            var hardwareCorrectionEvent = EndHardwareCorrectionEvent;
            if (hardwareCorrectionEvent == null)
                return;
            hardwareCorrectionEvent(s);
        }

        public void CreateResult(string code, string msg, string id)
        {
            CreateResult(StackEnd.Bottom, code, msg, new int?(), new int?(), id, new DateTime?(), null);
        }

        public string CreateJSONResult(string code, string id, IDictionary<string, object> jsonMessage)
        {
            var message = jsonMessage == null || jsonMessage.Count <= 0
                ? string.Empty
                : JsonConvert.SerializeObject(jsonMessage);
            return CreateResult(StackEnd.Bottom, code, message, new int?(), new int?(), id, new DateTime?(), null);
        }

        public void ForeachResult(Action<string> action)
        {
            using (new WithReadLock(ContextLock))
            {
                CachedResults.ForEach(action);
            }
        }

        public void ClearResults()
        {
            using (new WithWriteLock(ContextLock))
            {
                CachedResults.Clear();
                SaveUnderLock();
            }
        }

        public void Trash()
        {
            LogHelper.Instance.Log("Trash context {0} ( {1} )", ID, ProgramName);
            using (new WithWriteLock(ContextLock))
            {
                ChangeStatusUnderLock(ExecutionContextStatus.Garbage);
                if (CachedResults.Count > 0)
                    CachedResults.Clear();
                SaveUnderLock();
            }
        }

        public void Terminate()
        {
            if (Status == ExecutionContextStatus.Completed || Status == ExecutionContextStatus.Garbage ||
                Status == ExecutionContextStatus.Errored || Status == ExecutionContextStatus.Stopped)
                return;
            ChangeStatus(ExecutionContextStatus.Stopped);
        }

        public void Resume()
        {
            m_result.Errors.RemoveCode("E201");
            if (Status != ExecutionContextStatus.Suspended)
                LogHelper.Instance.Log(string.Format("Cannot resume context with status {0}", Status),
                    LogEntryType.Error);
            else
                ChangeStatus(ExecutionContextStatus.Pending);
        }

        public void Resurrect()
        {
            m_result.Errors.RemoveCode("E201");
            ChangeStatus(ExecutionContextStatus.Pending);
        }

        public void Pend()
        {
            if (!IsImmediate && !(Status == ExecutionContextStatus.Suspended))
                return;
            ChangeStatus(ExecutionContextStatus.Pending);
        }

        public void Suspend()
        {
            if (IsImmediate || Status == ExecutionContextStatus.Pending || Status == ExecutionContextStatus.Running)
                ChangeStatus(ExecutionContextStatus.Suspended);
            else
                LogHelper.Instance.Log(LogEntryType.Debug,
                    "Cannot suspend job that isn't in Pending or Running status.", LogEntryType.Error);
        }

        public bool WaitForCompletion()
        {
            if (IsComplete)
                return true;
            while (true)
                try
                {
                    if (CompletionWaitHandle.WaitOne(50, false) || IsComplete)
                        return true;
                    Thread.Sleep(0);
                }
                catch (ObjectDisposedException ex)
                {
                    return true;
                }
        }

        public bool Connect(IMessageSink sink)
        {
            using (new WithWriteLock(ContextLock))
            {
                MessageSink = sink;
                if (MessageSink == null)
                {
                    m_connectionState = ConnectionState.Disconnected;
                }
                else
                {
                    m_connectionState = ConnectionState.Connected;
                    FlushUnderLock();
                    SaveUnderLock();
                }
            }

            return true;
        }

        public bool Disconnect()
        {
            return Connect(null);
        }

        public ExecutionContextStatus GetStatus()
        {
            return Status.Value;
        }

        public object Pop(StackEnd end)
        {
            var obj = (object)null;
            using (new WithWriteLock(ContextLock))
            {
                obj = OnPop(end);
                SaveUnderLock();
            }

            return obj;
        }

        public void Push(object instance, StackEnd end)
        {
            using (new WithWriteLock(ContextLock))
            {
                OnPush(instance, end);
                SaveUnderLock();
            }
        }

        public void Push(List<ILocation> locations)
        {
            using (new WithWriteLock(ContextLock))
            {
                locations.ForEach(location =>
                {
                    OnPush(location.Slot, StackEnd.Top);
                    OnPush(location.Deck, StackEnd.Top);
                });
                OnPush(locations.Count, StackEnd.Top);
                SaveUnderLock();
            }
        }

        public void ClearStack()
        {
            using (new WithWriteLock(ContextLock))
            {
                DataStack.Clear();
                SaveUnderLock();
            }
        }

        public void ForeachStackItem(Action<object> action)
        {
            using (new WithReadLock(ContextLock))
            {
                foreach (var data in DataStack)
                    action(data);
            }
        }

        public void Signal(string signal)
        {
            using (new WithUpgradeableReadLock(SignalsLock))
            {
                if (SignalHandler != null)
                    SignalHandler(signal);
                else
                    using (new WithWriteLock(SignalsLock))
                    {
                        CachedSignals.Add(signal);
                    }
            }
        }

        public void ForeachMessage(Action<string> action)
        {
            using (new WithReadLock(ContextLock))
            {
                foreach (var message in Messages)
                    action(message.ToString());
            }
        }

        public void ForeachSymbol(Action<IContextSymbol> action)
        {
            using (new WithReadLock(ContextLock))
            {
                foreach (var key in SymbolTable.Keys)
                {
                    var contextSymbol = new ContextSymbol(key)
                    {
                        Value = SymbolTable[key]
                    };
                    if (!contextSymbol.IsReserved)
                        action(contextSymbol);
                }
            }
        }

        public void ClearSymbolTable()
        {
            using (new WithWriteLock(ContextLock))
            {
                SymbolTable.Clear();
                SaveUnderLock();
            }
        }

        public IFormattedLog ContextLog => AppLog;

        public string ID { get; private set; }

        public string ProgramName { get; private set; }

        public bool IsComplete => Status == ExecutionContextStatus.Errored ||
                                  Status == ExecutionContextStatus.Completed ||
                                  Status == ExecutionContextStatus.Garbage || Status == ExecutionContextStatus.Stopped;

        public IMessageSink MessageSink { get; private set; }

        public bool IsImmediate { get; internal set; }

        public DateTime CreatedOn { get; private set; }

        public bool IsConnected => m_connectionState == ConnectionState.Connected;

        public IExecutionResult Result => m_result;

        public DateTime? ExecutionStart { get; private set; }

        public DateTime? StartTime
        {
            get => m_startTime;
            set
            {
                using (new WithWriteLock(ContextLock))
                {
                    m_startTime = value;
                    SaveUnderLock();
                }
            }
        }

        public ProgramPriority Priority
        {
            get => !IsComplete ? m_priority : ProgramPriority.Lowest;
            set
            {
                using (new WithWriteLock(ContextLock))
                {
                    m_priority = value;
                    SaveUnderLock();
                }
            }
        }

        public string Label
        {
            get => m_label;
            set
            {
                using (new WithWriteLock(ContextLock))
                {
                    m_label = value;
                    SaveUnderLock();
                }
            }
        }

        public int ResultCount => CachedResults.Count;

        public int StackDepth => DataStack.Count;

        public bool Send(string message)
        {
            using (new WithWriteLock(ContextLock))
            {
                return SendUnderLock(message);
            }
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (IsImmediate)
                return;
            info.AddValue("IsCooperative", IsCooperative);
            info.AddValue("ID", ID);
            info.AddValue("Label", Label);
            info.AddValue("ProgramName", ProgramName);
            info.AddValue("Priority", Priority.ToString());
            info.AddValue("Status", Status.ToString());
            info.AddValue("CreatedOn", CreatedOn);
            if (StartTime.HasValue)
            {
                info.AddValue("StartTime", StartTime);
                info.AddValue("HasStartTime", true);
            }
            else
            {
                info.AddValue("HasStartTime", false);
            }

            if (ExecutionStart.HasValue)
            {
                info.AddValue("ExecutionStartTime", ExecutionStart.Value);
                info.AddValue("HasExecutionStartTime", true);
            }
            else
            {
                info.AddValue("HasExecutionStartTime", false);
            }

            var errorItems = new StringBuilder();
            m_result.Errors.ForEach(each => errorItems.Append(string.Format("{0}|{1};", each, each.Details)));
            info.AddValue("Errors", errorItems.ToString());
            var executionTime = m_result.ExecutionTime;
            if (executionTime.HasValue)
            {
                var serializationInfo = info;
                executionTime = m_result.ExecutionTime;
                var str = executionTime.ToString();
                serializationInfo.AddValue("ExecutionTime", str);
                info.AddValue("HasExecutionTime", true);
            }
            else
            {
                info.AddValue("HasExecutionTime", false);
            }

            var stringBuilder1 = new StringBuilder();
            foreach (var message in Messages)
                stringBuilder1.Append(string.Format("{0};", message));
            info.AddValue("Messages", stringBuilder1.ToString());
            var stringBuilder2 = new StringBuilder();
            foreach (var cachedResult in CachedResults)
                stringBuilder2.Append(string.Format("{0};", cachedResult));
            info.AddValue("Results", stringBuilder2.ToString());
            info.AddValue("ParentID", ParentID);
            if (SymbolTable.Keys.Count > 0)
            {
                info.AddValue("SymbolTable", SymbolTable, SymbolTable.GetType());
                info.AddValue("HasSymbolTable", true);
            }
            else
            {
                info.AddValue("HasSymbolTable", false);
            }

            var stringBuilder3 = new StringBuilder();
            foreach (var data in DataStack)
                stringBuilder3.Append(string.Format("{0}={1};", data.GetType().FullName, data));
            info.AddValue("Stack", stringBuilder3);
            if (IsCooperative)
            {
                var stringBuilder4 = new StringBuilder();
                foreach (var call in Registers.CallStack)
                {
                    var str = string.Format("{0}|{1}|{2}|{3}|{4};", call.PC, call.X, call.Y, call.ProgramName,
                        call.ParentContext != null ? call.ParentContext.LineNumber : -1);
                    stringBuilder4.Append(str);
                }

                info.AddValue("CallFrames", stringBuilder4.ToString());
                var binaryFormatter = new BinaryFormatter();
                var stringBuilder5 = new StringBuilder();
                foreach (var key in ProgramCache.Keys)
                    try
                    {
                        stringBuilder5.Append(key + ";");
                        using (var serializationStream = File.OpenWrite(Path.Combine(ContextDirectory, key)))
                        {
                            binaryFormatter.Serialize(serializationStream, ProgramCache[key]);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log(string.Format("Unable to serialize program {0}.", key), ex);
                    }

                info.AddValue("Programs", stringBuilder5.ToString());
            }

            AppLog.Serialize(info);
        }

        public void Dispose()
        {
            if (m_disposed)
                return;
            m_disposed = true;
            try
            {
                Directory.Delete(ContextDirectory, true);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(string.Format("Unable to delete context directory {0}", ContextDirectory), ex);
            }

            CachedResults.Clear();
            DataStack.Clear();
            Messages.Clear();
            SymbolTable.Clear();
            CompletionWaitHandle.Close();
        }

        internal void Reset()
        {
            Suspend();
            CompletionWaitHandle.Reset();
            m_result.Reset();
            Registers.Reset();
        }

        internal void Save()
        {
            using (new WithWriteLock(ContextLock))
            {
                SaveUnderLock();
            }
        }

        internal object GetSymbolValue(string name, ErrorList errors)
        {
            var upper = name.ToUpper();
            object symbolValue;
            if (SymbolTable.TryGetValue(upper, out symbolValue))
                return symbolValue;
            errors.Add(Error.NewError("E002", string.Format("The symbol {0} is not defined.", upper),
                string.Format("Line {0}: {1}", Registers.ActiveFrame.IP.LineNumber, Registers.ActiveFrame.IP)));
            return null;
        }

        internal void SetSymbolValue(string name, object value)
        {
            SymbolTable[name.ToUpper()] = value;
        }

        internal void Execute()
        {
            var executionTimer = new ExecutionTimer();
            var executionContextStatus = ExecutionContextStatus.Completed;
            try
            {
                if (!ExecutionStart.HasValue)
                    ExecutionStart = DateTime.Now;
                ChangeStatusUnderLock(ExecutionContextStatus.Running);
                SendStatusMessage(ExecutionContextStatus.Running);
                AppLog.Open();
                if (Registers.ActiveFrame == null)
                    Registers.PushCallFrame(ProgramName, ProgramCache[ProgramName]);
                label_5:
                while (!Status.ShouldPromote())
                {
                    if (Registers.ActiveFrame.IP != null)
                        Registers.ActiveFrame.IP.Execute(m_result, this);
                    if (m_result.Errors.ContainsError())
                    {
                        Status.NextValue = executionContextStatus = ExecutionContextStatus.Errored;
                        if (!Registers.ActiveFrame.IP.ExitsContext)
                            LogHelper.Instance.Log("Job {0} had errors processing instruction at line {1}.", ID,
                                Registers.ActiveFrame.IP.LineNumber);
                    }
                    else if (m_result.RestartAtCurrentIP)
                    {
                        m_result.RestartAtCurrentIP = false;
                    }
                    else if (m_result.SwitchToContext != null)
                    {
                        Registers.PushCallFrame(m_result.SwitchToContext.GetProgram().Operands[0].Value,
                            m_result.SwitchToContext);
                        m_result.SwitchToContext = null;
                    }
                    else
                    {
                        do
                        {
                            if (!Registers.ActiveFrame.FetchNextInstruction())
                                Registers.PopCallFrame();
                            else
                                goto label_5;
                        } while (Registers.ActiveFrame != null);

                        Status.NextValue = executionContextStatus = ExecutionContextStatus.Completed;
                    }
                }

                Status.Promote();
                m_result.UpdateExecutionTime(executionTimer.Elapsed);
                SendStatusMessage(Status.Value);
            }
            catch (Exception ex)
            {
                m_result.Errors.Add(Error.NewError("E001", "An unhandled exception was raised by the execution engine.",
                    ex.ToString()));
                LogHelper.Instance.Log(
                    string.Format("An instruction in program {0} ( ID = {1} ) threw an unhandled exception.",
                        ProgramName, ID), ex);
                executionContextStatus = ExecutionContextStatus.Errored;
                Status.Promote(ExecutionContextStatus.Errored);
                m_result.UpdateExecutionTime(executionTimer.Elapsed);
            }
            finally
            {
                Save();
                executionTimer.Dispose();
                if (IsComplete)
                {
                    CompletionWaitHandle.Set();
                    if (!IsImmediate && ShouldCleanup)
                    {
                        CleanupHardware();
                        var msg = string.Format("Job {0} ID {1} ends with status {2}", ProgramName, ID,
                            executionContextStatus.ToString());
                        LogHelper.Instance.Log(msg);
                        AppLog.Write(msg);
                        CompletionWaitHandle.Close();
                    }

                    NotifyJobComplete();
                }

                AppLog.Close();
            }
        }

        internal void PushTop(object instance)
        {
            OnPush(instance, StackEnd.Top);
        }

        internal void PushBottom(object instance)
        {
            OnPush(instance, StackEnd.Bottom);
        }

        internal T PopTop<T>()
        {
            return ConversionHelper.ChangeType<T>(OnPop(StackEnd.Top));
        }

        internal object PopTop()
        {
            return OnPop(StackEnd.Top);
        }

        internal object PopBottom()
        {
            return OnPop(StackEnd.Bottom);
        }

        internal void RegisterHandler(Action<string> handler)
        {
            if (handler == null)
                return;
            using (new WithUpgradeableReadLock(SignalsLock))
            {
                if (SignalHandler != null)
                    return;
                using (new WithWriteLock(SignalsLock))
                {
                    SignalHandler = handler;
                    CachedSignals.ForEach(each => SignalHandler(each));
                    CachedSignals.Clear();
                }
            }
        }

        internal event OnHardwareCorrectionStart StartHardwareCorrectionEvent;

        internal event OnHardwareCorrectionEnd EndHardwareCorrectionEvent;

        internal event OnJobComplete JobCompleteEvent;

        internal void CopyProgramAndReferences(
            string programName,
            ProgramInstruction programInstruction)
        {
            ProgramName = programName;
            IsCooperative = programInstruction.IsCooperative;
            ProgramCache[ProgramName] = (ProgramInstruction)programInstruction.Clone();
            CopyReferences(ProgramCache[ProgramName]);
        }

        internal bool FlushMessages()
        {
            return FlushUnderLock();
        }

        internal void PromoteDeferredStatusChange()
        {
            var status = Status;
            var nullable1 = DeferredStatusChangeRequested;
            ExecutionContextStatus? nullable2 = (ExecutionContextStatus)((int?)nullable1 ?? 1);
            status.NextValue = nullable2;
            nullable1 = new ExecutionContextStatus?();
        }

        internal void CleanupHardware()
        {
            var service = ServiceLocator.Instance.GetService<IControlSystem>();
            service.SetSensors(false);
            service.StopRoller();
            service.ToggleRingLight(false, new int?());
        }

        internal void OnMoveVendError(ErrorCodes error)
        {
            OnMoveVendError(error, null);
        }

        internal void OnMoveVendError(ErrorCodes error, string barcode)
        {
            CreateInfoResult("MachineError", string.Format("MOVEVEND returned error {0}", error), barcode);
            Result.Errors.Add(Error.NewError("E999", "Execution context error.", "MACHINE ERROR"));
        }

        internal string CreateCameraCaptureErrorResult()
        {
            return CreateResult(StackEnd.Bottom, "CameraCaptureFailure", "The CAMERA CAPTURE instruction failed.",
                new int?(), new int?(), null, new DateTime?(), null);
        }

        internal string CreateItemStuckResult(string id)
        {
            return CreateResult(StackEnd.Bottom, "ItemStuckInGripper", "There is an item stuck in the gripper",
                new int?(), new int?(), id, new DateTime?(), null);
        }

        internal string CreateMoveErrorResult()
        {
            return CreateMoveErrorResult(new int?(), new int?());
        }

        internal string CreateMoveErrorResult(int? deck, int? slot)
        {
            return CreateResult(StackEnd.Bottom, "MachineError", "There was an error executing the MOVE instruction.",
                deck, slot, null, new DateTime?(), null);
        }

        internal void CreateLookupErrorResult(string id)
        {
            CreateLookupErrorResult("BarcodeNotFound", id);
        }

        internal void CreateLookupErrorResult(string code, string id)
        {
            CreateInfoResult(code, "The LOOKUP request failed because the item was not found.", id);
        }

        internal void CreateNoBarcodeReadResult()
        {
            CreateNoBarcodeReadResult(null);
        }

        internal void CreateNoBarcodeReadResult(string matrix)
        {
            CreateInfoResult("BadIdRead", "There were no barcodes found.", matrix);
        }

        internal void CreateDuplicateItemResult(string matrix)
        {
            CreateInfoResult("DuplicateDetected", "The read ID is a duplicate.", matrix);
        }

        internal void CreateMachineFullResult()
        {
            CreateMachineFullResult(null);
        }

        internal void CreateMachineFullResult(string id)
        {
            CreateInfoResult("MachineFull", "The machine is full.", id);
        }

        internal string CreateInfoResult(StackEnd end, string code, string message)
        {
            return CreateResult(end, code, message, new int?(), new int?(), null, new DateTime?(), null);
        }

        internal string CreateInfoResult(string code, string message)
        {
            return CreateResult(StackEnd.Bottom, code, message, new int?(), new int?(), null, new DateTime?(), null);
        }

        internal string CreateInfoResult(string code, string message, string id)
        {
            return CreateResult(StackEnd.Bottom, code, message, new int?(), new int?(), id, new DateTime?(), null);
        }

        internal string CreateResult(
            string code,
            string message,
            int? deck,
            int? slot,
            string item,
            DateTime? returnTime,
            string previousID)
        {
            return CreateResult(StackEnd.Bottom, code, message, deck, slot, item, returnTime, previousID);
        }

        internal string CreateResult(
            StackEnd end,
            string code,
            string message,
            int? deck,
            int? slot,
            string item,
            DateTime? returnTime,
            string previousID)
        {
            if (IsImmediate)
                return string.Empty;
            using (new WithWriteLock(ContextLock))
            {
                var str1 = "~";
                if (deck.HasValue && slot.HasValue)
                    str1 = string.Format("{0},{1}", deck, slot);
                var str2 = "~";
                if (returnTime.HasValue)
                    str2 = returnTime.ToString();
                var guid = Guid.NewGuid();
                var now = DateTime.Now;
                var str3 = item ?? "~";
                var str4 = previousID ?? "~";
                if (message.Contains("'"))
                    message = message.Replace('\'', ' ');
                var result = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}", "RESULT", guid, now, code, str1, str3,
                    message, str2, str4);
                CachedResults.Add(result);
                return result;
            }
        }

        private void SendStatusMessage(ExecutionContextStatus status)
        {
            using (new WithWriteLock(ContextLock))
            {
                SendUnderLock(string.Format(">STATUS CHANGE< {0}", status));
            }
        }

        private void CopyReferences(Instruction instruction)
        {
            if (instruction == null || instruction.Instructions.Count == 0)
                return;
            foreach (Instruction instruction1 in instruction.Instructions)
                if (instruction1.Instructions.Count > 0)
                    CopyReferences(instruction1);
        }

        private void ChangeStatus(ExecutionContextStatus status)
        {
            using (new WithWriteLock(ContextLock))
            {
                ChangeStatusUnderLock(status);
                SaveUnderLock();
            }
        }

        private void ChangeStatusUnderLock(ExecutionContextStatus status)
        {
            if (Status == ExecutionContextStatus.Running)
            {
                if (DeferredStatusChangeRequested.HasValue)
                    return;
                DeferredStatusChangeRequested = status;
            }
            else
            {
                Status.Promote(status);
            }
        }

        private bool SendUnderLock(string message)
        {
            var time = DateTime.Now;
            if (LastEventTime.HasValue)
            {
                var second1 = time.Second;
                var dateTime = LastEventTime.Value;
                var second2 = dateTime.Second;
                if (second1 <= second2)
                {
                    dateTime = LastEventTime.Value;
                    time = dateTime.AddSeconds(1.0);
                }
            }

            LastEventTime = time;
            Messages.PushTop(new ScriptMessage(time, message));
            return FlushUnderLock();
        }

        private bool FlushUnderLock()
        {
            if (m_connectionState != ConnectionState.Connected)
                return false;
            var num = 0;
            while (Messages.Count > 0)
            {
                var message = Messages.PopTop().ToString();
                LogHelper.Instance.Log(LogEntryType.Debug, "Job ID = {0}, Message = {1}", ID, message);
                try
                {
                    MessageSink.Send(message);
                    ++num;
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log(LogEntryType.Error,
                        "Job {0} ID {1} failed to send message: exception message = {2}", ProgramName, ID, ex.Message);
                }
            }

            return num > 0;
        }

        private void SaveUnderLock()
        {
            if (IsImmediate)
                return;
            if (IsHydrating)
                LogHelper.Instance.Log("[ExecutionContext] !! Save under lock while hydrating !!");
            try
            {
                if (!Directory.Exists(ContextDirectory))
                    Directory.CreateDirectory(ContextDirectory);
                using (var serializationStream = File.OpenWrite(Path.Combine(ContextDirectory, "ctx.data")))
                {
                    new BinaryFormatter().Serialize(serializationStream, this);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("ExecutionContext.Save threw an unhandled exception.", ex);
            }
        }

        private void OnPush(object instance, StackEnd end)
        {
            if (StackEnd.Top == end)
            {
                DataStack.PushTop(instance);
            }
            else
            {
                if (end != StackEnd.Bottom)
                    return;
                DataStack.PushBottom(instance);
            }
        }

        private object OnPop(StackEnd end)
        {
            return StackEnd.Top != end ? DataStack.PopBottom() : DataStack.PopTop();
        }

        private void NotifyJobComplete()
        {
            var jobCompleteEvent = JobCompleteEvent;
            if (jobCompleteEvent == null)
                return;
            jobCompleteEvent(Status.Value);
        }
    }
}