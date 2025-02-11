using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Redbox.Lua
{
    internal class LuaStack
    {
        public EventHandler CountChange;
        private readonly LuaVirtualMachine m_virtualMachine;
        private const string RedboxExceptionName = "(redboxException)";
        private Stack<LuaScriptException> _redboxExceptionList = new Stack<LuaScriptException>();

        public LuaStack(LuaVirtualMachine virtualMachine) => this.m_virtualMachine = virtualMachine;

        public int Count => LuaVirtualMachine.lua_gettop((IntPtr)this.m_virtualMachine);

        public object Top => this.GetAt(-1);

        public object Pop()
        {
            object top = this.Top;
            LuaVirtualMachine.lua_pop((IntPtr)this.m_virtualMachine, 1);
            this.OnCountChange();
            return top;
        }

        public ReadOnlyCollection<object> PopTo(int oldTop)
        {
            List<object> objectList = new List<object>();
            if (oldTop != this.Count)
            {
                for (int index = oldTop + 1; index <= this.Count; ++index)
                    objectList.Add(this.GetAt(index));
                this.SetTop(oldTop);
                this.OnCountChange();
            }
            return objectList.AsReadOnly();
        }

        public void Drop(int count)
        {
            if (count == 0)
                return;
            LuaVirtualMachine.lua_pop((IntPtr)this.m_virtualMachine, count);
            this.OnCountChange();
        }

        public void GetReference(int reference)
        {
            LuaVirtualMachine.lua_rawgeti((IntPtr)this.m_virtualMachine, -10000, reference);
            this.OnCountChange();
        }

        public void NewTable()
        {
            LuaVirtualMachine.lua_newtable((IntPtr)this.m_virtualMachine);
            this.OnCountChange();
        }

        public void NewMetaTable(string name)
        {
            LuaVirtualMachine.luaL_newmetatable((IntPtr)this.m_virtualMachine, name);
            this.OnCountChange();
        }

        public IntPtr NewUserData(int size)
        {
            return LuaVirtualMachine.lua_newuserdata((IntPtr)this.m_virtualMachine, (IntPtr)size);
        }

        public IntPtr ToUserData(int index)
        {
            return LuaVirtualMachine.lua_touserdata((IntPtr)this.m_virtualMachine, index);
        }

        public void GetTable() => this.GetTable(-2);

        public void GetTable(int index)
        {
            LuaVirtualMachine.lua_gettable((IntPtr)this.m_virtualMachine, index);
            this.OnCountChange();
        }

        public void SetTable() => this.SetTable(-3);

        public void SetTable(int index)
        {
            LuaVirtualMachine.lua_settable((IntPtr)this.m_virtualMachine, index);
            this.OnCountChange();
        }

        public void SetTop(int index)
        {
            Trace.Assert(this.IsValidStackIndex(index), "not a valid stack index");
            LuaVirtualMachine.lua_settop((IntPtr)this.m_virtualMachine, index);
            this.OnCountChange();
        }

        public void GetGlobal(string name)
        {
            LuaVirtualMachine.lua_getglobal((IntPtr)this.m_virtualMachine, name);
            this.OnCountChange();
        }

        public void SetGlobal(string name)
        {
            LuaVirtualMachine.lua_setglobal((IntPtr)this.m_virtualMachine, name);
            this.OnCountChange();
        }

        public void GetField(int index, string name)
        {
            LuaVirtualMachine.lua_getfield((IntPtr)this.m_virtualMachine, index, name);
            this.OnCountChange();
        }

        public void SetField(int index, string name)
        {
            LuaVirtualMachine.lua_setfield((IntPtr)this.m_virtualMachine, index, name);
            this.OnCountChange();
        }

        public void SetMetaTable(int index)
        {
            LuaVirtualMachine.lua_setmetatable((IntPtr)this.m_virtualMachine, index);
            this.OnCountChange();
        }

        public int NextKey()
        {
            int num = LuaVirtualMachine.lua_next((IntPtr)this.m_virtualMachine, -2);
            this.OnCountChange();
            return num;
        }

        public int ProtectedCall(int nargs, int nresults, int errfunc)
        {
            return LuaVirtualMachine.lua_pcall((IntPtr)this.m_virtualMachine, nargs, nresults, errfunc);
        }

        public bool Check(int depth)
        {
            return LuaVirtualMachine.lua_checkstack((IntPtr)this.m_virtualMachine, depth) != 0;
        }

        public void Remove(int index)
        {
            LuaVirtualMachine.lua_remove((IntPtr)this.m_virtualMachine, index);
        }

        public void GetPath(string[] path)
        {
            for (int index = 0; index < path.Length; ++index)
            {
                this.Push((object)path[index]);
                this.GetTable();
                if (this.Top == null)
                    break;
            }
        }

        public void SetPath(string[] path, object value)
        {
            for (int index = 0; index < path.Length - 1; ++index)
            {
                this.Push((object)path[index]);
                this.GetTable(-2);
            }
            string[] strArray = path;
            this.Push((object)strArray[strArray.Length - 1]);
            this.Push(value);
            this.SetTable(-3);
        }

        public object GetAt(int index)
        {
            if (index < 0)
                index = this.Count + index + 1;
            Trace.Assert(this.IsValidStackIndex(index), "not a valid stack index");
            switch (LuaVirtualMachine.lua_type((IntPtr)this.m_virtualMachine, index))
            {
                case LuaTypes.None:
                case LuaTypes.Nil:
                    return (object)null;
                case LuaTypes.Boolean:
                    return (object)LuaVirtualMachine.lua_toboolean((IntPtr)this.m_virtualMachine, index);
                case LuaTypes.LightUserData:
                case LuaTypes.UserData:
                    LuaVirtualMachine.lua_pushvalue((IntPtr)this.m_virtualMachine, index);
                    return (object)new LuaUserData(this.m_virtualMachine, LuaVirtualMachine.luaL_ref((IntPtr)this.m_virtualMachine, -10000));
                case LuaTypes.Number:
                    return (object)LuaVirtualMachine.lua_tonumber((IntPtr)this.m_virtualMachine, index);
                case LuaTypes.String:
                    string at1 = LuaVirtualMachine.lua_tostring((IntPtr)this.m_virtualMachine, index);
                    if (string.IsNullOrEmpty(at1))
                        return (object)at1;
                    if (at1 == "(redboxException)")
                    {
                        LuaScriptException at2 = this._redboxExceptionList.Pop();
                        if (at2 != null)
                            return (object)at2;
                    }
                    return (object)at1;
                case LuaTypes.Table:
                    LuaVirtualMachine.lua_pushvalue((IntPtr)this.m_virtualMachine, index);
                    return (object)new LuaTable(this.m_virtualMachine, LuaVirtualMachine.luaL_ref((IntPtr)this.m_virtualMachine, -10000));
                case LuaTypes.Function:
                    LuaVirtualMachine.lua_pushvalue((IntPtr)this.m_virtualMachine, index);
                    return (object)new LuaFunction(this.m_virtualMachine, LuaVirtualMachine.luaL_ref((IntPtr)this.m_virtualMachine, -10000));
                default:
                    return (object)null;
            }
        }

        public void Push(object value)
        {
            switch (value)
            {
                case null:
                    LuaVirtualMachine.lua_pushnil((IntPtr)this.m_virtualMachine);
                    break;
                case bool _:
                    LuaVirtualMachine.lua_pushboolean((IntPtr)this.m_virtualMachine, Convert.ToBoolean(value));
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
                case Decimal _:
                case double _:
                case char _:
                    LuaVirtualMachine.lua_pushnumber((IntPtr)this.m_virtualMachine, Convert.ToDouble(value));
                    break;
                case string _:
                    LuaVirtualMachine.lua_pushstring((IntPtr)this.m_virtualMachine, (string)value);
                    break;
                case LuaObject _:
                    ((LuaObject)value).Push();
                    break;
                case LuaScriptException _:
                    this._redboxExceptionList.Push((LuaScriptException)value);
                    LuaVirtualMachine.lua_pushstring((IntPtr)this.m_virtualMachine, "(redboxException)");
                    break;
            }
            this.OnCountChange();
        }

        public void PushClosure(LuaCFunctionDelegate function)
        {
            LuaVirtualMachine.lua_pushcclosure((IntPtr)this.m_virtualMachine, function, 0);
        }

        protected void OnCountChange()
        {
            if (this.CountChange == null)
                return;
            this.CountChange((object)this, EventArgs.Empty);
        }

        private bool IsValidStackIndex(int index) => index >= 0 && index <= this.Count;
    }
}
