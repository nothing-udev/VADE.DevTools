using System;
using System.Collections.Generic;
using UnityEngine;

namespace VADE.DevTools.DI
{

    public static class Dependency
    {
        private static readonly Dictionary<Type, object> _instances = new();

        public static void Register<T>(T instance)
        {
            var type = typeof(T);
            if (_instances.ContainsKey(type))
                Debug.LogWarning($"[Dependency] '{type.Name}' уже зарегистрирован — предыдущий экземпляр будет заменён.");
            _instances[type] = instance;
        }

        public static void Register<TContract, TImplementation>(TImplementation instance) where TImplementation : TContract
        {
            Register<TContract>(instance);
        }

        public static T Resolve<T>()
        {
            if (_instances.TryGetValue(typeof(T), out var instance))
                return (T)instance;

            throw new InvalidOperationException($"[Dependency] '{typeof(T).Name}' не зарегистрирован. Проверьте порядок вызовов в Bootstrap.");
        }

        public static bool TryResolve<T>(out T instance)
        {
            if (_instances.TryGetValue(typeof(T), out var raw))
            {
                instance = (T)raw;
                return true;
            }

            instance = default;
            return false;
        }

        public static bool IsRegistered<T>() => _instances.ContainsKey(typeof(T));

        public static void Unregister<T>() => _instances.Remove(typeof(T));

        public static void Clear() => _instances.Clear();
    }
}
