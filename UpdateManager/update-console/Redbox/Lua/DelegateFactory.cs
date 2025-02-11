using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Redbox.Lua
{
    internal static class DelegateFactory
    {
        private static int m_luaClassNumber = 1;
        private static readonly ModuleBuilder m_newModule;
        private static readonly AssemblyName m_assemblyName;
        private static readonly AssemblyBuilder m_newAssembly;
        private static readonly IDictionary<Type, Type> m_delegateCollection = (IDictionary<Type, Type>)new Dictionary<Type, Type>();

        public static Delegate GetDelegate(Type delegateType, LuaFunction luaFunc)
        {
            List<Type> typeList = new List<Type>();
            Type type;
            if (DelegateFactory.m_delegateCollection.ContainsKey(delegateType))
            {
                type = DelegateFactory.m_delegateCollection[delegateType];
            }
            else
            {
                type = DelegateFactory.GenerateDelegate(delegateType);
                DelegateFactory.m_delegateCollection[delegateType] = type;
            }
            MethodInfo method = delegateType.GetMethod("Invoke");
            typeList.Add(method.ReturnType);
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                if (parameter.ParameterType.IsByRef)
                    typeList.Add(parameter.ParameterType);
            }
            LuaDelegate instance = (LuaDelegate)Activator.CreateInstance(type);
            instance.m_function = luaFunc;
            instance.m_returnTypes = typeList.ToArray();
            return Delegate.CreateDelegate(delegateType, (object)instance, "CallFunction");
        }

        static DelegateFactory()
        {
            DelegateFactory.m_assemblyName = new AssemblyName()
            {
                Name = "Redbox.Lua.GeneratedCode"
            };
            DelegateFactory.m_newAssembly = Thread.GetDomain().DefineDynamicAssembly(DelegateFactory.m_assemblyName, AssemblyBuilderAccess.Run);
            DelegateFactory.m_newModule = DelegateFactory.m_newAssembly.DefineDynamicModule("Redbox.Lua.GeneratedCode.Module");
        }

        private static Type GenerateDelegate(Type delegateType)
        {
            string name;
            lock (typeof(DelegateFactory))
            {
                name = "__DelegateWrapper_" + (object)DelegateFactory.m_luaClassNumber;
                ++DelegateFactory.m_luaClassNumber;
            }
            TypeBuilder typeBuilder = DelegateFactory.m_newModule.DefineType(name, TypeAttributes.Public, typeof(LuaDelegate));
            MethodInfo method1 = delegateType.GetMethod("Invoke");
            ParameterInfo[] parameters = method1.GetParameters();
            Type[] parameterTypes = new Type[parameters.Length];
            Type returnType = method1.ReturnType;
            int num1 = 0;
            int length = 0;
            for (int index = 0; index < parameterTypes.Length; ++index)
            {
                parameterTypes[index] = parameters[index].ParameterType;
                if (!parameters[index].IsIn && parameters[index].IsOut)
                    ++num1;
                if (parameterTypes[index].IsByRef)
                    ++length;
            }
            int[] numArray = new int[length];
            ILGenerator ilGenerator = typeBuilder.DefineMethod("CallFunction", method1.Attributes, returnType, parameterTypes).GetILGenerator();
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
            int index1 = 0;
            int num2 = 0;
            int index2 = 0;
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
                        ilGenerator.Emit(OpCodes.Ldind_Ref);
                }
                else if (parameterTypes[index1].IsValueType)
                    ilGenerator.Emit(OpCodes.Box, parameterTypes[index1]);
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
                            ilGenerator.Emit(OpCodes.Ldind_Ref);
                    }
                    else if (parameterTypes[index1].IsValueType)
                        ilGenerator.Emit(OpCodes.Box, parameterTypes[index1]);
                    ilGenerator.Emit(OpCodes.Stelem_Ref);
                    ++num2;
                }
            }
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldloc_0);
            ilGenerator.Emit(OpCodes.Ldloc_1);
            ilGenerator.Emit(OpCodes.Ldloc_2);
            MethodInfo method2 = typeof(LuaDelegate).GetMethod("CallFunction");
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
                ilGenerator.Emit(OpCodes.Castclass, returnType);
            ilGenerator.Emit(OpCodes.Stloc_3);
            for (int index3 = 0; index3 < numArray.Length; ++index3)
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
