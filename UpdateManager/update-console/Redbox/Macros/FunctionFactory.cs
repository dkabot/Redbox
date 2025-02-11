using Redbox.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Permissions;

namespace Redbox.Macros
{
    internal sealed class FunctionFactory
    {
        private static readonly Hashtable m_methodInfoCollection = new Hashtable();

        [ReflectionPermission(SecurityAction.Demand, Flags = ReflectionPermissionFlag.NoFlags)]
        public static bool ScanAssembly(string assemblyFile)
        {
            return FunctionFactory.ScanAssembly(Assembly.LoadFrom(assemblyFile));
        }

        [ReflectionPermission(SecurityAction.Demand, Flags = ReflectionPermissionFlag.NoFlags)]
        public static bool ScanAssembly(Assembly assembly)
        {
            bool flag1 = false;
            foreach (Type type in assembly.GetTypes())
            {
                bool flag2 = FunctionFactory.ScanTypeForFunctions(type);
                flag1 |= flag2;
            }
            return flag1;
        }

        [ReflectionPermission(SecurityAction.Demand, Flags = ReflectionPermissionFlag.NoFlags)]
        public static void ScanDir(string path, bool failOnError)
        {
            if (string.IsNullOrEmpty(path))
                return;
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            List<FileInfo> fileInfoList = new List<FileInfo>((IEnumerable<FileInfo>)directoryInfo.GetFiles("*.dll"));
            fileInfoList.AddRange((IEnumerable<FileInfo>)directoryInfo.GetFiles("*.exe"));
            fileInfoList.ForEach((Action<FileInfo>)(f =>
            {
                try
                {
                    FunctionFactory.ScanAssembly(f.FullName);
                }
                catch (Exception ex)
                {
                    string str = string.Format((IFormatProvider)CultureInfo.InvariantCulture, "Failure scanning \"{0}\" for extensions", (object)f);
                    if (failOnError)
                        throw new EvaluatorException(str + ".", Location.UnknownLocation, ex);
                }
            }));
        }

        [ReflectionPermission(SecurityAction.Demand, Flags = ReflectionPermissionFlag.NoFlags)]
        public static void ScanExecutionPath()
        {
            FunctionFactory.ScanDir(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", ""), true);
        }

        internal static MethodInfo LookupFunction(string functionName, FunctionArgument[] args)
        {
            object methodInfo = FunctionFactory.m_methodInfoCollection[(object)functionName];
            MethodInfo function1 = methodInfo != null ? methodInfo as MethodInfo : throw new EvaluatorException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1052"), (object)functionName));
            if (function1 != (MethodInfo)null)
            {
                if (function1.GetParameters().Length == args.Length)
                {
                    FunctionFactory.CheckDeprecation(functionName, (MemberInfo)function1);
                    return function1;
                }
            }
            else
            {
                ArrayList arrayList = (ArrayList)methodInfo;
                for (int index = 0; index < arrayList.Count; ++index)
                {
                    MethodInfo function2 = (MethodInfo)arrayList[index];
                    if (function2.GetParameters().Length == args.Length)
                    {
                        FunctionFactory.CheckDeprecation(functionName, (MemberInfo)function2);
                        return function2;
                    }
                }
            }
            throw new EvaluatorException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1044"), (object)functionName, (object)args.Length));
        }

        private static void CheckDeprecation(string functionName, MemberInfo function)
        {
            ObsoleteAttribute obsoleteAttribute = (ObsoleteAttribute)Attribute.GetCustomAttribute(function, typeof(ObsoleteAttribute), true) ?? (ObsoleteAttribute)Attribute.GetCustomAttribute((MemberInfo)function.DeclaringType, typeof(ObsoleteAttribute), true);
            if (obsoleteAttribute == null)
                return;
            string message = string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1087"), (object)functionName, (object)obsoleteAttribute.Message);
            if (obsoleteAttribute.IsError)
                throw new EvaluatorException(message, Location.UnknownLocation);
            LogHelper.Instance.Log(string.Format("{0}", (object)message), LogEntryType.Info);
        }

        private static bool ScanTypeForFunctions(Type type)
        {
            try
            {
                FunctionSetAttribute functionSetAttribute = (FunctionSetAttribute)FunctionFactory.ExtrapolateAttribute((MemberInfo)type, "Redbox.Macros.FunctionSetAttribute", new FunctionFactory.ConstructDelegate(FunctionFactory.FunctionSetAttributeConstructor));
                if (functionSetAttribute == null)
                    return false;
                bool flag = type.IsTypeOfExpressionEvaluator();
                if (type.IsSubclassOfFunctionSetBase() && !type.IsAbstract)
                    flag = true;
                if (!flag)
                    return false;
                string prefix = functionSetAttribute.Prefix;
                if (!string.IsNullOrEmpty(prefix))
                {
                    string str = prefix + "::";
                    foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
                    {
                        FunctionAttribute functionAttribute = (FunctionAttribute)FunctionFactory.ExtrapolateAttribute((MemberInfo)method, "Redbox.Macros.FunctionAttribute", new FunctionFactory.ConstructDelegate(FunctionFactory.FunctionAttributeConstructor));
                        if (functionAttribute != null)
                            FunctionFactory.RegisterFunction(str + functionAttribute.Name, method);
                    }
                    return true;
                }
                LogHelper.Instance.Log(string.Format("Ignoring functions in type \"{0}\": no prefix was set.", (object)type.AssemblyQualifiedName), LogEntryType.Info);
                return false;
            }
            catch
            {
                LogHelper.Instance.Log(string.Format("Failure scanning \"{0}\" for functions.", (object)type.AssemblyQualifiedName), LogEntryType.Error);
                throw;
            }
        }

        private static void RegisterFunction(string key, MethodInfo info)
        {
            object methodInfo1 = FunctionFactory.m_methodInfoCollection[(object)key];
            if (methodInfo1 == null)
            {
                FunctionFactory.m_methodInfoCollection.Add((object)key, (object)info);
            }
            else
            {
                MethodInfo methodInfo2 = methodInfo1 as MethodInfo;
                if (methodInfo2 == (MethodInfo)null)
                {
                    ((ArrayList)methodInfo1).Add((object)info);
                }
                else
                {
                    ArrayList arrayList = new ArrayList(3)
          {
            (object) methodInfo2,
            (object) info
          };
                    FunctionFactory.m_methodInfoCollection[(object)key] = (object)arrayList;
                }
            }
        }

        private static Attribute ExtrapolateAttribute(
          MemberInfo type,
          string fullname,
          FunctionFactory.ConstructDelegate doconstruct)
        {
            foreach (Attribute customAttribute in Attribute.GetCustomAttributes(type))
            {
                Type typeId = customAttribute.TypeId as Type;
                if (typeId != (Type)null && typeId.FullName == fullname)
                    return doconstruct(customAttribute);
            }
            return (Attribute)null;
        }

        private static Attribute FunctionSetAttributeConstructor(Attribute attribute)
        {
            return (Attribute)new FunctionSetAttribute(attribute.GetType().InvokeMember("Prefix", BindingFlags.GetProperty, (Binder)null, (object)attribute, new object[0]).ToString(), attribute.GetType().InvokeMember("Category", BindingFlags.GetProperty, (Binder)null, (object)attribute, new object[0]).ToString());
        }

        private static Attribute FunctionAttributeConstructor(Attribute attribute)
        {
            return (Attribute)new FunctionAttribute(attribute.GetType().InvokeMember("Name", BindingFlags.GetProperty, (Binder)null, (object)attribute, new object[0]).ToString());
        }

        private delegate Attribute ConstructDelegate(Attribute attribute);
    }
}
