using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Threading;
using Redbox.HAL.Component.Model.Timers;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    internal sealed class ProgramCache : IDisposable
    {
        private readonly ReaderWriterLockSlim CacheLock = new ReaderWriterLockSlim();

        private readonly IDictionary<string, CompiledProgram> CompiledPrograms =
            new Dictionary<string, CompiledProgram>();

        private readonly Dictionary<string, string> JobToOperand = new Dictionary<string, string>();
        private readonly List<string> NoShowJobs = new List<string>();
        private readonly IDictionary<string, Type> OperandToType = new Dictionary<string, Type>();
        private bool Disposed;

        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            JobToOperand.Clear();
            OperandToType.Clear();
            CompiledPrograms.Clear();
            GC.SuppressFinalize(this);
        }

        internal void CompilePrograms(string scriptsDirectory, ErrorList errorList)
        {
            if (JobToOperand.Keys.Count > 0)
                return;
            JobToOperand["return"] = "RETURN";
            JobToOperand["quick-return"] = "QUICKRETURN";
            using (var executionTimer = new ExecutionTimer())
            {
                var assemblyFile = ServiceLocator.Instance.GetService<IRuntimeService>()
                    .RuntimePath(Assembly.GetExecutingAssembly().GetName().Name + ".dll");
                try
                {
                    RegisterNatives(Assembly.LoadFrom(assemblyFile).GetTypes());
                }
                catch (BadImageFormatException ex)
                {
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log(
                        string.Format("Unable to load assembly '{0}' to scan for commands.", assemblyFile), ex);
                }

                executionTimer.Stop();
                LogHelper.Instance.Log(string.Format("[Program Cache] Time to scan for {0} native jobs: {1}",
                    JobToOperand.Keys.Count, executionTimer.Elapsed));
            }

            foreach (var key in JobToOperand.Keys)
            {
                var programName = key;
                var programSource = string.Format("NATIVEJOB {0}", JobToOperand[key]);
                LogHelper.Instance.Log(LogEntryType.Debug, "Compile native program {0}", programName);
                var executionResult = CompileStringSource(programName, programSource);
                errorList.AddRange(executionResult.Errors);
                if (executionResult.Errors.ContainsError())
                {
                    LogHelper.Instance.Log(string.Format("There were errors compiling native script {0}", programName),
                        LogEntryType.Error);
                    foreach (object error in executionResult.Errors)
                        LogHelper.Instance.Log(error.ToString(), LogEntryType.Error);
                }
            }

            using (var executionTimer = new ExecutionTimer())
            {
                var files = Directory.GetFiles(scriptsDirectory, "*.hs");
                foreach (var str in files)
                {
                    var withoutExtension = Path.GetFileNameWithoutExtension(str);
                    if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
                        LogHelper.Instance.Log(
                            string.Format("Compile program {0} from file {1}", withoutExtension, str),
                            LogEntryType.Info);
                    using (var memoryStream =
                           new MemoryStream(ServiceLocator.Instance.GetService<IRuntimeService>().ReadToBuffer(str)))
                    {
                        var executionResult = CompileInner(memoryStream, withoutExtension);
                        errorList.AddRange(executionResult.Errors);
                        if (executionResult.Errors.ContainsError())
                        {
                            LogHelper.Instance.Log(
                                string.Format("There were errors compiling script {0}", withoutExtension),
                                LogEntryType.Error);
                            foreach (object error in executionResult.Errors)
                                LogHelper.Instance.Log(error.ToString(), LogEntryType.Error);
                        }
                    }
                }

                executionTimer.Stop();
                LogHelper.Instance.Log("[Program Cache] Time to compile {0} programs: {1}", files.Length,
                    executionTimer.Elapsed);
            }
        }

        internal NativeJobAdapter MakeJobFromOperand(
            ExecutionContext ctx,
            ExecutionResult result,
            string key)
        {
            var nativeJobAdapter = (NativeJobAdapter)null;
            if (key == "RETURN")
                nativeJobAdapter = new ReturnJob(result, ctx);
            else if (OperandToType.ContainsKey(key))
                try
                {
                    nativeJobAdapter = (NativeJobAdapter)Activator.CreateInstance(OperandToType[key],
                        BindingFlags.Instance | BindingFlags.NonPublic, null, new object[2]
                        {
                            result,
                            ctx
                        }, null);
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log("Unable to create job {0}", key);
                    LogHelper.Instance.Log(ex.Message);
                    nativeJobAdapter = null;
                }

            return nativeJobAdapter;
        }

        internal ExecutionResult CompileStringSource(string programName, string programSource)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream))
                {
                    streamWriter.WriteLine(programSource);
                    streamWriter.Flush();
                    memoryStream.Seek(0L, SeekOrigin.Begin);
                    return CompileInner(memoryStream, programName);
                }
            }
        }

        internal ExecutionResult CompileInner(Stream stream, string programName)
        {
            using (new WithUpgradeableReadLock(CacheLock))
            {
                if (GetProgram(programName) == null)
                    using (new WithWriteLock(CacheLock))
                    {
                        return CompileProgramInner(stream, programName);
                    }

                if (programName.Equals("$$$immediate$$$", StringComparison.CurrentCultureIgnoreCase))
                    using (new WithWriteLock(CacheLock))
                    {
                        CompiledPrograms.Remove("$$$immediate$$$");
                        return CompileProgramInner(stream, programName);
                    }

                if (ControllerConfiguration.Instance.AllowProgramCacheReplacement2)
                    using (new WithWriteLock(CacheLock))
                    {
                        CompiledPrograms.Remove(programName);
                        LogHelper.Instance.Log(
                            string.Format("Allow replacement of program {0} - allow cache replacement2 is true.",
                                programName), LogEntryType.Info);
                        return CompileProgramInner(stream, programName);
                    }

                if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
                    LogHelper.Instance.Log(string.Format("Not allowing replacement of program {0}", programName),
                        LogEntryType.Info);
                return new ExecutionResult
                {
                    ExecutionTime = new TimeSpan(0L)
                };
            }
        }

        internal CompiledProgram GetProgram(string name)
        {
            using (new WithReadLock(CacheLock))
            {
                return CompiledPrograms.ContainsKey(name) ? CompiledPrograms[name] : null;
            }
        }

        internal bool RemoveProgram(string name)
        {
            using (new WithWriteLock(CacheLock))
            {
                return CompiledPrograms.Remove(name);
            }
        }

        internal List<string> ProgramList()
        {
            var stringList = new List<string>();
            using (new WithReadLock(CacheLock))
            {
                foreach (var key1 in CompiledPrograms.Keys)
                {
                    var key = key1;
                    if (NoShowJobs.Find(each => each.Equals(key, StringComparison.CurrentCultureIgnoreCase)) == null)
                        stringList.Add(key);
                }
            }

            return stringList;
        }

        private ExecutionResult CompileProgramInner(Stream stream, string programName)
        {
            ProgramInstruction compiledProgram;
            var compileResult = new Compiler(stream, programName).Compile(out compiledProgram);
            CompiledPrograms.Add(programName, new CompiledProgram(compileResult, compiledProgram));
            return compileResult;
        }

        private void RegisterNatives(IEnumerable<Type> types)
        {
            var num1 = 0;
            var num2 = 0;
            foreach (var type in types)
            {
                var customAttributes =
                    (NativeJobAttribute[])type.GetCustomAttributes(typeof(NativeJobAttribute), false);
                if (customAttributes != null && customAttributes.Length != 0)
                    foreach (var nativeJobAttribute in customAttributes)
                        if (nativeJobAttribute.ProgramName != null)
                        {
                            if (nativeJobAttribute.Operand == null)
                                nativeJobAttribute.Operand = nativeJobAttribute.ProgramName.ToUpper();
                            OperandToType.Add(nativeJobAttribute.Operand, type);
                            JobToOperand.Add(nativeJobAttribute.ProgramName, nativeJobAttribute.Operand);
                            ++num2;
                            if (nativeJobAttribute.HideFromList)
                                NoShowJobs.Add(nativeJobAttribute.ProgramName);
                            var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
                            if (constructors == null || constructors.Length == 0)
                                LogHelper.Instance.Log("Type {0} does not express a non-public ctor.", type.ToString());
                            else
                                foreach (MethodBase methodBase in constructors)
                                {
                                    var parameters = methodBase.GetParameters();
                                    if (parameters == null || parameters.Length == 0)
                                        LogHelper.Instance.Log("Type {0} has a ctor that does not match.",
                                            type.ToString());
                                    else if (parameters.Length != 2)
                                        LogHelper.Instance.Log("Type {0} does not have a 2-parameter ctor.",
                                            type.ToString());
                                    else if (parameters[0].ParameterType.Name != "ExecutionResult" ||
                                             parameters[1].ParameterType.Name != "ExecutionContext")
                                        LogHelper.Instance.Log(
                                            "Type {0} does not have a ctor that matches the expected parameter list.",
                                            type.ToString());
                                    else
                                        ++num1;
                                }
                        }
            }

            if (num1 == num2)
                return;
            LogHelper.Instance.Log("There is a mismatch between ctor count ({0}) & job keys ({1})", num1, num2);
        }
    }
}