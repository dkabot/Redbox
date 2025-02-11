using Redbox.Core;
using System;
using System.Collections.ObjectModel;

namespace Redbox.Lua
{
    internal class LuaDelegate
    {
        internal Type[] m_returnTypes;
        internal LuaFunction m_function;

        public LuaDelegate()
        {
            this.m_function = (LuaFunction)null;
            this.m_returnTypes = (Type[])null;
        }

        public object CallFunction(object[] args, object[] inArgs, int[] outArgs)
        {
            ReadOnlyCollection<object> readOnlyCollection = this.m_function.Call(inArgs);
            object obj;
            int index1;
            if (this.m_returnTypes[0] == typeof(void))
            {
                obj = (object)null;
                index1 = 0;
            }
            else
            {
                obj = readOnlyCollection[0];
                index1 = 1;
            }
            for (int index2 = 0; index2 < outArgs.Length; ++index2)
            {
                args[outArgs[index2]] = ConversionHelper.ChangeType(readOnlyCollection[index1], this.m_returnTypes[index2]);
                ++index1;
            }
            return obj;
        }
    }
}
