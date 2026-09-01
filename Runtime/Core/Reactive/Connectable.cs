using System;
using System.Collections.Generic;
using UnityEngine;

namespace VADE.DevTools.Reactive
{

    public sealed class Connectable : IDisposable
    {
        private readonly List<IDisposable> _disposables = new();

        public int Count => _disposables.Count;

        public void Add(IDisposable disposable)
        {
            if (disposable == null) return;
            _disposables.Add(disposable);
        }

        public static Connectable operator +(Connectable connectable, IDisposable disposable)
        {
            connectable.Add(disposable);
            return connectable;
        }

        public void Dispose()
        {
            for (int i = 0; i < _disposables.Count; i++)
            {
                try
                {
                    _disposables[i]?.Dispose();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            _disposables.Clear();
        }
    }
}
