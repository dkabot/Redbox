using System;
using System.Reflection;

namespace Redbox.Core
{
    public static class Singleton<T> where T : class
    {
        static Singleton()
        {
            var type = typeof(T);
            var constructorInfo = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Length == 0
                ? type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null)
                : throw new SingletonException("A Singleton<T> class must not implement public instance constructors.");
            Instance = !(constructorInfo == null)
                ? constructorInfo.Invoke(null) as T
                : throw new SingletonException(
                    "A Singleton<T> class must implement a non-public instance constructor.");
        }

        public static T Instance { get; private set; }
    }
}