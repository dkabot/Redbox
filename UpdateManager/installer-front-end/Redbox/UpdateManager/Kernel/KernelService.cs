using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading;

namespace Redbox.UpdateManager.Kernel
{
    internal sealed class KernelService : IKernelService
    {
        private bool _performedShutdown;
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private int _lockTimeout = 3000;

        public static KernelService Instance => Singleton<KernelService>.Instance;

        public void Initialize()
        {
            this.Reset();
            this.ShutdownType = ShutdownType.None;
            ServiceLocator.Instance.AddService(typeof(IKernelService), (object)this);
        }

        public void Reset()
        {
            if (!this._lock.TryEnterWriteLock(this._lockTimeout))
            {
                new ErrorList().Add(Redbox.UpdateManager.ComponentModel.Error.NewError("KernelService.Reset", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
            }
            else
            {
                try
                {
                    this.InternalReset();
                }
                finally
                {
                    this._lock.ExitWriteLock();
                }
            }
        }

        public Dictionary<object, object> GetTable(string name)
        {
            if (!(this.LuaRuntime[name] is LuaTable luaTable))
                return (Dictionary<object, object>)null;
            Dictionary<object, object> table = new Dictionary<object, object>();
            foreach (object key in (IEnumerable)luaTable.Keys)
                table.Add(key, luaTable[key]);
            return table;
        }

        public string FormatLuaValue(object value) => LuaHelper.FormatLuaValue(value);

        public ReadOnlyCollection<object> ExecuteChunk(string chunk)
        {
            return this.ExecuteChunkLockCheck(chunk, out bool _);
        }

        public ReadOnlyCollection<object> ExecuteChunkLockCheck(string chunk, out bool locked)
        {
            locked = false;
            if (!this._lock.TryEnterWriteLock(this._lockTimeout))
            {
                locked = true;
                new ErrorList().Add(Redbox.UpdateManager.ComponentModel.Error.NewError("KernelService.ExecuteChunck(chunk)", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
                return (ReadOnlyCollection<object>)null;
            }
            try
            {
                return this.IsShuttingDown ? (ReadOnlyCollection<object>)null : this.LuaRuntime.DoString(chunk);
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        public ReadOnlyCollection<object> ExecuteChunk(string chunk, bool reset)
        {
            if (!this._lock.TryEnterWriteLock(this._lockTimeout))
            {
                new ErrorList().Add(Redbox.UpdateManager.ComponentModel.Error.NewError("KernelService.ExecuteChunck(chunk,reset)", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
                return (ReadOnlyCollection<object>)null;
            }
            try
            {
                if (this.IsShuttingDown)
                    return (ReadOnlyCollection<object>)null;
                if (reset)
                    this.InternalReset();
                return this.LuaRuntime.DoString(chunk);
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        public ReadOnlyCollection<object> ExecuteChunkNoLock(
          string chunk,
          bool reset,
          string resultTableName,
          out Dictionary<object, object> result,
          out bool scriptCompleted)
        {
            return this.InternalExecuteChunk(chunk, reset, resultTableName, out result, out scriptCompleted);
        }

        public ReadOnlyCollection<object> ExecuteChunk(
          string chunk,
          bool reset,
          string resultTableName,
          out Dictionary<object, object> result,
          out bool scriptCompleted)
        {
            if (!this._lock.TryEnterWriteLock(this._lockTimeout))
            {
                new ErrorList().Add(Redbox.UpdateManager.ComponentModel.Error.NewError("KernelService.ExecuteChunck(chunck,reset,resulttablename,out,out", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
                result = (Dictionary<object, object>)null;
                scriptCompleted = false;
                return (ReadOnlyCollection<object>)null;
            }
            try
            {
                return this.InternalExecuteChunk(chunk, reset, resultTableName, out result, out scriptCompleted);
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        public void Execute(Action action)
        {
            if (!this._lock.TryEnterWriteLock(this._lockTimeout))
            {
                new ErrorList().Add(Redbox.UpdateManager.ComponentModel.Error.NewError("KernelService.Execute", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
            }
            else
            {
                try
                {
                    if (this.IsShuttingDown || action == null)
                        return;
                    action();
                }
                finally
                {
                    this._lock.ExitWriteLock();
                }
            }
        }

        public void Shutdown()
        {
            if (this.LuaRuntime == null)
                return;
            this.LuaRuntime.Dispose();
        }

        public void AddGlobal(string key, object value)
        {
            if (value is string)
                value = (object)ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties((string)value);
            this.LuaRuntime[key] = value;
        }

        public void SetScriptComplete() => this.ScriptCompleted = true;

        public string StartupScript { get; set; }

        public bool ScriptCompleted { get; private set; }

        public ShutdownType ShutdownType { get; set; }

        public bool IsShuttingDown => this.ShutdownType != 0;

        public void PerformShutdown()
        {
            if (this._performedShutdown)
                return;
            switch (this.ShutdownType)
            {
                case ShutdownType.Reboot:
                    this._performedShutdown = true;
                    ShutdownTool.Shutdown(ShutdownFlags.Reboot | ShutdownFlags.Force | ShutdownFlags.ForceIfHung, ShutdownReason.FlagPlanned);
                    break;
                case ShutdownType.Shutdown:
                    this._performedShutdown = true;
                    ShutdownTool.Shutdown(ShutdownFlags.ShutDown | ShutdownFlags.Force, ShutdownReason.FlagPlanned);
                    break;
            }
        }

        internal void ExecutePreloadScripts()
        {
        }

        internal void InternalReset()
        {
            this.ScriptCompleted = false;
            if (this.LuaRuntime != null)
            {
                this.LuaRuntime.Dispose();
                this.LuaRuntime = (LuaVirtualMachine)null;
            }
            this.LuaRuntime = new LuaVirtualMachine();
            this.ConfigureKernelFunctions();
            this.ExecutePreloadScripts();
        }

        internal ReadOnlyCollection<object> InternalExecuteChunk(
          string chunk,
          bool reset,
          string resultTableName,
          out Dictionary<object, object> result,
          out bool scriptCompleted)
        {
            result = (Dictionary<object, object>)null;
            scriptCompleted = false;
            if (this.IsShuttingDown)
                return (ReadOnlyCollection<object>)null;
            if (reset)
                this.InternalReset();
            ReadOnlyCollection<object> readOnlyCollection = this.LuaRuntime.DoString(chunk);
            result = this.GetTable(resultTableName);
            scriptCompleted = this.ScriptCompleted;
            return readOnlyCollection;
        }

        internal void ConfigureKernelFunctions()
        {
            try
            {
                this.LuaRuntime["kernel"] = (object)new LuaTable(this.LuaRuntime);
                foreach (Type type in typeof(KernelService).Assembly.GetTypes())
                {
                    foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        KernelFunctionAttribute customAttribute = (KernelFunctionAttribute)Attribute.GetCustomAttribute((MemberInfo)method, typeof(KernelFunctionAttribute));
                        if (customAttribute != null)
                            this.LuaRuntime.RegisterMethod(customAttribute.Name, method, true);
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Exception[] loaderExceptions = ex.LoaderExceptions;
                LogHelper.Instance.Log("There was a ReflectionTypeLoadException in ConfigureKernelFunctions", (Exception)ex);
                foreach (Exception e in loaderExceptions)
                    LogHelper.Instance.Log("Loader Exception: ", e);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("There was an unknown exception in ConfigureKernelFunctions", ex);
            }
        }

        internal LuaVirtualMachine LuaRuntime { get; set; }

        internal LuaDebug? DebugState { get; set; }

        private KernelService()
        {
        }
    }
}
