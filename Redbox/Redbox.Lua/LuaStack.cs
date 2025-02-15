using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Redbox.Lua
{
    public class LuaStack
    {
        private const string RedboxExceptionName = "(redboxException)";
        private readonly Stack<LuaScriptException> _redboxExceptionList = new Stack<LuaScriptException>();
        private readonly LuaVirtualMachine m_virtualMachine;
        public EventHandler CountChange;

        public LuaStack(LuaVirtualMachine virtualMachine)
        {
            m_virtualMachine = virtualMachine;
        }

        public int Count => LuaVirtualMachine.lua_gettop((IntPtr)m_virtualMachine);

        public object Top => GetAt(-1);

        public object Pop()
        {
            var top = Top;
            LuaVirtualMachine.lua_pop((IntPtr)m_virtualMachine, 1);
            OnCountChange();
            return top;
        }

        public ReadOnlyCollection<object> PopTo(int oldTop)
        {
            var objectList = new List<object>();
            if (oldTop != Count)
            {
                for (var index = oldTop + 1; index <= Count; ++index)
                    objectList.Add(GetAt(index));
                SetTop(oldTop);
                OnCountChange();
            }

            return objectList.AsReadOnly();
        }

        public void Drop(int count)
        {
            if (count == 0)
                return;
            LuaVirtualMachine.lua_pop((IntPtr)m_virtualMachine, count);
            OnCountChange();
        }

        public void GetReference(int reference)
        {
            LuaVirtualMachine.lua_rawgeti((IntPtr)m_virtualMachine, -10000, reference);
            OnCountChange();
        }

        public void NewTable()
        {
            LuaVirtualMachine.lua_newtable((IntPtr)m_virtualMachine);
            OnCountChange();
        }

        public void NewMetaTable(string name)
        {
            LuaVirtualMachine.luaL_newmetatable((IntPtr)m_virtualMachine, name);
            OnCountChange();
        }

        public IntPtr NewUserData(int size)
        {
            return LuaVirtualMachine.lua_newuserdata((IntPtr)m_virtualMachine, (IntPtr)size);
        }

        public IntPtr ToUserData(int index)
        {
            return LuaVirtualMachine.lua_touserdata((IntPtr)m_virtualMachine, index);
        }

        public void GetTable()
        {
            GetTable(-2);
        }

        public void GetTable(int index)
        {
            LuaVirtualMachine.lua_gettable((IntPtr)m_virtualMachine, index);
            OnCountChange();
        }

        public void SetTable()
        {
            SetTable(-3);
        }

        public void SetTable(int index)
        {
            LuaVirtualMachine.lua_settable((IntPtr)m_virtualMachine, index);
            OnCountChange();
        }

        public void SetTop(int index)
        {
            Trace.Assert(IsValidStackIndex(index), "not a valid stack index");
            LuaVirtualMachine.lua_settop((IntPtr)m_virtualMachine, index);
            OnCountChange();
        }

        public void GetGlobal(string name)
        {
            LuaVirtualMachine.lua_getglobal((IntPtr)m_virtualMachine, name);
            OnCountChange();
        }

        public void SetGlobal(string name)
        {
            LuaVirtualMachine.lua_setglobal((IntPtr)m_virtualMachine, name);
            OnCountChange();
        }

        public void GetField(int index, string name)
        {
            LuaVirtualMachine.lua_getfield((IntPtr)m_virtualMachine, index, name);
            OnCountChange();
        }

        public void SetField(int index, string name)
        {
            LuaVirtualMachine.lua_setfield((IntPtr)m_virtualMachine, index, name);
            OnCountChange();
        }

        public void SetMetaTable(int index)
        {
            LuaVirtualMachine.lua_setmetatable((IntPtr)m_virtualMachine, index);
            OnCountChange();
        }

        public int NextKey()
        {
            var num = LuaVirtualMachine.lua_next((IntPtr)m_virtualMachine, -2);
            OnCountChange();
            return num;
        }

        public int ProtectedCall(int nargs, int nresults, int errfunc)
        {
            return LuaVirtualMachine.lua_pcall((IntPtr)m_virtualMachine, nargs, nresults, errfunc);
        }

        public bool Check(int depth)
        {
            return LuaVirtualMachine.lua_checkstack((IntPtr)m_virtualMachine, depth) != 0;
        }

        public void Remove(int index)
        {
            LuaVirtualMachine.lua_remove((IntPtr)m_virtualMachine, index);
        }

        public void GetPath(string[] path)
        {
            for (var index = 0; index < path.Length; ++index)
            {
                Push(path[index]);
                GetTable();
                if (Top == null)
                    break;
            }
        }

        public void SetPath(string[] path, object value)
        {
            for (var index = 0; index < path.Length - 1; ++index)
            {
                Push(path[index]);
                GetTable(-2);
            }

            Push(path[path.Length - 1]);
            Push(value);
            SetTable(-3);
        }

        public object GetAt(int index)
        {
            if (index < 0)
                index = Count + index + 1;
            Trace.Assert(IsValidStackIndex(index), "not a valid stack index");
            switch (LuaVirtualMachine.lua_type((IntPtr)m_virtualMachine, index))
            {
                case LuaTypes.None:
                case LuaTypes.Nil:
                    return null;
                case LuaTypes.Boolean:
                    return LuaVirtualMachine.lua_toboolean((IntPtr)m_virtualMachine, index);
                case LuaTypes.LightUserData:
                case LuaTypes.UserData:
                    LuaVirtualMachine.lua_pushvalue((IntPtr)m_virtualMachine, index);
                    return new LuaUserData(m_virtualMachine,
                        LuaVirtualMachine.luaL_ref((IntPtr)m_virtualMachine, -10000));
                case LuaTypes.Number:
                    return LuaVirtualMachine.lua_tonumber((IntPtr)m_virtualMachine, index);
                case LuaTypes.String:
                    var at1 = LuaVirtualMachine.lua_tostring((IntPtr)m_virtualMachine, index);
                    if (string.IsNullOrEmpty(at1))
                        return at1;
                    if (at1 == "(redboxException)")
                    {
                        var at2 = _redboxExceptionList.Pop();
                        if (at2 != null)
                            return at2;
                    }

                    return at1;
                case LuaTypes.Table:
                    LuaVirtualMachine.lua_pushvalue((IntPtr)m_virtualMachine, index);
                    return new LuaTable(m_virtualMachine, LuaVirtualMachine.luaL_ref((IntPtr)m_virtualMachine, -10000));
                case LuaTypes.Function:
                    LuaVirtualMachine.lua_pushvalue((IntPtr)m_virtualMachine, index);
                    return new LuaFunction(m_virtualMachine,
                        LuaVirtualMachine.luaL_ref((IntPtr)m_virtualMachine, -10000));
                default:
                    return null;
            }
        }

        public void Push(object value)
        {
            switch (value)
            {
                case null:
                    LuaVirtualMachine.lua_pushnil((IntPtr)m_virtualMachine);
                    break;
                case bool _:
                    LuaVirtualMachine.lua_pushboolean((IntPtr)m_virtualMachine, Convert.ToBoolean(value));
                    break;
                case sbyte _:
                case byte _:
                case short _:
                case ushort _:
                case int _:
                case uint _:
                case long _:
                case float _:
                case ulong _:
                case decimal _:
                case double _:
                case char _:
                    LuaVirtualMachine.lua_pushnumber((IntPtr)m_virtualMachine, Convert.ToDouble(value));
                    break;
                case string _:
                    LuaVirtualMachine.lua_pushstring((IntPtr)m_virtualMachine, (string)value);
                    break;
                case LuaObject _:
                    ((LuaObject)value).Push();
                    break;
                case LuaScriptException _:
                    _redboxExceptionList.Push((LuaScriptException)value);
                    LuaVirtualMachine.lua_pushstring((IntPtr)m_virtualMachine, "(redboxException)");
                    break;
            }

            OnCountChange();
        }

        public void PushClosure(LuaCFunctionDelegate function)
        {
            LuaVirtualMachine.lua_pushcclosure((IntPtr)m_virtualMachine, function, 0);
        }

        protected void OnCountChange()
        {
            if (CountChange == null)
                return;
            CountChange(this, EventArgs.Empty);
        }

        private bool IsValidStackIndex(int index)
        {
            return index >= 0 && index <= Count;
        }
    }
}