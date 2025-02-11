using Redbox.Core;
using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Redbox.Lua
{
    internal class LuaVirtualMachine : IDisposable
    {
        private object _lock = new object();
        private System.Collections.Generic.Stack<int> _removeReferenceList = new System.Collections.Generic.Stack<int>();
        internal const int LuaErrRun = 2;
        internal const int LuaErrSyntax = 3;
        internal const int LuaErrMem = 4;
        internal const int LuaMultRet = -1;
        internal const int LuaRegistryIndex = -10000;
        internal const int LuaEnvironIndex = -10001;
        internal const int LuaGlobalsIndex = -10002;
        internal const int LuaIdSize = 60;
        internal const string LuaDllName = "lua51";
        private const string RedboxLuaFlag = "__redbox_lua_flag";
        private IntPtr m_luaState;
        private LuaHookFunctionDelegate m_hookCallback;
        private readonly LuaCFunctionDelegate m_panicCallback;
        private readonly LuaCFunctionDelegate m_collectObjectCallback;
        private readonly ObjectCache m_objectCache = new ObjectCache();
        private readonly LuaCFunctionDelegate m_executeFunctionCallback;

        public LuaVirtualMachine()
        {
            this.m_luaState = LuaVirtualMachine.luaL_newstate();
            this.Stack = new LuaStack(this);
            this.m_panicCallback = new LuaCFunctionDelegate(LuaVirtualMachine.PanicCallback);
            this.m_collectObjectCallback = new LuaCFunctionDelegate(this.CollectObject);
            this.m_executeFunctionCallback = new LuaCFunctionDelegate(this.ExecuteFunction);
            LuaVirtualMachine.luaL_openlibs((IntPtr)this);
            if (this.IsAlreadyLoaded())
                throw new LuaException("Lua has already been loaded into the given state.");
            this.SetRedboxFlag();
            this.CreateFunctionMetaTable();
            this.CreateObjectTable();
            LuaVirtualMachine.lua_atpanic((IntPtr)this, this.m_panicCallback);
            this.Compiler = new LuaCompiler(this);
        }

        public static implicit operator IntPtr(LuaVirtualMachine virtualMachine)
        {
            return virtualMachine.m_luaState;
        }

        public object this[string fullPath]
        {
            get
            {
                if (string.IsNullOrEmpty(fullPath))
                    return (object)null;
                int count = this.Stack.Count;
                string[] sourceArray = fullPath.Split(".".ToCharArray());
                this.Stack.GetGlobal(sourceArray[0]);
                object obj = this.Stack.GetAt(-1);
                if (sourceArray.Length > 1)
                {
                    string[] strArray = new string[sourceArray.Length - 1];
                    Array.Copy((Array)sourceArray, 1, (Array)strArray, 0, sourceArray.Length - 1);
                    this.Stack.GetPath(strArray);
                    obj = this.Stack.Top;
                }
                this.Stack.SetTop(count);
                return obj;
            }
            set
            {
                int count = this.Stack.Count;
                string[] sourceArray = fullPath.Split(".".ToCharArray());
                if (sourceArray.Length == 1)
                {
                    this.Stack.Push(value);
                    this.Stack.SetGlobal(fullPath);
                }
                else
                {
                    this.Stack.GetGlobal(sourceArray[0]);
                    string[] strArray = new string[sourceArray.Length - 1];
                    Array.Copy((Array)sourceArray, 1, (Array)strArray, 0, sourceArray.Length - 1);
                    this.Stack.SetPath(strArray, value);
                }
                this.Stack.SetTop(count);
            }
        }

        public void Dispose()
        {
            if (this.m_luaState == IntPtr.Zero)
                return;
            this.RemoveReferences();
            LuaVirtualMachine.lua_close((IntPtr)this);
            this.m_luaState = IntPtr.Zero;
        }

        public ReadOnlyCollection<object> DoString(string chunk)
        {
            return this.LoadString(chunk, "__dostring__")?.Call();
        }

        public LuaFunction LoadString(string chunk, string name)
        {
            int count = this.Stack.Count;
            this.IsExecuting = true;
            try
            {
                IntPtr luaState = (IntPtr)this;
                string buff = chunk;
                int length = buff.Length;
                string name1 = name;
                if (LuaVirtualMachine.luaL_loadbuffer(luaState, buff, (uint)length, name1) != 0)
                    this.ThrowExceptionFromError(count);
            }
            finally
            {
                this.IsExecuting = false;
            }
            LuaFunction luaFunction = (LuaFunction)this.Stack.Pop();
            this.Stack.SetTop(count);
            return luaFunction;
        }

        public LuaFunction LoadFile(string fileName)
        {
            int count = this.Stack.Count;
            this.IsExecuting = true;
            try
            {
                if (LuaVirtualMachine.luaL_loadfile((IntPtr)this, fileName) != 0)
                    this.ThrowExceptionFromError(count);
            }
            finally
            {
                this.IsExecuting = false;
            }
            LuaFunction luaFunction = (LuaFunction)this.Stack.Pop();
            this.Stack.SetTop(count);
            return luaFunction;
        }

        public RegisteredMethod RegisterMethod(
          string name,
          MethodInfo function,
          bool compileStaticCallSite,
          string namespacePath)
        {
            if (this[namespacePath] == null)
                this[namespacePath] = (object)new LuaTable(this);
            return this.RegisterMethod(name, function, compileStaticCallSite);
        }

        public RegisteredMethod RegisterMethod(
          string name,
          MethodInfo function,
          bool compileStaticCallSite)
        {
            RegisteredMethod instance = new RegisteredMethod(this, function);
            if (compileStaticCallSite)
                instance.Compile();
            this.Stack.Push((object)this.m_objectCache.CacheObject((object)instance));
            this.Stack.GetField(-10000, "kernel_objects_mt");
            this.Stack.SetMetaTable(-2);
            this[name] = this.Stack.Top;
            this.Stack.Pop();
            return instance;
        }

        public int GetInfo(string what, ref LuaDebug luaDebug)
        {
            return LuaVirtualMachine.lua_getinfo((IntPtr)this, what, ref luaDebug);
        }

        public int SetDebugHook(LuaHookEventMasks mask, int count)
        {
            if (this.m_hookCallback != null)
                return -1;
            this.m_hookCallback = new LuaHookFunctionDelegate(this.DebugHookCallback);
            return LuaVirtualMachine.lua_sethook((IntPtr)this, Marshal.GetFunctionPointerForDelegate<LuaHookFunctionDelegate>(this.m_hookCallback), (int)mask, count);
        }

        public int RemoveDebugHook()
        {
            this.m_hookCallback = (LuaHookFunctionDelegate)null;
            return LuaVirtualMachine.lua_sethook((IntPtr)this, IntPtr.Zero, 0, 0);
        }

        public LuaHookEventMasks GetHookMask() => LuaVirtualMachine.lua_gethookmask((IntPtr)this);

        public int GetHookCount() => LuaVirtualMachine.lua_gethookcount((IntPtr)this);

        public bool GetStack(int level, out LuaDebug luaDebug)
        {
            luaDebug = new LuaDebug();
            return LuaVirtualMachine.lua_getstack((IntPtr)this, level, ref luaDebug) != 0;
        }

        public string GetLocal(LuaDebug luaDebug, int n)
        {
            return LuaVirtualMachine.lua_getlocal((IntPtr)this, ref luaDebug, n);
        }

        public string SetLocal(LuaDebug luaDebug, int n)
        {
            return LuaVirtualMachine.lua_setlocal((IntPtr)this, ref luaDebug, n);
        }

        public string GetUpValue(int funcindex, int n)
        {
            return LuaVirtualMachine.lua_getupvalue((IntPtr)this, funcindex, n);
        }

        public string SetUpValue(int funcindex, int n)
        {
            return LuaVirtualMachine.lua_setupvalue((IntPtr)this, funcindex, n);
        }

        public bool CompareReference(int ref1, int ref2)
        {
            return LuaVirtualMachine.lua_compareref((IntPtr)this, ref1, ref2);
        }

        public LuaStack Stack { get; private set; }

        public bool IsExecuting { get; internal set; }

        public LuaCompiler Compiler { get; private set; }

        public event EventHandler<DebugHookEventArgs> DebugHook;

        public event EventHandler<HookExceptionEventArgs> HookException;

        public void RemoveReference(int reference)
        {
            lock (this._lock)
                this._removeReferenceList.Push(reference);
        }

        public void RemoveReferences()
        {
            lock (this._lock)
            {
                if (!(this.m_luaState != IntPtr.Zero))
                    return;
                while (this._removeReferenceList.Count > 0)
                    LuaVirtualMachine.luaL_unref(this.m_luaState, -10000, this._removeReferenceList.Pop());
            }
        }

        internal static string StringPointerToString(IntPtr ptr) => Marshal.PtrToStringAnsi(ptr);

        internal static string StringPointerToString(IntPtr ptr, int len)
        {
            return Marshal.PtrToStringAnsi(ptr, len);
        }

        internal static void lua_getglobal(IntPtr luaState, string name)
        {
            LuaVirtualMachine.lua_getfield(luaState, -10002, name);
        }

        internal static void lua_pop(IntPtr luaState, int n)
        {
            LuaVirtualMachine.lua_settop(luaState, -n - 1);
        }

        internal static void lua_setglobal(IntPtr luaState, string name)
        {
            LuaVirtualMachine.lua_setfield(luaState, -10002, name);
        }

        internal static void lua_newtable(IntPtr luaState)
        {
            LuaVirtualMachine.lua_createtable(luaState, 0, 0);
        }

        internal static string lua_tostring(IntPtr luaState, int index)
        {
            IntPtr len;
            return LuaVirtualMachine.StringPointerToString(LuaVirtualMachine.lua_tolstring(luaState, index, out len), len.ToInt32());
        }

        internal static bool lua_compareref(IntPtr luaState, int ref1, int ref2)
        {
            int newTop = LuaVirtualMachine.lua_gettop(luaState);
            LuaVirtualMachine.lua_rawgeti(luaState, -10000, ref1);
            LuaVirtualMachine.lua_rawgeti(luaState, -10000, ref2);
            int num = LuaVirtualMachine.lua_equal(luaState, -1, -2);
            LuaVirtualMachine.lua_settop(luaState, newTop);
            return (uint)num > 0U;
        }

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_atpanic(IntPtr luaState, LuaCFunctionDelegate panicf);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_close(IntPtr luaState);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_createtable(IntPtr luaState, int narr, int nrec);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_call(IntPtr luaState, int nargs, int nresults);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_error(IntPtr luaState);

        [DllImport("lua51", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_getfield(IntPtr luaState, int index, string k);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_gettable(IntPtr luaState, int index);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_gettop(IntPtr luaState);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_next(IntPtr luaState, int index);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_pcall(IntPtr luaState, int nargs, int nresults, int errfunc);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_pushboolean(IntPtr luaState, bool b);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_pushcclosure(IntPtr luaState, LuaCFunctionDelegate f, int n);

        [DllImport("lua51", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_pushlstring(IntPtr luaState, string s, IntPtr len);

        [DllImport("lua51", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_pushstring(IntPtr luaState, string s);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_pushnil(IntPtr luaState);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_pushnumber(IntPtr luaState, double n);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_pushvalue(IntPtr luaState, int index);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_settop(IntPtr luaState, int newTop);

        [DllImport("lua51", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_setfield(IntPtr luaState, int index, string k);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_settable(IntPtr luaState, int index);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool lua_toboolean(IntPtr luaState, int index);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr lua_tolstring(IntPtr luaState, int index, out IntPtr len);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern double lua_tonumber(IntPtr luaState, int index);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr lua_touserdata(IntPtr luaState, int index);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern LuaTypes lua_type(IntPtr luaState, int index);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr lua_typename(IntPtr luaState, LuaTypes type);

        [DllImport("lua51", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int luaL_loadbuffer(IntPtr luaState, string buff, uint sz, string name);

        [DllImport("lua51", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int luaL_loadfile(IntPtr luaState, string fileName);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr luaL_newstate();

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void luaL_openlibs(IntPtr luaState);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr lua_gethook(IntPtr luaState);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern LuaHookEventMasks lua_gethookmask(IntPtr luaState);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_gethookcount(IntPtr luaState);

        [DllImport("lua51", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_getinfo(IntPtr luaState, string what, ref LuaDebug luaDebug);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern string lua_getlocal(IntPtr luaState, ref LuaDebug luaDebug, int n);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern string lua_setlocal(IntPtr luaState, ref LuaDebug luaDebug, int n);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_getstack(IntPtr luaState, int level, ref LuaDebug luaDebug);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_sethook(IntPtr luaState, IntPtr f, int mask, int count);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int luaopen_base(IntPtr luaState);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int luaopen_math(IntPtr luaState);

        [DllImport("lua51", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern string lua_getupvalue(IntPtr luaState, int funcindex, int n);

        [DllImport("lua51", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern string lua_setupvalue(IntPtr luaState, int funcindex, int n);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_rawgeti(IntPtr luaState, int tableIndex, int index);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_rawseti(IntPtr luaState, int tableIndex, int index);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int luaL_ref(IntPtr luaState, int index);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void luaL_unref(IntPtr luaState, int index, int reference);

        [DllImport("lua51", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void luaL_newmetatable(IntPtr luaState, string name);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_setmetatable(IntPtr luaState, int index);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr lua_newuserdata(IntPtr luaState, IntPtr size);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void lua_remove(IntPtr luaState, int index);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_equal(IntPtr luaState, int index1, int index2);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lua_checkstack(IntPtr luaState, int extra);

        [DllImport("lua51", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int luaL_where(IntPtr luaState, int level);

        internal void ThrowExceptionFromError(int oldTop)
        {
            object obj = this.Stack.Pop();
            this.Stack.SetTop(oldTop);
            if (obj is LuaScriptException luaScriptException)
                throw luaScriptException;
            obj = obj == null ? (object)"Unknown Lua Error" : throw new LuaScriptException(obj.ToString(), string.Empty);
        }

        internal int SetPendingException(Exception e)
        {
            if (e == null)
                return 0;
            this.ThrowError((object)e);
            return 1;
        }

        internal void ThrowError(object e)
        {
            int count = this.Stack.Count;
            LuaVirtualMachine.luaL_where((IntPtr)this, 1);
            ReadOnlyCollection<object> readOnlyCollection = this.Stack.PopTo(count);
            string empty = string.Empty;
            if (readOnlyCollection.Count > 0)
                empty = readOnlyCollection[0].ToString();
            switch (e)
            {
                case string message:
                    e = (object)new LuaScriptException(message, empty);
                    break;
                case Exception innerException:
                    e = (object)new LuaScriptException(innerException, empty);
                    break;
            }
            this.Stack.Push(e);
            LuaVirtualMachine.lua_error((IntPtr)this);
        }

        private static int PanicCallback(IntPtr luaState)
        {
            throw new LuaException(string.Format("unprotected error in call to Lua API ({0})", (object)LuaVirtualMachine.lua_tostring(luaState, -1)));
        }

        private void DebugHookCallback(IntPtr luaState, ref LuaDebug luaDebug)
        {
            try
            {
                if (this.DebugHook == null)
                    return;
                this.DebugHook((object)this, new DebugHookEventArgs(new LuaDebugInfo(luaDebug)));
            }
            catch (Exception ex)
            {
                this.OnHookException(new HookExceptionEventArgs(ex));
            }
        }

        private void OnHookException(HookExceptionEventArgs e)
        {
            EventHandler<HookExceptionEventArgs> hookException = this.HookException;
            if (hookException == null)
                return;
            hookException((object)this, e);
        }

        private bool IsAlreadyLoaded()
        {
            this.Stack.Push((object)"__redbox_lua_flag");
            this.Stack.GetTable(-10000);
            object obj = this.Stack.Pop();
            return obj != null && (bool)obj;
        }

        private void SetRedboxFlag()
        {
            this.Stack.Push((object)"__redbox_lua_flag");
            this.Stack.Push((object)true);
            this.Stack.SetTable(-10000);
        }

        private void CreateFunctionMetaTable()
        {
            int count = this.Stack.Count;
            this.Stack.NewMetaTable("kernel_objects_mt");
            this.Stack.Push((object)"__gc");
            this.Stack.PushClosure(this.m_collectObjectCallback);
            this.Stack.SetTable(-3);
            this.Stack.Push((object)"__call");
            this.Stack.PushClosure(this.m_executeFunctionCallback);
            this.Stack.SetTable(-3);
            this.Stack.SetTop(count);
        }

        private void CreateObjectTable()
        {
            this.Stack.NewTable();
            this.Stack.Push((object)"__mode");
            this.Stack.Push((object)"v");
            this.Stack.SetTable(-3);
            this.Stack.Push((object)"kernel_objects");
            this.Stack.SetTable(-10000);
        }

        private int CollectObject(IntPtr luaState)
        {
            using (LuaUserData luaUserData = (LuaUserData)this.Stack.Pop())
            {
                int id = luaUserData.ReadInt32(0);
                LogHelper.Instance.Log(string.Format("Lua: CollectObject called from runtime for object {0}.", (object)id), LogEntryType.Debug);
                this.m_objectCache.RemoveId(id);
                return 0;
            }
        }

        private int ExecuteFunction(IntPtr luaState)
        {
            int int32 = Convert.ToInt32(this.Stack.GetAt(1));
            LogHelper.Instance.Log(string.Format("Lua: ExecuteFunction called from runtime for object {0}.", (object)int32), LogEntryType.Debug);
            RegisteredMethod registeredMethod = (RegisteredMethod)this.m_objectCache.GetObject(int32);
            if (registeredMethod != null)
            {
                LogHelper.Instance.Log(string.Format("...Registered method '{0}' found in object cache; executing.", (object)registeredMethod.Method), LogEntryType.Debug);
                ReadOnlyCollection<object> parms = this.Stack.PopTo(1);
                int itemsOnStack = 0;
                object obj = registeredMethod.Invoke(parms, out itemsOnStack);
                if (itemsOnStack > 0)
                    return itemsOnStack;
                if (registeredMethod.Method.ReturnType != typeof(void))
                {
                    this.Stack.Push(obj);
                    return 1;
                }
            }
            this.RemoveReferences();
            return 0;
        }
    }
}
