using System;
using System.Reflection;

namespace Redbox.HAL.Core;

public static class Singleton<T> where T : class
{
    static Singleton()
    {
        var type = typeof(T);
        Instance = ((type.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Length == 0
                        ? type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes,
                            null)
                        : throw new SingletonException(
                            "A Singleton<T> class must not implement public instance constructors.")) ??
                    throw new SingletonException(
                        "A Singleton<T> class must implement a non-public instance constructor."))
            .Invoke(null) as T;
    }

    public static T Instance { get; private set; }
}