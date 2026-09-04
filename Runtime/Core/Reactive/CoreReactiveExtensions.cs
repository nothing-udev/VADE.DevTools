using System;
using UnityEngine;

namespace VADE.DevTools.Reactive
{
    public static class CoreReactiveExtensions
    {
        public static IDisposable Subscribe<T>(this Reactive<T> prop, Action<T> onChanged)
        {
            void Update(T v) => onChanged(v);

            prop.OnChanged += Update;
            onChanged(prop.value);

            return new Subscription(() => prop.OnChanged -= Update);
        }

        public static IDisposable SubscribeAndInvoke<T>(this Reactive<T> prop, Action<T> onChanged)
            => prop.Subscribe(onChanged);

        public static IDisposable BindTo(this Reactive<bool> prop, GameObject go)
        {
            void Update(bool v) => go.SetActive(v);
            prop.OnChanged += Update;
            go.SetActive(prop.value);
            return new Subscription(() => prop.OnChanged -= Update);
        }

        private sealed class Subscription : IDisposable
        {
            private Action _unsubscribe;
            public Subscription(Action unsubscribe) => _unsubscribe = unsubscribe;
            public void Dispose()
            {
                _unsubscribe?.Invoke();
                _unsubscribe = null;
            }
        }
    }
}
