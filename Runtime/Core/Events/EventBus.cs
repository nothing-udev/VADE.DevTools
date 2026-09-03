using System;
using System.Collections.Generic;

namespace VADE.DevTools.Events
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> handlers = new();

        public static IDisposable Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);

            if (handlers.TryGetValue(type, out var existing))
                handlers[type] = Delegate.Combine(existing, handler);
            else
                handlers[type] = handler;

            return new Subscription(() => Unsubscribe(handler));
        }

        private static void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!handlers.TryGetValue(type, out var existing)) return;

            var result = Delegate.Remove(existing, handler);
            if (result == null) handlers.Remove(type);
            else handlers[type] = result;
        }

        public static void Publish<T>(T message)
        {
            if (handlers.TryGetValue(typeof(T), out var d) && d is Action<T> action)
                action.Invoke(message);
        }

        public static bool HasSubscribers<T>() => handlers.ContainsKey(typeof(T));

        public static void Clear() => handlers.Clear();

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
