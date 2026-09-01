using System;
using System.Collections.Generic;
using VADE.DevTools.Reactive;

namespace VADE.DevTools.StateMachine
{

    public class StateMachine<TState>
    {
        private readonly Reactive<TState> _current;
        private readonly Dictionary<TState, Action> _onEnter = new();
        private readonly Dictionary<TState, Action> _onExit = new();

        public TState Current => _current.value;

        public event Action<TState> OnChanged
        {
            add => _current.OnChanged += value;
            remove => _current.OnChanged -= value;
        }

        public StateMachine(TState initial)
        {
            _current = new Reactive<TState>(initial);
        }

        public StateMachine<TState> OnEnter(TState state, Action callback)
        {
            _onEnter[state] = callback;
            return this;
        }

        public StateMachine<TState> OnExit(TState state, Action callback)
        {
            _onExit[state] = callback;
            return this;
        }

        public bool Is(TState state) => Equals(Current, state);

        public void ChangeState(TState next)
        {
            if (Equals(_current.value, next)) return;

            if (_onExit.TryGetValue(_current.value, out var exit))
                exit?.Invoke();

            _current.value = next;

            if (_onEnter.TryGetValue(next, out var enter))
                enter?.Invoke();
        }
    }
}
