using System;
using UnityEngine.Events;

namespace VADE.DevTools.Reactive
{
    public static class UnityEventExtensions
    {
        public static IDisposable Subscribe(this UnityEvent unityEvent, Action action)
        {
            UnityAction unityAction = () => action();
            unityEvent.AddListener(unityAction);
            return new Subscription(() => unityEvent.RemoveListener(unityAction));
        }

        public static IDisposable Subscribe<T>(this UnityEvent<T> unityEvent, Action<T> action)
        {
            UnityAction<T> unityAction = value => action(value);
            unityEvent.AddListener(unityAction);
            return new Subscription(() => unityEvent.RemoveListener(unityAction));
        }

        private sealed class Subscription : IDisposable
        {
            private Action unsubscribe;
            public Subscription(Action unsubscribe) => this.unsubscribe = unsubscribe;
            public void Dispose()
            {
                unsubscribe?.Invoke();
                unsubscribe = null;
            }
        }
    }
}
