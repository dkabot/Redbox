using System;
using System.Reflection;

namespace Redbox.Core
{
    internal static class Singleton<T> where T : class
    {
        public static T Instance { get; private set; }

        static Singleton()
        {
            Type type = typeof(T);
            ConstructorInfo constructorInfo = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Length == 0 ? type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, (Binder)null, Type.EmptyTypes, (ParameterModifier[])null) : throw new SingletonException("A Singleton<T> class must not implement public instance constructors.");
            Singleton<T>.Instance = !(constructorInfo == (ConstructorInfo)null) ? constructorInfo.Invoke((object[])null) as T : throw new SingletonException("A Singleton<T> class must implement a non-public instance constructor.");
        }
    }
}
