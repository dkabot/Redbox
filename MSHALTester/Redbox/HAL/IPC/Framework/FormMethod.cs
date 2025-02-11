using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework;

internal class FormMethod
{
    public readonly IDictionary<string, FormMethodParameter> ParameterCache =
        new Dictionary<string, FormMethodParameter>();

    private DynamicMethod m_dynamicMethod;
    private FormMethodHandler m_methodHandler;
    private List<FormMethodParameter> m_orderedParameters;
    private List<FormMethodParameter> m_requiredParameters;

    public string Usage { get; set; }

    public MethodInfo Method { get; set; }

    public string Description { get; set; }

    public List<FormMethodParameter> OrderedParameters
    {
        get
        {
            if (m_orderedParameters == null)
            {
                m_orderedParameters = new List<FormMethodParameter>(ParameterCache.Values);
                m_orderedParameters.Sort((x, y) => x.Index.CompareTo(y.Index));
            }

            return m_orderedParameters;
        }
    }

    public List<FormMethodParameter> RequiredParameters
    {
        get
        {
            if (m_requiredParameters == null)
            {
                m_requiredParameters = new List<FormMethodParameter>();
                foreach (var key in ParameterCache.Keys)
                {
                    var formMethodParameter = ParameterCache[key];
                    if (formMethodParameter.IsRequired())
                        m_requiredParameters.Add(formMethodParameter);
                }
            }

            return m_requiredParameters;
        }
    }

    public void Invoke(
        CommandResult result,
        CommandContext context,
        CommandTokenizer tokenizer,
        object commandInstance)
    {
        var values = new object[OrderedParameters.Count + 1];
        values[0] = context;
        var num = 1;
        foreach (var orderedParameter in OrderedParameters)
        {
            var keyValuePair = tokenizer.Tokens.GetKeyValuePair(orderedParameter.KeyName);
            if (keyValuePair == null)
                values[num++] = null;
            else
                try
                {
                    values[num++] = orderedParameter.ConvertValue(keyValuePair.Value);
                }
                catch (Exception ex)
                {
                    result.Errors.Add(Error.NewError("S999",
                        string.Format("Unable to convert key '{0}:' to type '{1}'.", keyValuePair.Key,
                            orderedParameter.Parameter.ParameterType.Name), ex));
                }
        }

        if (result.Errors.ContainsError())
            return;
        try
        {
            InvokeMethod(commandInstance, values);
        }
        catch (Exception ex)
        {
            result.Errors.Add(Error.NewError("S999",
                string.Format("Execution of command form method '{0}.{1}' failed.", commandInstance.GetType().Name,
                    Method.Name), ex));
        }
    }

    public bool ValidateParameters(CommandResult result, CommandTokenizer tokenizer)
    {
        foreach (var requiredParameter in RequiredParameters)
            if (tokenizer.Tokens.GetKeyValuePair(requiredParameter.KeyName) == null)
                result.Errors.Add(Error.NewError("S001",
                    string.Format("The named parameter '{0}:' is required.", requiredParameter.KeyName.ToLower()),
                    "Submit the command specifying the missing named parameter."));
        return !result.Errors.ContainsError();
    }

    internal void InvokeMethod(object instance, object[] values)
    {
        if (m_methodHandler == null)
        {
            m_dynamicMethod = new DynamicMethod(string.Format("___{0}", Method.Name), Method.ReturnType, new Type[2]
            {
                typeof(object),
                typeof(object[])
            }, instance.GetType(), true);
            var ilGenerator = m_dynamicMethod.GetILGenerator();
            if (!Method.IsStatic)
                ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldc_I4_0);
            ilGenerator.Emit(OpCodes.Ldelem_Ref);
            for (var index = 0; index < OrderedParameters.Count; ++index)
            {
                ilGenerator.Emit(OpCodes.Ldarg_1);
                ilGenerator.Emit(OpCodes.Ldc_I4, index + 1);
                ilGenerator.Emit(OpCodes.Ldelem_Ref);
                var parameterType = OrderedParameters[index].Parameter.ParameterType;
                if (parameterType.IsValueType)
                    ilGenerator.Emit(OpCodes.Unbox_Any, parameterType);
            }

            if (Method.IsVirtual)
                ilGenerator.Emit(OpCodes.Callvirt, Method);
            else
                ilGenerator.Emit(OpCodes.Call, Method);
            ilGenerator.Emit(OpCodes.Ret);
            m_methodHandler = (FormMethodHandler)m_dynamicMethod.CreateDelegate(typeof(FormMethodHandler));
        }

        m_methodHandler(instance, values);
    }
}