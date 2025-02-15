using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using Redbox.Core;

namespace Redbox.Macros
{
    public sealed class FunctionFactory
    {
        private static readonly Hashtable m_methodInfoCollection = new Hashtable();

        [ReflectionPermission(SecurityAction.Demand, Flags = ReflectionPermissionFlag.NoFlags)]
        public static bool ScanAssembly(string assemblyFile)
        {
            return ScanAssembly(Assembly.LoadFrom(assemblyFile));
        }

        [ReflectionPermission(SecurityAction.Demand, Flags = ReflectionPermissionFlag.NoFlags)]
        public static bool ScanAssembly(Assembly assembly)
        {
            var flag1 = false;
            foreach (var type in assembly.GetTypes())
            {
                var flag2 = ScanTypeForFunctions(type);
                flag1 |= flag2;
            }

            return flag1;
        }

        [ReflectionPermission(SecurityAction.Demand, Flags = ReflectionPermissionFlag.NoFlags)]
        public static void ScanDir(string path, bool failOnError)
        {
            if (string.IsNullOrEmpty(path))
                return;
            var directoryInfo = new DirectoryInfo(path);
            var fileInfoList = new List<FileInfo>(directoryInfo.GetFiles("*.dll"));
            fileInfoList.AddRange(directoryInfo.GetFiles("*.exe"));
            fileInfoList.ForEach(f =>
            {
                try
                {
                    ScanAssembly(f.FullName);
                }
                catch (Exception ex)
                {
                    var str = string.Format(CultureInfo.InvariantCulture, "Failure scanning \"{0}\" for extensions", f);
                    if (failOnError)
                        throw new EvaluatorException(str + ".", Location.UnknownLocation, ex);
                }
            });
        }

        [ReflectionPermission(SecurityAction.Demand, Flags = ReflectionPermissionFlag.NoFlags)]
        public static void ScanExecutionPath()
        {
            ScanDir(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", ""),
                true);
        }

        internal static MethodInfo LookupFunction(string functionName, FunctionArgument[] args)
        {
            var methodInfo = m_methodInfoCollection[functionName];
            var function1 = methodInfo != null
                ? methodInfo as MethodInfo
                : throw new EvaluatorException(string.Format(CultureInfo.InvariantCulture,
                    ResourceUtils.GetString("NA1052"), functionName));
            if (function1 != null)
            {
                if (function1.GetParameters().Length == args.Length)
                {
                    CheckDeprecation(functionName, function1);
                    return function1;
                }
            }
            else
            {
                var arrayList = (ArrayList)methodInfo;
                for (var index = 0; index < arrayList.Count; ++index)
                {
                    var function2 = (MethodInfo)arrayList[index];
                    if (function2.GetParameters().Length == args.Length)
                    {
                        CheckDeprecation(functionName, function2);
                        return function2;
                    }
                }
            }

            throw new EvaluatorException(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1044"),
                functionName, args.Length));
        }

        private static void CheckDeprecation(string functionName, MemberInfo function)
        {
            var obsoleteAttribute =
                (ObsoleteAttribute)Attribute.GetCustomAttribute(function, typeof(ObsoleteAttribute), true) ??
                (ObsoleteAttribute)Attribute.GetCustomAttribute(function.DeclaringType, typeof(ObsoleteAttribute),
                    true);
            if (obsoleteAttribute == null)
                return;
            var message = string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1087"), functionName,
                obsoleteAttribute.Message);
            if (obsoleteAttribute.IsError)
                throw new EvaluatorException(message, Location.UnknownLocation);
            LogHelper.Instance.Log(string.Format("{0}", message), LogEntryType.Info);
        }

        private static bool ScanTypeForFunctions(Type type)
        {
            try
            {
                var functionSetAttribute = (FunctionSetAttribute)ExtrapolateAttribute(type,
                    "Redbox.Macros.FunctionSetAttribute", FunctionSetAttributeConstructor);
                if (functionSetAttribute == null)
                    return false;
                var flag = type.IsTypeOfExpressionEvaluator();
                if (type.IsSubclassOfFunctionSetBase() && !type.IsAbstract)
                    flag = true;
                if (!flag)
                    return false;
                var prefix = functionSetAttribute.Prefix;
                if (!string.IsNullOrEmpty(prefix))
                {
                    var str = prefix + "::";
                    foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static |
                                                           BindingFlags.Public))
                    {
                        var functionAttribute = (FunctionAttribute)ExtrapolateAttribute(method,
                            "Redbox.Macros.FunctionAttribute", FunctionAttributeConstructor);
                        if (functionAttribute != null)
                            RegisterFunction(str + functionAttribute.Name, method);
                    }

                    return true;
                }

                LogHelper.Instance.Log(
                    string.Format("Ignoring functions in type \"{0}\": no prefix was set.", type.AssemblyQualifiedName),
                    LogEntryType.Info);
                return false;
            }
            catch
            {
                LogHelper.Instance.Log(
                    string.Format("Failure scanning \"{0}\" for functions.", type.AssemblyQualifiedName),
                    LogEntryType.Error);
                throw;
            }
        }

        private static void RegisterFunction(string key, MethodInfo info)
        {
            var methodInfo1 = m_methodInfoCollection[key];
            if (methodInfo1 == null)
            {
                m_methodInfoCollection.Add(key, info);
            }
            else
            {
                var methodInfo2 = methodInfo1 as MethodInfo;
                if (methodInfo2 == null)
                {
                    ((ArrayList)methodInfo1).Add(info);
                }
                else
                {
                    var arrayList = new ArrayList(3)
                    {
                        methodInfo2,
                        info
                    };
                    m_methodInfoCollection[key] = arrayList;
                }
            }
        }

        private static Attribute ExtrapolateAttribute(
            MemberInfo type,
            string fullname,
            ConstructDelegate doconstruct)
        {
            foreach (var customAttribute in Attribute.GetCustomAttributes(type))
            {
                var typeId = customAttribute.TypeId as Type;
                if (typeId != null && typeId.FullName == fullname)
                    return doconstruct(customAttribute);
            }

            return null;
        }

        private static Attribute FunctionSetAttributeConstructor(Attribute attribute)
        {
            return new FunctionSetAttribute(
                attribute.GetType().InvokeMember("Prefix", BindingFlags.GetProperty, null, attribute, new object[0])
                    .ToString(),
                attribute.GetType().InvokeMember("Category", BindingFlags.GetProperty, null, attribute, new object[0])
                    .ToString());
        }

        private static Attribute FunctionAttributeConstructor(Attribute attribute)
        {
            return new FunctionAttribute(attribute.GetType()
                .InvokeMember("Name", BindingFlags.GetProperty, null, attribute, new object[0]).ToString());
        }

        private delegate Attribute ConstructDelegate(Attribute attribute);
    }
}