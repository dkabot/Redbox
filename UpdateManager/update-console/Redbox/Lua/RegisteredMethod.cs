using Redbox.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;

namespace Redbox.Lua
{
    internal class RegisteredMethod
    {
        private readonly IDictionary<MethodInfo, RegisteredMethod.CompiledMethodHandler> m_compiledMethodHandlers = (IDictionary<MethodInfo, RegisteredMethod.CompiledMethodHandler>)new Dictionary<MethodInfo, RegisteredMethod.CompiledMethodHandler>();

        public RegisteredMethod(LuaVirtualMachine virtualMachine, MethodInfo methodInfo)
        {
            this.VirtualMachineInstance = virtualMachine;
            this.Method = methodInfo;
            this.MethodOverloads = RegisteredMethod.FindMethodOverloads(methodInfo);
        }

        public object Invoke(ReadOnlyCollection<object> parms, out int itemsOnStack)
        {
            itemsOnStack = 0;
            try
            {
                MethodInfo bestMethodOverload = this.FindBestMethodOverload(parms);
                return this.InvokeMethod(bestMethodOverload, RegisteredMethod.ConvertParametersForMethodInfo(bestMethodOverload, (IEnumerable<object>)parms));
            }
            catch (TargetInvocationException ex)
            {
                itemsOnStack = this.VirtualMachineInstance.SetPendingException(ex.GetBaseException());
            }
            catch (Exception ex)
            {
                itemsOnStack = this.VirtualMachineInstance.SetPendingException(ex);
            }
            return (object)null;
        }

        public MethodInfo Method { get; private set; }

        public List<MethodInfo> MethodOverloads { get; private set; }

        public LuaVirtualMachine VirtualMachineInstance { get; private set; }

        internal void Compile()
        {
            this.Compile(this.Method);
            foreach (MethodInfo methodOverload in this.MethodOverloads)
                this.Compile(methodOverload);
        }

        internal void Compile(MethodInfo methodInfo)
        {
            if (this.m_compiledMethodHandlers.ContainsKey(methodInfo))
                return;
            RegisteredMethod.CompiledMethodHandler compiledMethodHandler1 = new RegisteredMethod.CompiledMethodHandler();
            if ((MethodInfo)compiledMethodHandler1.DynamicMethod == (MethodInfo)null)
            {
                compiledMethodHandler1.DynamicMethod = new DynamicMethod(string.Format("___{0}", (object)methodInfo.Name), typeof(object), new Type[1]
                {
          typeof (object[])
                }, methodInfo.DeclaringType, true);
                ILGenerator ilGenerator = compiledMethodHandler1.DynamicMethod.GetILGenerator();
                ParameterInfo[] parameters = methodInfo.GetParameters();
                for (int index = 0; index < parameters.Length; ++index)
                {
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    ilGenerator.Emit(OpCodes.Ldc_I4, index);
                    ilGenerator.Emit(OpCodes.Ldelem_Ref);
                    Type parameterType = parameters[index].ParameterType;
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
                    ilGenerator.Emit(OpCodes.Ldnull);
                ilGenerator.Emit(OpCodes.Ret);
            }
            RegisteredMethod.CompiledMethodHandler compiledMethodHandler2 = compiledMethodHandler1;
            compiledMethodHandler2.MethodHandler = (RegisteredMethodHandler)compiledMethodHandler2.DynamicMethod.CreateDelegate(typeof(RegisteredMethodHandler));
            this.m_compiledMethodHandlers[methodInfo] = compiledMethodHandler1;
        }

        internal object InvokeMethod(MethodInfo methodInfo, object[] values)
        {
            return !this.m_compiledMethodHandlers.ContainsKey(methodInfo) ? methodInfo.Invoke((object)null, values) : this.m_compiledMethodHandlers[methodInfo].MethodHandler(values);
        }

        private static List<MethodInfo> FindMethodOverloads(MethodInfo targetMethodInfo)
        {
            List<MethodInfo> methodOverloads = new List<MethodInfo>();
            foreach (MethodInfo method in targetMethodInfo.DeclaringType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (string.Compare(method.Name, targetMethodInfo.Name, false) == 0 && method != targetMethodInfo)
                    methodOverloads.Add(method);
            }
            return methodOverloads;
        }

        private static object[] ConvertParametersForMethodInfo(
          MethodInfo methodInfo,
          IEnumerable<object> parms)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();
            object[] objArray = new object[parameters.Length];
            int index = 0;
            foreach (object parm in parms)
            {
                objArray[index] = !typeof(Delegate).IsAssignableFrom(parameters[index].ParameterType) || !(parm is LuaFunction) ? ConversionHelper.ChangeType(parm, parameters[index].ParameterType) : (object)DelegateFactory.GetDelegate(parameters[index].ParameterType, (LuaFunction)parm);
                ++index;
                if (index >= parameters.Length)
                    break;
            }
            return objArray;
        }

        private MethodInfo FindBestMethodOverload(ReadOnlyCollection<object> parms)
        {
            if (this.MethodOverloads.Count == 0)
                return this.Method;
            List<RegisteredMethod.ScoredMethodInfo> scoredMethodInfoList = new List<RegisteredMethod.ScoredMethodInfo>();
            foreach (MethodInfo methodOverload in this.MethodOverloads)
            {
                ParameterInfo[] parameters = methodOverload.GetParameters();
                if (parms.Count == parameters.Length)
                {
                    int num = 0;
                    for (int index = 0; index < parameters.Length; ++index)
                    {
                        Type type = parms[index].GetType();
                        if (type == typeof(double))
                        {
                            if (parameters[index].ParameterType == typeof(double))
                                num += 5;
                            else if (parameters[index].ParameterType == typeof(Decimal))
                                num += 4;
                            else if (parameters[index].ParameterType == typeof(float))
                                num += 3;
                            else if (parameters[index].ParameterType == typeof(long))
                                num += 2;
                            else if (parameters[index].ParameterType == typeof(int) || parameters[index].ParameterType == typeof(ushort) || parameters[index].ParameterType == typeof(short) || parameters[index].ParameterType == typeof(byte))
                                ++num;
                        }
                        else if (type == typeof(bool) && parameters[index].ParameterType == typeof(bool))
                            ++num;
                        else if (type == typeof(string) && parameters[index].ParameterType == typeof(string))
                            ++num;
                        else if (type == typeof(LuaFunction) && typeof(Delegate).IsAssignableFrom(parameters[index].ParameterType))
                            ++num;
                        else if (type == typeof(LuaObject) && parameters[index].ParameterType == typeof(LuaObject))
                            ++num;
                    }
                    if (num > 0)
                        scoredMethodInfoList.Add(new RegisteredMethod.ScoredMethodInfo()
                        {
                            Score = num,
                            MethodInfo = methodOverload
                        });
                }
            }
            scoredMethodInfoList.Sort((Comparison<RegisteredMethod.ScoredMethodInfo>)((x, y) => y.Score.CompareTo(x.Score)));
            return scoredMethodInfoList.Count <= 0 ? this.Method : scoredMethodInfoList[0].MethodInfo;
        }

        internal sealed class CompiledMethodHandler
        {
            public DynamicMethod DynamicMethod;
            public RegisteredMethodHandler MethodHandler;
        }

        private sealed class ScoredMethodInfo
        {
            public int Score;
            public MethodInfo MethodInfo;
        }
    }
}
