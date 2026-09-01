using System;

namespace VADE.DevTools.UI
{
    public static class ToggleInputExtensions
    {
        public static IDisposable Subscribe(this ToggleInput toggle, Action<bool> onChanged)
        {
            UnityEngine.Events.UnityAction<bool> unityAction = (val) => onChanged(val);

            toggle.toggleOutputEvent.AddListener(unityAction);
            return new Subscription(() => toggle.toggleOutputEvent.RemoveListener(unityAction));
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
