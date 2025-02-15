using System;
using Redbox.Core;

namespace Redbox.Lua
{
    public class LuaDelegate
    {
        internal LuaFunction m_function;
        internal Type[] m_returnTypes;

        public LuaDelegate()
        {
            m_function = null;
            m_returnTypes = null;
        }

        public object CallFunction(object[] args, object[] inArgs, int[] outArgs)
        {
            var readOnlyCollection = m_function.Call(inArgs);
            object obj;
            int index1;
            if (m_returnTypes[0] == typeof(void))
            {
                obj = null;
                index1 = 0;
            }
            else
            {
                obj = readOnlyCollection[0];
                index1 = 1;
            }

            for (var index2 = 0; index2 < outArgs.Length; ++index2)
            {
                args[outArgs[index2]] = ConversionHelper.ChangeType(readOnlyCollection[index1], m_returnTypes[index2]);
                ++index1;
            }

            return obj;
        }
    }
}