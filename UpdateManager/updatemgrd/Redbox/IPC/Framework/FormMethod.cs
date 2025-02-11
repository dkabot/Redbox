using Redbox.Command.Tokenizer;
using Redbox.Core;
using Redbox.Tokenizer.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Redbox.IPC.Framework
{
    internal class FormMethod
    {
        private List<string> _filters = new List<string>();
        public readonly IDictionary<string, FormMethodParameter> ParameterCache = (IDictionary<string, FormMethodParameter>)new Dictionary<string, FormMethodParameter>();
        private object m_lock = new object();
        private DynamicMethod m_dynamicMethod;
        private FormMethodHandler m_methodHandler;
        private List<FormMethodParameter> m_orderedParameters;
        private List<FormMethodParameter> m_requiredParameters;

        public void Invoke(
          CommandResult result,
          CommandContext context,
          CommandTokenizer tokenizer,
          object commandInstance,
          bool enable,
          List<string> sourceFilters)
        {
            if (!this.IsInFilter(enable, sourceFilters))
            {
                LogHelper.Instance.Log(string.Format("Command {0} is not in filters.", (object)this.Method.Name));
                result.Errors.Add(Error.NewError("S514", string.Format("Invalid Command: {0}", (object)this.Method.Name), ""));
            }
            object[] values = new object[this.OrderedParameters.Count + 1];
            values[0] = (object)context;
            int num = 1;
            foreach (FormMethodParameter orderedParameter in this.OrderedParameters)
            {
                KeyValuePair keyValuePair = tokenizer.Tokens.GetKeyValuePair(orderedParameter.KeyName);
                if (keyValuePair == null)
                {
                    values[num++] = (object)null;
                }
                else
                {
                    try
                    {
                        values[num++] = orderedParameter.ConvertValue(keyValuePair.Value);
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(Error.NewError("S999", string.Format("Unable to convert key '{0}:' to type '{1}'.", (object)keyValuePair.Key, (object)orderedParameter.Parameter.ParameterType.Name), ex));
                    }
                }
            }
            if (result.Errors.ContainsError())
                return;
            try
            {
                this.InvokeMethod(commandInstance, values);
            }
            catch (Exception ex)
            {
                result.Errors.Add(Error.NewError("S999", string.Format("Execution of command form method '{0}.{1}' failed.", (object)commandInstance.GetType().Name, (object)this.Method.Name), ex));
            }
        }

        public bool ValidateParameters(
          CommandResult result,
          CommandTokenizer tokenizer,
          Action<string, string> paramAction)
        {
            foreach (FormMethodParameter requiredParameter in this.RequiredParameters)
            {
                KeyValuePair keyValuePair = tokenizer.Tokens.GetKeyValuePair(requiredParameter.KeyName);
                if (keyValuePair != null)
                {
                    if (paramAction != null)
                        paramAction(keyValuePair.Key, keyValuePair.Value);
                }
                else
                    result.Errors.Add(Error.NewError("S001", string.Format("The named parameter '{0}:' is required.", (object)requiredParameter.KeyName.ToLower()), "Submit the command specifying the missing named parameter."));
            }
            return !result.Errors.ContainsError();
        }

        public string Usage { get; set; }

        public MethodInfo Method { get; set; }

        public string Description { get; set; }

        public List<string> Filters => this._filters;

        public bool Loggable { set; get; }

        public List<FormMethodParameter> OrderedParameters
        {
            get
            {
                if (this.m_orderedParameters == null)
                {
                    this.m_orderedParameters = new List<FormMethodParameter>((IEnumerable<FormMethodParameter>)this.ParameterCache.Values);
                    this.m_orderedParameters.Sort((Comparison<FormMethodParameter>)((x, y) => x.Index.CompareTo(y.Index)));
                }
                return this.m_orderedParameters;
            }
        }

        public List<FormMethodParameter> RequiredParameters
        {
            get
            {
                if (this.m_requiredParameters == null)
                {
                    lock (this.m_lock)
                    {
                        if (this.m_requiredParameters == null)
                        {
                            List<FormMethodParameter> formMethodParameterList = new List<FormMethodParameter>();
                            foreach (string key in (IEnumerable<string>)this.ParameterCache.Keys)
                            {
                                FormMethodParameter formMethodParameter = this.ParameterCache[key];
                                if (formMethodParameter == null)
                                    LogHelper.Instance.Log(string.Format("FormMethod.RequiredParameters: Parameter name '{0}' is in the ParameterCache but the value is null.", (object)key), LogEntryType.Info);
                                if (formMethodParameter != null && formMethodParameter.IsRequired())
                                    formMethodParameterList.Add(formMethodParameter);
                            }
                            this.m_requiredParameters = formMethodParameterList;
                        }
                    }
                }
                return this.m_requiredParameters;
            }
        }

        private bool IsInFilter(bool enable, List<string> sourceFilters)
        {
            if (!enable)
                return true;
            return sourceFilters != null && sourceFilters.Count != 0 && this._filters.Count != 0 && sourceFilters.Intersect<string>((IEnumerable<string>)this._filters, (IEqualityComparer<string>)StringComparer.CurrentCultureIgnoreCase).Count<string>() > 0;
        }

        internal void Compile(Type instanceType)
        {
            if (this.m_methodHandler != null)
                return;
            if ((MethodInfo)this.m_dynamicMethod == (MethodInfo)null)
            {
                this.m_dynamicMethod = new DynamicMethod(string.Format("___{0}", (object)this.Method.Name), this.Method.ReturnType, new Type[2]
                {
          typeof (object),
          typeof (object[])
                }, instanceType, true);
                ILGenerator ilGenerator = this.m_dynamicMethod.GetILGenerator();
                if (!this.Method.IsStatic)
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldarg_1);
                ilGenerator.Emit(OpCodes.Ldc_I4_0);
                ilGenerator.Emit(OpCodes.Ldelem_Ref);
                for (int index = 0; index < this.OrderedParameters.Count; ++index)
                {
                    ilGenerator.Emit(OpCodes.Ldarg_1);
                    ilGenerator.Emit(OpCodes.Ldc_I4, index + 1);
                    ilGenerator.Emit(OpCodes.Ldelem_Ref);
                    Type parameterType = this.OrderedParameters[index].Parameter.ParameterType;
                    if (parameterType.IsValueType)
                        ilGenerator.Emit(OpCodes.Unbox_Any, parameterType);
                }
                if (this.Method.IsVirtual)
                    ilGenerator.Emit(OpCodes.Callvirt, this.Method);
                else
                    ilGenerator.Emit(OpCodes.Call, this.Method);
                ilGenerator.Emit(OpCodes.Ret);
            }
            this.m_methodHandler = (FormMethodHandler)this.m_dynamicMethod.CreateDelegate(typeof(FormMethodHandler));
        }

        internal void InvokeMethod(object instance, object[] values)
        {
            if (this.m_methodHandler == null)
                this.Method.Invoke(instance, values);
            else
                this.m_methodHandler(instance, values);
        }
    }
}
