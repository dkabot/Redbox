using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using Redbox.Core;

namespace Redbox.Lua
{
    public class RegisteredMethod
    {
        private readonly IDictionary<MethodInfo, CompiledMethodHandler> m_compiledMethodHandlers =
            new Dictionary<MethodInfo, CompiledMethodHandler>();

        public RegisteredMethod(LuaVirtualMachine virtualMachine, MethodInfo methodInfo)
        {
            VirtualMachineInstance = virtualMachine;
            Method = methodInfo;
            MethodOverloads = FindMethodOverloads(methodInfo);
        }

        public MethodInfo Method { get; }

        public List<MethodInfo> MethodOverloads { get; }

        public LuaVirtualMachine VirtualMachineInstance { get; }

        public object Invoke(ReadOnlyCollection<object> parms, out int itemsOnStack)
        {
            itemsOnStack = 0;
            try
            {
                var bestMethodOverload = FindBestMethodOverload(parms);
                return InvokeMethod(bestMethodOverload, ConvertParametersForMethodInfo(bestMethodOverload, parms));
            }
            catch (TargetInvocationException ex)
            {
                itemsOnStack = VirtualMachineInstance.SetPendingException(ex.GetBaseException());
            }
            catch (Exception ex)
            {
                itemsOnStack = VirtualMachineInstance.SetPendingException(ex);
            }

            return null;
        }

        internal void Compile()
        {
            Compile(Method);
            foreach (var methodOverload in MethodOverloads)
                Compile(methodOverload);
        }

        internal void Compile(MethodInfo methodInfo)
        {
            if (m_compiledMethodHandlers.ContainsKey(methodInfo))
                return;
            var compiledMethodHandler = new CompiledMethodHandler();
            if (compiledMethodHandler.DynamicMethod == null)
            {
                compiledMethodHandler.DynamicMethod = new DynamicMethod(string.Format("___{0}", methodInfo.Name),
                    typeof(object), new Type[1]
                    {
                        typeof(object[])
                    }, methodInfo.DeclaringType, true);
                var ilGenerator = compiledMethodHandler.DynamicMethod.GetILGenerator();
                var parameters = methodInfo.GetParameters();
                for (var index = 0; index < parameters.Length; ++index)
                {
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    ilGenerator.Emit(OpCodes.Ldc_I4, index);
                    ilGenerator.Emit(OpCodes.Ldelem_Ref);
                    var parameterType = parameters[index].ParameterType;
                    if (parameterType.IsValueType)
                        ilGenerator.Emit(OpCodes.Unbox_Any, parameterType);
                }

                if (methodInfo.IsVirtual)
                    ilGenerator.Emit(OpCodes.Callvirt, methodInfo);
                else
                    ilGenerator.Emit(OpCodes.Call, methodInfo);
                if (methodInfo.ReturnType != typeof(void))
                {
                    if (methodInfo.ReturnType.IsValueType)
                        ilGenerator.Emit(OpCodes.Box, methodInfo.ReturnType);
                }
                else
                {
                    ilGenerator.Emit(OpCodes.Ldnull);
                }

                ilGenerator.Emit(OpCodes.Ret);
            }

            compiledMethodHandler.MethodHandler =
                (RegisteredMethodHandler)compiledMethodHandler.DynamicMethod.CreateDelegate(
                    typeof(RegisteredMethodHandler));
            m_compiledMethodHandlers[methodInfo] = compiledMethodHandler;
        }

        internal object InvokeMethod(MethodInfo methodInfo, object[] values)
        {
            return !m_compiledMethodHandlers.ContainsKey(methodInfo)
                ? methodInfo.Invoke(null, values)
                : m_compiledMethodHandlers[methodInfo].MethodHandler(values);
        }

        private static List<MethodInfo> FindMethodOverloads(MethodInfo targetMethodInfo)
        {
            var methodOverloads = new List<MethodInfo>();
            foreach (var method in targetMethodInfo.DeclaringType.GetMethods(BindingFlags.Static | BindingFlags.Public |
                                                                             BindingFlags.NonPublic))
                if (string.Compare(method.Name, targetMethodInfo.Name, false) == 0 && method != targetMethodInfo)
                    methodOverloads.Add(method);
            return methodOverloads;
        }

        private static object[] ConvertParametersForMethodInfo(
            MethodInfo methodInfo,
            IEnumerable<object> parms)
        {
            var parameters = methodInfo.GetParameters();
            var objArray = new object[parameters.Length];
            var index = 0;
            foreach (var parm in parms)
            {
                objArray[index] =
                    !typeof(Delegate).IsAssignableFrom(parameters[index].ParameterType) || !(parm is LuaFunction)
                        ? ConversionHelper.ChangeType(parm, parameters[index].ParameterType)
                        : DelegateFactory.GetDelegate(parameters[index].ParameterType, (LuaFunction)parm);
                ++index;
                if (index >= parameters.Length)
                    break;
            }

            return objArray;
        }

        private MethodInfo FindBestMethodOverload(ReadOnlyCollection<object> parms)
        {
            if (MethodOverloads.Count == 0)
                return Method;
            var scoredMethodInfoList = new List<ScoredMethodInfo>();
            foreach (var methodOverload in MethodOverloads)
            {
                var parameters = methodOverload.GetParameters();
                if (parms.Count == parameters.Length)
                {
                    var num = 0;
                    for (var index = 0; index < parameters.Length; ++index)
                    {
                        var type = parms[index].GetType();
                        if (type == typeof(double))
                        {
                            if (parameters[index].ParameterType == typeof(double))
                                num += 5;
                            else if (parameters[index].ParameterType == typeof(decimal))
                                num += 4;
                            else if (parameters[index].ParameterType == typeof(float))
                                num += 3;
                            else if (parameters[index].ParameterType == typeof(long))
                                num += 2;
                            else if (parameters[index].ParameterType == typeof(int) ||
                                     parameters[index].ParameterType == typeof(ushort) ||
                                     parameters[index].ParameterType == typeof(short) ||
                                     parameters[index].ParameterType == typeof(byte))
                                ++num;
                        }
                        else if (type == typeof(bool) && parameters[index].ParameterType == typeof(bool))
                        {
                            ++num;
                        }
                        else if (type == typeof(string) && parameters[index].ParameterType == typeof(string))
                        {
                            ++num;
                        }
                        else if (type == typeof(LuaFunction) &&
                                 typeof(Delegate).IsAssignableFrom(parameters[index].ParameterType))
                        {
                            ++num;
                        }
                        else if (type == typeof(LuaObject) && parameters[index].ParameterType == typeof(LuaObject))
                        {
                            ++num;
                        }
                    }

                    if (num > 0)
                        scoredMethodInfoList.Add(new ScoredMethodInfo
                        {
                            Score = num,
                            MethodInfo = methodOverload
                        });
                }
            }

            scoredMethodInfoList.Sort((x, y) => y.Score.CompareTo(x.Score));
            return scoredMethodInfoList.Count <= 0 ? Method : scoredMethodInfoList[0].MethodInfo;
        }

        internal sealed class CompiledMethodHandler
        {
            public DynamicMethod DynamicMethod;
            public RegisteredMethodHandler MethodHandler;
        }

        private sealed class ScoredMethodInfo
        {
            public MethodInfo MethodInfo;
            public int Score;
        }
    }
}