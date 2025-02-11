using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Timers;
using Microsoft.Win32;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Component.Model.Threading;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.Script.Framework.Services;

namespace Redbox.HAL.Script.Framework
{
    public sealed class ExecutionEngine : IExecutionService, IConfigurationObserver
    {
        private static readonly object InstanceLock = new object();
        private readonly ReaderWriterLockSlim ContextLock = new ReaderWriterLockSlim();
        private readonly ExecutionContextList Contexts = new ExecutionContextList();
        internal readonly string DataDirectory;
        private readonly IdHelper IdGenerator = new IdHelper();
        private readonly ExecutionContext ImmediateModeContext;
        private readonly object ModeLock = new object();

        private readonly string[] NonRestartableJobs = new string[4]
        {
            "init",
            "return",
            "vend",
            "soft-sync"
        };

        private readonly string[] NonSchedulableJobsWithinReboot = new string[7]
        {
            "qlm-unload",
            "thin",
            "unload-thin",
            "sync",
            "sync-locations",
            "thin-vmz",
            "clean-vmz"
        };

        private readonly PollerManager PollerManager = new PollerManager();
        private readonly ProgramCache ProgramCache = new ProgramCache();
        private readonly RedboxTimer RebootWakeupTimer;
        private readonly ManualResetEvent SchedulerResetEvent = new ManualResetEvent(false);
        private readonly string ScriptsPath;

        private readonly string[] SingletonJobs = new string[1]
        {
            "clean-vmz"
        };

        private ExecutionContext ActiveContext;
        private bool ContextSwitchRequested;
        private TimeSpan? RebootWindowStart;
        private bool SchedulerEnabled;
        private Thread SchedulerThread;

        private ExecutionEngine(string scriptsPath)
        {
            Mode = EngineModes.Normal;
            ScriptsPath = scriptsPath;
            RebootWakeupTimer = new RedboxTimer("Reboot Wakeup", OnRebootWakeup);
            DataDirectory = ServiceLocator.Instance.GetService<IRuntimeService>().RuntimePath("data");
            ImmediateModeContext = new ExecutionContext("IMMEDIATE", "$$$immediate$$$", new DateTime?(),
                ProgramPriority.Immediate, "HAL Script Immediate Mode")
            {
                IsImmediate = true
            };
            Contexts.Add(ImmediateModeContext);
            InitStatus = ExecutionContextStatus.Pending;
        }

        internal static ExecutionEngine Instance { get; private set; }

        internal ExecutionContextStatus InitStatus { get; private set; }

        internal TimeSpan? KioskRebootTime { get; private set; }

        internal EngineModes Mode { get; private set; }

        public void NotifyConfigurationLoaded()
        {
            var preventionWindow = ControllerConfiguration.Instance.RebootExecutionPreventionWindow;
            LogHelper.Instance.Log(LogEntryType.Debug,
                "[ExecutionEngine] NotifyConfigurationLoaded reboot window = {0}", preventionWindow);
            if (preventionWindow <= 0)
                return;
            try
            {
                using (var subKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store"))
                {
                    var s = string.Empty;
                    var obj = subKey.GetValue("RebootTime");
                    LogHelper.Instance.Log(LogEntryType.Debug, "[ExecutionEngine] rtValue = {0}", obj.ToString());
                    if (obj != null)
                        s = ConversionHelper.ChangeType<string>(obj);
                    if (string.IsNullOrEmpty(s))
                        return;
                    try
                    {
                        KioskRebootTime = TimeSpan.Parse(s);
                        RebootWindowStart = KioskRebootTime.Value.Subtract(new TimeSpan(0, preventionWindow, 0));
                        LogHelper.Instance.Log(
                            "[ExecutionEngine] Configuration load KioskRebootTime = {0} RebootWindowStart = {1}",
                            KioskRebootTime, RebootWindowStart);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log("[ExecutionEngine] Parsing kiosk reboot time failed.", ex);
                        KioskRebootTime = RebootWindowStart = new TimeSpan?();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("[ExecutionEngine] Unable to read secret option 'RebootTime'", ex);
            }
        }

        public void NotifyConfigurationChangeStart()
        {
        }

        public void NotifyConfigurationChangeEnd()
        {
        }

        public IExecutionContext ScheduleJob(
            string progName,
            string label,
            DateTime? startTime,
            ProgramPriority priority)
        {
            return ScheduleJobInternal(progName, label, startTime, priority);
        }

        public IExecutionContext GetActiveContext()
        {
            return ActiveContext;
        }

        public IExecutionContext GetJob(string jobId)
        {
            return Contexts.GetByID(jobId);
        }

        public IExecutionContext[] GetByLabel(string label)
        {
            return Contexts.GetByLabel(label);
        }

        public string[] GetProgramList()
        {
            var stringList = new List<string>();
            var list = ProgramCache.ProgramList();
            using (new DisposeableList<string>(list))
            {
                foreach (var name in list)
                    if (!name.StartsWith("$"))
                        stringList.Add(string.Format("{0}|{1}|{2}", name, ProgramCache.GetProgram(name), false));
            }

            return stringList.ToArray();
        }

        public ExecutionContextStatus GetInitJobStatus()
        {
            return InitStatus;
        }

        public void RemoveProgram(string programName)
        {
            ProgramCache.RemoveProgram(programName);
        }

        public void CleanupJobs(bool force)
        {
            Contexts.CleanupGarbage(force);
        }

        public void Enter(EngineModes mode)
        {
            lock (ModeLock)
            {
                Mode |= mode;
                LogHelper.Instance.Log("[Execution Engine]: Entering mode {0}", mode.ToString());
                OnModeChanged();
            }
        }

        public void Exit(EngineModes mode)
        {
            lock (ModeLock)
            {
                Mode &= ~mode;
                LogHelper.Instance.Log("[Execution Engine]: Exiting mode {0}", mode.ToString());
                OnModeChanged();
            }
        }

        public void Initialize(ErrorList errors)
        {
            ControllerConfiguration.Instance.AddObserver(this);
            ServiceLocator.Instance.AddService<IBarcodeValidatorService>(new RedboxBarcodeValidator());
            ServiceLocator.Instance.AddService<IFraudService>(new FraudValidatorService());
            PollerManager.InitializePollers();
            ProgramCache.CompilePrograms(ScriptsPath, errors);
            LoadJobs();
        }

        public void Start()
        {
            RestartYoungest(5);
            CleanupJobs(false);
            SchedulerThread = new Thread(ProcessScheduler)
            {
                Name = "HAL Script Scheduler",
                IsBackground = true
            };
            SchedulerThread.SetApartmentState(ApartmentState.STA);
            SchedulerThread.Start();
            SchedulerEnabled = true;
            LogHelper.Instance.Log("[ExecutionEngine] Version {0} initialized.",
                typeof(ExecutionEngine).Assembly.GetName().Version);
            ScheduleInit(true);
        }

        public void Shutdown()
        {
            PollerManager.StopPollers();
            SchedulerEnabled = false;
            SuspendAll(true);
            while (ActiveContext != null)
                Thread.Sleep(0);
            LogHelper.Instance.Log("ExecutionEngine.Shutdown() complete.");
        }

        public void Suspend()
        {
            PollerManager.StopPollers();
            SuspendAll(true);
        }

        public void Restart()
        {
            ScheduleInit(false);
            RestartYoungest(5);
            PollerManager.StartPollers();
        }

        public IExecutionResult CompileImmediate(Stream s)
        {
            return Compile(s, "$$$immediate$$$");
        }

        public IExecutionResult Compile(Stream stream, string programName)
        {
            return ProgramCache.CompileInner(stream, programName);
        }

        public IExecutionContext ExecuteImmediate()
        {
            ImmediateModeContext.Reset();
            var program = ProgramCache.GetProgram("$$$immediate$$$");
            if (program == null || program.CompilerResult.Errors.ContainsError())
            {
                ImmediateModeContext.Result.Errors.Add(Error.NewError("E001",
                    string.Format("The program '{0}' was not found in the program cache.", "$$$immediate$$$"),
                    "Recompile the program which will place it in the program cache."));
                return ImmediateModeContext;
            }

            ImmediateModeContext.CopyProgramAndReferences("$$$immediate$$$", program.ParseTree);
            ImmediateModeContext.Priority = ProgramPriority.Immediate;
            ImmediateModeContext.Pend();
            return ImmediateModeContext;
        }

        public List<IExecutionContext> GetJobList()
        {
            var rv = new List<IExecutionContext>();
            Contexts.ForEach(each => rv.Add(each));
            return rv;
        }

        public void PushValue(IExecutionContext context, string value, StackEnd end, ErrorList errors)
        {
            var tokenizer = new Tokenizer(1, string.Format("PUSH " + value));
            tokenizer.Tokenize();
            if (tokenizer.Errors.ContainsError())
                errors.AddRange(tokenizer.Errors);
            else
                foreach (var token in tokenizer.Tokens)
                    if (token.Type == TokenType.NumericLiteral || token.Type == TokenType.StringLiteral)
                    {
                        var instance = token.ConvertValue();
                        context.Push(instance, end);
                    }
        }

        public bool IsImmediateJobPending => ImmediateModeContext.Status == ExecutionContextStatus.Pending ||
                                             ImmediateModeContext.Status == ExecutionContextStatus.Running;

        public event EventHandler<EngineModeChangeEventArgs> EngineModeChange;

        public static IExecutionService NewEngine(string scriptsPath)
        {
            lock (InstanceLock)
            {
                if (Instance == null)
                    Instance = new ExecutionEngine(scriptsPath);
                return Instance;
            }
        }

        public IExecutionResult Compile(string programSource, string programName)
        {
            return ProgramCache.CompileStringSource(programName, programSource);
        }

        public void SuspendAll(bool force)
        {
            Contexts.ForEach(each =>
            {
                if (each.IsComplete)
                    return;
                each.Suspend();
                if (!force)
                    return;
                if (each.Status == ExecutionContextStatus.Running)
                    each.PromoteDeferredStatusChange();
                else
                    each.Save();
            });
        }

        public string GetActiveContextProgramName()
        {
            return ActiveContext == null || ActiveContext.Registers.ActiveFrame == null
                ? "Idle"
                : ActiveContext.Registers.ActiveFrame.ProgramName;
        }

        internal NativeJobAdapter MakeJobFromOperand(
            ExecutionContext ctx,
            ExecutionResult result,
            string key)
        {
            return ProgramCache.MakeJobFromOperand(ctx, result, key);
        }

        internal void PerformContextSwitch(bool runtimeOnly)
        {
            ActiveContext.PromoteDeferredStatusChange();
            if (runtimeOnly)
                return;
            ActiveContext.Save();
            ContextSwitchRequested = true;
            SchedulerResetEvent.Set();
        }

        internal ExecutionContext ScheduleJobInternal(
            string progName,
            string label,
            DateTime? startTime,
            ProgramPriority priority)
        {
            var programName = progName;
            var executionContext = new ExecutionContext(GetUniqueId(), programName, startTime, priority, label);
            var program = ProgramCache.GetProgram(progName);
            if (program == null)
            {
                executionContext.Result.Errors.Add(Error.NewError("E001",
                    string.Format("The program '{0}' was not found in the program cache.", programName),
                    "Recompile the program which will place it in the program cache."));
                return executionContext;
            }

            using (new WithUpgradeableReadLock(ContextLock))
            {
                if (IsSingletonJob(progName) && Contexts.Find(each =>
                        each.ProgramName.Equals(progName, StringComparison.CurrentCultureIgnoreCase) &&
                        !each.IsComplete) != null)
                {
                    var str = string.Format(
                        "[ExecutionEngine] The program '{0}' is marked singleton, instance currently running - deny new job.",
                        programName);
                    LogHelper.Instance.Log(str);
                    executionContext.Result.Errors.Add(Error.NewError("E001", str, "Reschedule the program ."));
                    return executionContext;
                }

                executionContext.CopyProgramAndReferences(programName, program.ParseTree);
                using (new WithWriteLock(ContextLock))
                {
                    Contexts.Add(executionContext);
                }
            }

            executionContext.Save();
            return executionContext;
        }

        internal void ProcessScheduler()
        {
            while (true)
            {
                Thread.Sleep(0);
                if (!CanSchedule())
                    SuspendCritical(true);
                if (SchedulerEnabled)
                {
                    if (ContextSwitchRequested || ActiveContext == null)
                    {
                        ActiveContext = Contexts.GetEligibleExecutionContext();
                        ContextSwitchRequested = false;
                        SchedulerResetEvent.Reset();
                    }

                    if (ActiveContext != null && ActiveContext.Status != ExecutionContextStatus.Running)
                    {
                        ActiveContext.Execute();
                        ActiveContext = null;
                    }
                }

                SchedulerResetEvent.WaitOne(250, false);
            }
        }

        private bool IsSingletonJob(string name)
        {
            return Array.FindIndex(SingletonJobs,
                singleton => singleton.Equals(name, StringComparison.CurrentCultureIgnoreCase)) != -1;
        }

        private void SuspendCritical(bool force)
        {
            Contexts.FindAll(each => !each.IsComplete && each.Status != ExecutionContextStatus.Suspended).ForEach(
                each =>
                {
                    if (string.IsNullOrEmpty(Array.Find(NonSchedulableJobsWithinReboot,
                            job => job == each.ProgramName)))
                        return;
                    each.Suspend();
                    if (!force)
                        return;
                    if (each.Status == ExecutionContextStatus.Running)
                        each.PromoteDeferredStatusChange();
                    else
                        each.Save();
                });
        }

        private bool CanSchedule()
        {
            if (!KioskRebootTime.HasValue || !RebootWindowStart.HasValue)
                return true;
            var timeOfDay = DateTime.Now.TimeOfDay;
            if (!(timeOfDay >= RebootWindowStart.Value) || !(timeOfDay <= KioskRebootTime.Value))
                return true;
            if (!RebootWakeupTimer.Started)
            {
                RebootWakeupTimer.ScheduleAtNext(KioskRebootTime.Value.Add(new TimeSpan(0, 5, 0)));
                LogHelper.Instance.Log("Time {0} is within reboot window of {1}; disabling scheduler.",
                    FormatTime(timeOfDay), FormatTime(KioskRebootTime.Value));
                PollerManager.StopPollers();
            }

            return false;
        }

        private string FormatTime(TimeSpan t)
        {
            return string.Format("{0} {1}", DateTime.Now.ToShortDateString(), t.ToString());
        }

        private string GetUniqueId()
        {
            using (new WithReadLock(ContextLock))
            {
                var id = ImmediateModeContext.ID;
                for (var index = 0; index < 8; ++index)
                {
                    var pseudoUniqueId = IdGenerator.GeneratePseudoUniqueId(8);
                    if (pseudoUniqueId != id && Contexts.GetByID(pseudoUniqueId) == null)
                        return pseudoUniqueId;
                }
            }

            throw new ApplicationException("Unable to generate a unique ID for ExecutionContext.");
        }

        private void OnRebootWakeup(object source, ElapsedEventArgs e)
        {
            RebootWakeupTimer.Disable();
            RestartYoungest(1);
            PollerManager.StartPollers();
        }

        private void ScheduleInit(bool isBootstrap)
        {
            InitStatus = ExecutionContextStatus.Pending;
            var executionContext =
                ScheduleJobInternal("init", "HAL Service Startup: Init", DateTime.Now, ProgramPriority.High);
            executionContext.JobCompleteEvent += InitJob_JobCompleteEvent;
            LogHelper.Instance.Log("[ExecutionEngine] Schedule init job {0}", executionContext.ID);
            if (isBootstrap)
                executionContext.SetSymbolValue("__EE_BOOTSTRAP_INIT", true);
            executionContext.Pend();
        }

        private void InitJob_JobCompleteEvent(ExecutionContextStatus status)
        {
            PollerManager.StartPollers();
            InitStatus = status;
        }

        private void RestartYoungest(int delay)
        {
            foreach (var name in NonSchedulableJobsWithinReboot)
            {
                var byName = Contexts.GetByName(name);
                if (byName.Length != 0)
                {
                    Array.Sort(byName, (left, right) => left.CreatedOn.CompareTo(right.CreatedOn));
                    for (var index = byName.Length - 1; index >= 0; --index)
                    {
                        var executionContext = byName[index];
                        if (!executionContext.IsComplete)
                        {
                            var now = DateTime.Now;
                            var startTime = executionContext.StartTime;
                            if (startTime.HasValue)
                            {
                                startTime = executionContext.StartTime;
                                if (!(startTime.Value < DateTime.Now.AddMinutes(delay)))
                                    goto label_7;
                            }

                            executionContext.StartTime = DateTime.Now.AddMinutes(delay);
                            label_7:
                            LogHelper.Instance.Log("[Execution Engine] Restarting job {0} ( ID = {1} )",
                                executionContext.ProgramName, executionContext.ID);
                            executionContext.Status.NextValue = ExecutionContextStatus.Pending;
                            executionContext.Status.Promote();
                            break;
                        }
                    }
                }
            }
        }

        private void LoadJobs()
        {
            var path1 = Path.Combine(DataDirectory, "contexts");
            try
            {
                if (!Directory.Exists(path1))
                {
                    LogHelper.Instance.Log("[ExecutionEngine] Contexts directory doesn't exist; creating.",
                        LogEntryType.Debug);
                    Directory.CreateDirectory(path1);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("[ExecutionEngine] Unable to create contexts directory.", ex);
            }

            Array.ForEach(Directory.GetDirectories(path1), contextPath =>
            {
                var path2 = Path.Combine(contextPath, "ctx.data");
                var executionContext = (ExecutionContext)null;
                try
                {
                    using (var serializationStream = File.OpenRead(path2))
                    {
                        executionContext = (ExecutionContext)new BinaryFormatter().Deserialize(serializationStream);
                    }

                    if (executionContext == null)
                        return;
                    if (executionContext.Status.Value == ExecutionContextStatus.Suspended &&
                        Array.IndexOf(NonRestartableJobs, executionContext.ProgramName) > -1)
                    {
                        LogHelper.Instance.Log(
                            "[ExecutionEngine] Job {0} ( {1} ) is in pending status at initialization; mark it garbage.",
                            executionContext.ID, executionContext.ProgramName);
                        executionContext.Status.NextValue = ExecutionContextStatus.Garbage;
                        executionContext.Status.Promote();
                    }

                    Contexts.Add(executionContext);
                }
                catch
                {
                    LogHelper.Instance.Log("[ExecutionEngine] Unable to load context {0}; deleting.", contextPath);
                    try
                    {
                        Directory.Delete(contextPath, true);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log("[ExecutionEngine] Unable to delete context", ex);
                    }
                }
            });
        }

        private void OnModeChanged()
        {
            var engineModeChange = EngineModeChange;
            if (engineModeChange == null)
                return;
            engineModeChange(this, new EngineModeChangeEventArgs(Mode));
        }
    }
}