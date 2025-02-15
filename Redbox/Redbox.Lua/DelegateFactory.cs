using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Redbox.Lua
{
    public static class DelegateFactory
    {
        private static int m_luaClassNumber = 1;
        private static readonly ModuleBuilder m_newModule;
        private static readonly AssemblyName m_assemblyName;
        private static readonly AssemblyBuilder m_newAssembly;
        private static readonly IDictionary<Type, Type> m_delegateCollection = new Dictionary<Type, Type>();

        static DelegateFactory()
        {
            m_assemblyName = new AssemblyName
            {
                Name = "Redbox.Lua.GeneratedCode"
            };
            m_newAssembly = Thread.GetDomain().DefineDynamicAssembly(m_assemblyName, AssemblyBuilderAccess.Run);
            m_newModule = m_newAssembly.DefineDynamicModule("Redbox.Lua.GeneratedCode.Module");
        }

        public static Delegate GetDelegate(Type delegateType, LuaFunction luaFunc)
        {
            var typeList = new List<Type>();
            Type type;
            if (m_delegateCollection.ContainsKey(delegateType))
            {
                type = m_delegateCollection[delegateType];
            }
            else
            {
                type = GenerateDelegate(delegateType);
                m_delegateCollection[delegateType] = type;
            }

            var method = delegateType.GetMethod("Invoke");
            typeList.Add(method.ReturnType);
            foreach (var parameter in method.GetParameters())
                if (parameter.ParameterType.IsByRef)
                    typeList.Add(parameter.ParameterType);
            var instance = (LuaDelegate)Activator.CreateInstance(type);
            instance.m_function = luaFunc;
            instance.m_returnTypes = typeList.ToArray();
            return Delegate.CreateDelegate(delegateType, instance, "CallFunction");
        }

        private static Type GenerateDelegate(Type delegateType)
        {
            string name;
            lock (typeof(DelegateFactory))
            {
                name = "__DelegateWrapper_" + m_luaClassNumber;
                ++m_luaClassNumber;
            }

            var typeBuilder = m_newModule.DefineType(name, TypeAttributes.Public, typeof(LuaDelegate));
            var method1 = delegateType.GetMethod("Invoke");
            var parameters = method1.GetParameters();
            var parameterTypes = new Type[parameters.Length];
            var returnType = method1.ReturnType;
            var num1 = 0;
            var length = 0;
            for (var index = 0; index < parameterTypes.Length; ++index)
            {
                parameterTypes[index] = parameters[index].ParameterType;
                if (!parameters[index].IsIn && parameters[index].IsOut)
                    ++num1;
                if (parameterTypes[index].IsByRef)
                    ++length;
            }

            var numArray = new int[length];
            var ilGenerator = typeBuilder.DefineMethod("CallFunction", method1.Attributes, returnType, parameterTypes)
                .GetILGenerator();
            ilGenerator.DeclareLocal(typeof(object[]));
            ilGenerator.DeclareLocal(typeof(object[]));
            ilGenerator.DeclareLocal(typeof(int[]));
            if (!(returnType == typeof(void)))
                ilGenerator.DeclareLocal(returnType);
            else
                ilGenerator.DeclareLocal(typeof(object));
            ilGenerator.Emit(OpCodes.Ldc_I4, parameterTypes.Length);
            ilGenerator.Emit(OpCodes.Newarr, typeof(object));
            ilGenerator.Emit(OpCodes.Stloc_0);
            ilGenerator.Emit(OpCodes.Ldc_I4, parameterTypes.Length - num1);
            ilGenerator.Emit(OpCodes.Newarr, typeof(object));
            ilGenerator.Emit(OpCodes.Stloc_1);
            ilGenerator.Emit(OpCodes.Ldc_I4, length);
            ilGenerator.Emit(OpCodes.Newarr, typeof(int));
            ilGenerator.Emit(OpCodes.Stloc_2);
            var index1 = 0;
            var num2 = 0;
            var index2 = 0;
            for (; index1 < parameterTypes.Length; ++index1)
            {
                ilGenerator.Emit(OpCodes.Ldloc_0);
                ilGenerator.Emit(OpCodes.Ldc_I4, index1);
                ilGenerator.Emit(OpCodes.Ldarg, index1 + 1);
                if (parameterTypes[index1].IsByRef)
                {
                    if (parameterTypes[index1].GetElementType().IsValueType)
                    {
                        ilGenerator.Emit(OpCodes.Ldobj, parameterTypes[index1].GetElementType());
                        ilGenerator.Emit(OpCodes.Box, parameterTypes[index1].GetElementType());
                    }
                    else
                    {
                        ilGenerator.Emit(OpCodes.Ldind_Ref);
                    }
                }
                else if (parameterTypes[index1].IsValueType)
                {
                    ilGenerator.Emit(OpCodes.Box, parameterTypes[index1]);
                }

                ilGenerator.Emit(OpCodes.Stelem_Ref);
                if (parameterTypes[index1].IsByRef)
                {
                    ilGenerator.Emit(OpCodes.Ldloc_2);
                    ilGenerator.Emit(OpCodes.Ldc_I4, index2);
                    ilGenerator.Emit(OpCodes.Ldc_I4, index1);
                    ilGenerator.Emit(OpCodes.Stelem_I4);
                    numArray[index2] = index1;
                    ++index2;
                }

                if (parameters[index1].IsIn || !parameters[index1].IsOut)
                {
                    ilGenerator.Emit(OpCodes.Ldloc_1);
                    ilGenerator.Emit(OpCodes.Ldc_I4, num2);
                    ilGenerator.Emit(OpCodes.Ldarg, index1 + 1);
                    if (parameterTypes[index1].IsByRef)
                    {
                        if (parameterTypes[index1].GetElementType().IsValueType)
                        {
                            ilGenerator.Emit(OpCodes.Ldobj, parameterTypes[index1].GetElementType());
                            ilGenerator.Emit(OpCodes.Box, parameterTypes[index1].GetElementType());
                        }
                        else
                        {
                            ilGenerator.Emit(OpCodes.Ldind_Ref);
                        }
                    }
                    else if (parameterTypes[index1].IsValueType)
                    {
                        ilGenerator.Emit(OpCodes.Box, parameterTypes[index1]);
                    }

                    ilGenerator.Emit(OpCodes.Stelem_Ref);
                    ++num2;
                }
            }

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldloc_0);
            ilGenerator.Emit(OpCodes.Ldloc_1);
            ilGenerator.Emit(OpCodes.Ldloc_2);
            var method2 = typeof(LuaDelegate).GetMethod("CallFunction");
            ilGenerator.Emit(OpCodes.Call, method2);
            if (returnType == typeof(void))
            {
                ilGenerator.Emit(OpCodes.Pop);
                ilGenerator.Emit(OpCodes.Ldnull);
            }
            else if (returnType.IsValueType)
            {
                ilGenerator.Emit(OpCodes.Unbox, returnType);
                ilGenerator.Emit(OpCodes.Ldobj, returnType);
            }
            else
            {
                ilGenerator.Emit(OpCodes.Castclass, returnType);
            }

            ilGenerator.Emit(OpCodes.Stloc_3);
            for (var index3 = 0; index3 < numArray.Length; ++index3)
            {
                ilGenerator.Emit(OpCodes.Ldarg, numArray[index3] + 1);
                ilGenerator.Emit(OpCodes.Ldloc_0);
                ilGenerator.Emit(OpCodes.Ldc_I4, numArray[index3]);
                ilGenerator.Emit(OpCodes.Ldelem_Ref);
                if (parameterTypes[numArray[index3]].GetElementType().IsValueType)
                {
                    ilGenerator.Emit(OpCodes.Unbox, parameterTypes[numArray[index3]].GetElementType());
                    ilGenerator.Emit(OpCodes.Ldobj, parameterTypes[numArray[index3]].GetElementType());
                    ilGenerator.Emit(OpCodes.Stobj, parameterTypes[numArray[index3]].GetElementType());
                }
                else
                {
                    ilGenerator.Emit(OpCodes.Castclass, parameterTypes[numArray[index3]].GetElementType());
                    ilGenerator.Emit(OpCodes.Stind_Ref);
                }
            }

            if (!(returnType == typeof(void)))
                ilGenerator.Emit(OpCodes.Ldloc_3);
            ilGenerator.Emit(OpCodes.Ret);
            return typeBuilder.CreateType();
        }
    }
}