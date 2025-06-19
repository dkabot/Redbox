using System;

// Puyodead1 - I aint fixing all this shit, if you want to, go for it

namespace Redbox.Rental.UI
{
    public class DynamicRouting
    {
        private static DynamicRoutedCommand InitRoutedCommand(DynamicRoutedCommand routeCommand, dynamic execute,
            dynamic canExecute)
        {
            if (routeCommand == null) throw new NullReferenceException("The routeCommand can not be null");
            //if (DynamicRouting.<>o__0.<>p__0 == null)
            //{
            //	DynamicRouting.<>o__0.<>p__0 = CallSite<Action<CallSite, DynamicRoutedCommand, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "InitDynamicExecutes", null, typeof(DynamicRouting), new CSharpArgumentInfo[]
            //	{
            //		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
            //		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
            //		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
            //	}));
            //}
            //DynamicRouting.<>o__0.<>p__0.Target(DynamicRouting.<>o__0.<>p__0, routeCommand, execute, canExecute);
            return routeCommand;
        }

        public static DynamicRoutedCommand RegisterRoutedCommand(DynamicRoutedCommand routeCommand, Action execute,
            Func<bool> canExecute = null)
        {
            return InitRoutedCommand(routeCommand, execute, canExecute);
        }

        public static DynamicRoutedCommand RegisterRoutedCommand(DynamicRoutedCommand routeCommand, Action<int> execute,
            Predicate<int> canExecute = null)
        {
            return InitRoutedCommand(routeCommand, execute, canExecute);
        }

        public static DynamicRoutedCommand RegisterRoutedCommand(DynamicRoutedCommand routeCommand,
            Action<bool> execute, Predicate<bool> canExecute = null)
        {
            return InitRoutedCommand(routeCommand, execute, canExecute);
        }

        public static DynamicRoutedCommand RegisterRoutedCommand(DynamicRoutedCommand routeCommand,
            Action<double> execute, Predicate<double> canExecute = null)
        {
            return InitRoutedCommand(routeCommand, execute, canExecute);
        }

        public static DynamicRoutedCommand RegisterRoutedCommand(DynamicRoutedCommand routeCommand,
            Action<string> execute, Predicate<string> canExecute = null)
        {
            return InitRoutedCommand(routeCommand, execute, canExecute);
        }

        public static DynamicRoutedCommand RegisterRoutedCommand(DynamicRoutedCommand routeCommand,
            Action<object> execute, Predicate<object> canExecute = null)
        {
            return InitRoutedCommand(routeCommand, execute, canExecute);
        }

        public static DynamicRoutedCommand RegisterRoutedCommand<T>(DynamicRoutedCommand routeCommand,
            Action<T> execute, Predicate<object> canExecute = null)
        {
            return InitRoutedCommand(routeCommand, execute, canExecute);
        }
    }
}