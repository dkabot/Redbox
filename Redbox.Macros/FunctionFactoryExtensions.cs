using System;

namespace Redbox.Macros
{
    public static class FunctionFactoryExtensions
    {
        public static bool IsSubclassOfFunctionSetBase(this Type type)
        {
            if (type.BaseType.FullName == "System.Object")
                return false;
            return type.BaseType.FullName == "Redbox.Macros.FunctionSetBase" ||
                   type.BaseType.IsSubclassOfFunctionSetBase();
        }

        public static bool IsTypeOfExpressionEvaluator(this Type type)
        {
            return type.FullName == "Redbox.Macros.ExpressionEvaluator";
        }
    }
}