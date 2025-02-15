using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.InteropServices;
using Redbox.Core;

namespace Redbox.Lua
{
    public class LuaVirtualMachine : IDisposable
    {
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
        private readonly object _lock = new object();
        private readonly Stack<int> _removeReferenceList = new Stack<int>();
        private readonly LuaCFunctionDelegate m_collectObjectCallback;
        private readonly LuaCFunctionDelegate m_executeFunctionCallback;
        private readonly ObjectCache m_objectCache = new ObjectCache();
        private readonly LuaCFunctionDelegate m_panicCallback;
        private LuaHookFunctionDelegate m_hookCallback;
        private IntPtr m_luaState;

        public LuaVirtualMachine()
        {
            m_luaState = luaL_newstate();
            Stack = new LuaStack(this);
            m_panicCallback = PanicCallback;
            m_collectObjectCallback = CollectObject;
            m_executeFunctionCallback = ExecuteFunction;
            luaL_openlibs((IntPtr)this);
            if (IsAlreadyLoaded())
                throw new LuaException("Lua has already been loaded into the given state.");
            SetRedboxFlag();
            CreateFunctionMetaTable();
            CreateObjectTable();
            lua_atpanic((IntPtr)this, m_panicCallback);
            Compiler = new LuaCompiler(this);
        }

        public object this[string fullPath]
        {
            get
            {
                if (string.IsNullOrEmpty(fullPath))
                    return null;
                var count = Stack.Count;
                var sourceArray = fullPath.Split(".".ToCharArray());
                Stack.GetGlobal(sourceArray[0]);
                var obj = Stack.GetAt(-1);
                if (sourceArray.Length > 1)
                {
                    var strArray = new string[sourceArray.Length - 1];
                    Array.Copy(sourceArray, 1, strArray, 0, sourceArray.Length - 1);
                    Stack.GetPath(strArray);
                    obj = Stack.Top;
                }

                Stack.SetTop(count);
                return obj;
            }
            set
            {
                var count = Stack.Count;
                var sourceArray = fullPath.Split(".".ToCharArray());
                if (sourceArray.Length == 1)
                {
                    Stack.Push(value);
                    Stack.SetGlobal(fullPath);
                }
                else
                {
                    Stack.GetGlobal(sourceArray[0]);
                    var strArray = new string[sourceArray.Length - 1];
                    Array.Copy(sourceArray, 1, strArray, 0, sourceArray.Length - 1);
                    Stack.SetPath(strArray, value);
                }

                Stack.SetTop(count);
            }
        }

        public LuaStack Stack { get; }

        public bool IsExecuting { get; internal set; }

        public LuaCompiler Compiler { get; private set; }

        public void Dispose()
        {
            if (m_luaState == IntPtr.Zero)
                return;
            RemoveReferences();
            lua_close((IntPtr)this);
            m_luaState = IntPtr.Zero;
        }

        public static implicit operator IntPtr(LuaVirtualMachine virtualMachine)
        {
            return virtualMachine.m_luaState;
        }

        public ReadOnlyCollection<object> DoString(string chunk)
        {
            return LoadString(chunk, "__dostring__")?.Call();
        }

        public LuaFunction LoadString(string chunk, string name)
        {
            var count = Stack.Count;
            IsExecuting = true;
            try
            {
                if (luaL_loadbuffer((IntPtr)this, chunk, (uint)chunk.Length, name) != 0)
                    ThrowExceptionFromError(count);
            }
            finally
            {
                IsExecuting = false;
            }

            var luaFunction = (LuaFunction)Stack.Pop();
            Stack.SetTop(count);
            return luaFunction;
        }

        public LuaFunction LoadFile(string fileName)
        {
            var count = Stack.Count;
            IsExecuting = true;
            try
            {
                if (luaL_loadfile((IntPtr)this, fileName) != 0)
                    ThrowExceptionFromError(count);
            }
            finally
            {
                IsExecuting = false;
            }

            var luaFunction = (LuaFunction)Stack.Pop();
            Stack.SetTop(count);
            return luaFunction;
        }

        public RegisteredMethod RegisterMethod(
            string name,
            MethodInfo function,
            bool compileStaticCallSite,
            string namespacePath)
        {
            if (this[namespacePath] == null)
                this[namespacePath] = new LuaTable(this);
            return RegisterMethod(name, function, compileStaticCallSite);
        }

        public RegisteredMethod RegisterMethod(
            string name,
            MethodInfo function,
            bool compileStaticCallSite)
        {
            var instance = new RegisteredMethod(this, function);
            if (compileStaticCallSite)
                instance.Compile();
            Stack.Push(m_objectCache.CacheObject(instance));
            Stack.GetField(-10000, "kernel_objects_mt");
            Stack.SetMetaTable(-2);
            this[name] = Stack.Top;
            Stack.Pop();
            return instance;
        }

        public int GetInfo(string what, ref LuaDebug luaDebug)
        {
            return lua_getinfo((IntPtr)this, what, ref luaDebug);
        }

        public int SetDebugHook(LuaHookEventMasks mask, int count)
        {
            if (m_hookCallback != null)
                return -1;
            m_hookCallback = DebugHookCallback;
            return lua_sethook((IntPtr)this, Marshal.GetFunctionPointerForDelegate(m_hookCallback), (int)mask, count);
        }

        public int RemoveDebugHook()
        {
            m_hookCallback = null;
            return lua_sethook((IntPtr)this, IntPtr.Zero, 0, 0);
        }

        public LuaHookEventMasks GetHookMask()
        {
            return lua_gethookmask((IntPtr)this);
        }

        public int GetHookCount()
        {
            return lua_gethookcount((IntPtr)this);
        }

        public bool GetStack(int level, out LuaDebug luaDebug)
        {
            luaDebug = new LuaDebug();
            return lua_getstack((IntPtr)this, level, ref luaDebug) != 0;
        }

        public string GetLocal(LuaDebug luaDebug, int n)
        {
            return lua_getlocal((IntPtr)this, ref luaDebug, n);
        }

        public string SetLocal(LuaDebug luaDebug, int n)
        {
            return lua_setlocal((IntPtr)this, ref luaDebug, n);
        }

        public string GetUpValue(int funcindex, int n)
        {
            return lua_getupvalue((IntPtr)this, funcindex, n);
        }

        public string SetUpValue(int funcindex, int n)
        {
            return lua_setupvalue((IntPtr)this, funcindex, n);
        }

        public bool CompareReference(int ref1, int ref2)
        {
            return lua_compareref((IntPtr)this, ref1, ref2);
        }

        public event EventHandler<DebugHookEventArgs> DebugHook;

        public event EventHandler<HookExceptionEventArgs> HookException;

        public void RemoveReference(int reference)
        {
            lock (_lock)
            {
                _removeReferenceList.Push(reference);
            }
        }

        public void RemoveReferences()
        {
            lock (_lock)
            {
                if (!(m_luaState != IntPtr.Zero))
                    return;
                while (_removeReferenceList.Count > 0)
                    luaL_unref(m_luaState, -10000, _removeReferenceList.Pop());
            }
        }

        internal static string StringPointerToString(IntPtr ptr)
        {
            return Marshal.PtrToStringAnsi(ptr);
        }

        internal static string StringPointerToString(IntPtr ptr, int len)
        {
            return Marshal.PtrToStringAnsi(ptr, len);
        }

        internal static void lua_getglobal(IntPtr luaState, string name)
        {
            lua_getfield(luaState, -10002, name);
        }

        internal static void lua_pop(IntPtr luaState, int n)
        {
            lua_settop(luaState, -n - 1);
        }

        internal static void lua_setglobal(IntPtr luaState, string name)
        {
            lua_setfield(luaState, -10002, name);
        }

        internal static void lua_newtable(IntPtr luaState)
        {
            lua_createtable(luaState, 0, 0);
        }

        internal static string lua_tostring(IntPtr luaState, int index)
        {
            IntPtr len;
            return StringPointerToString(lua_tolstring(luaState, index, out len), len.ToInt32());
        }

        internal static bool lua_compareref(IntPtr luaState, int ref1, int ref2)
        {
            var newTop = lua_gettop(luaState);
            lua_rawgeti(luaState, -10000, ref1);
            lua_rawgeti(luaState, -10000, ref2);
            var num = lua_equal(luaState, -1, -2);
            lua_settop(luaState, newTop);
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
            var obj = Stack.Pop();
            Stack.SetTop(oldTop);
            if (obj is LuaScriptException luaScriptException)
                throw luaScriptException;
            obj = obj == null
                ? (object)"Unknown Lua Error"
                : throw new LuaScriptException(obj.ToString(), string.Empty);
        }

        internal int SetPendingException(Exception e)
        {
            if (e == null)
                return 0;
            ThrowError(e);
            return 1;
        }

        internal void ThrowError(object e)
        {
            var count = Stack.Count;
            luaL_where((IntPtr)this, 1);
            var readOnlyCollection = Stack.PopTo(count);
            var empty = string.Empty;
            if (readOnlyCollection.Count > 0)
                empty = readOnlyCollection[0].ToString();
            switch (e)
            {
                case string message:
                    e = new LuaScriptException(message, empty);
                    break;
                case Exception innerException:
                    e = new LuaScriptException(innerException, empty);
                    break;
            }

            Stack.Push(e);
            lua_error((IntPtr)this);
        }

        private static int PanicCallback(IntPtr luaState)
        {
            throw new LuaException(string.Format("unprotected error in call to Lua API ({0})",
                lua_tostring(luaState, -1)));
        }

        private void DebugHookCallback(IntPtr luaState, ref LuaDebug luaDebug)
        {
            try
            {
                if (DebugHook == null)
                    return;
                DebugHook(this, new DebugHookEventArgs(new LuaDebugInfo(luaDebug)));
            }
            catch (Exception ex)
            {
                OnHookException(new HookExceptionEventArgs(ex));
            }
        }

        private void OnHookException(HookExceptionEventArgs e)
        {
            var hookException = HookException;
            if (hookException == null)
                return;
            hookException(this, e);
        }

        private bool IsAlreadyLoaded()
        {
            Stack.Push("__redbox_lua_flag");
            Stack.GetTable(-10000);
            var obj = Stack.Pop();
            return obj != null && (bool)obj;
        }

        private void SetRedboxFlag()
        {
            Stack.Push("__redbox_lua_flag");
            Stack.Push(true);
            Stack.SetTable(-10000);
        }

        private void CreateFunctionMetaTable()
        {
            var count = Stack.Count;
            Stack.NewMetaTable("kernel_objects_mt");
            Stack.Push("__gc");
            Stack.PushClosure(m_collectObjectCallback);
            Stack.SetTable(-3);
            Stack.Push("__call");
            Stack.PushClosure(m_executeFunctionCallback);
            Stack.SetTable(-3);
            Stack.SetTop(count);
        }

        private void CreateObjectTable()
        {
            Stack.NewTable();
            Stack.Push("__mode");
            Stack.Push("v");
            Stack.SetTable(-3);
            Stack.Push("kernel_objects");
            Stack.SetTable(-10000);
        }

        private int CollectObject(IntPtr luaState)
        {
            using (var luaUserData = (LuaUserData)Stack.Pop())
            {
                var id = luaUserData.ReadInt32(0);
                LogHelper.Instance.Log(string.Format("Lua: CollectObject called from runtime for object {0}.", id),
                    LogEntryType.Debug);
                m_objectCache.RemoveId(id);
                return 0;
            }
        }

        private int ExecuteFunction(IntPtr luaState)
        {
            var int32 = Convert.ToInt32(Stack.GetAt(1));
            LogHelper.Instance.Log(string.Format("Lua: ExecuteFunction called from runtime for object {0}.", int32),
                LogEntryType.Debug);
            var registeredMethod = (RegisteredMethod)m_objectCache.GetObject(int32);
            if (registeredMethod != null)
            {
                LogHelper.Instance.Log(
                    string.Format("...Registered method '{0}' found in object cache; executing.",
                        registeredMethod.Method), LogEntryType.Debug);
                var parms = Stack.PopTo(1);
                var itemsOnStack = 0;
                var obj = registeredMethod.Invoke(parms, out itemsOnStack);
                if (itemsOnStack > 0)
                    return itemsOnStack;
                if (registeredMethod.Method.ReturnType != typeof(void))
                {
                    Stack.Push(obj);
                    return 1;
                }
            }

            RemoveReferences();
            return 0;
        }
    }
}