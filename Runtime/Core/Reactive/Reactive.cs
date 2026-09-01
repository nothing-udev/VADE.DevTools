using System;

namespace VADE.DevTools.Reactive
{
    public class Reactive<T>
    {
        private T _value;
        private readonly Action<T> _onSet;

        public event Action<T> OnChanged;

        public T value
        {
            get => _value;
            set
            {
                if (Equals(_value, value)) return;
                _value = value;
                _onSet?.Invoke(_value);
                OnChanged?.Invoke(_value);
            }
        }

        public Reactive(T initialValue = default, Action<T> onSet = null)
        {
            _value = initialValue;
            _onSet = onSet;
        }

        public void SetSilently(T v) => _value = v;

        public override string ToString() => _value?.ToString() ?? "null";

        public static implicit operator T(Reactive<T> reactive) => reactive._value;
    }
}
