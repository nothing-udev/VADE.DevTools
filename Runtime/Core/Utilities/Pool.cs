using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VADE.DevTools.Utilities
{

    public class Pool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _inactive = new();
        private readonly HashSet<T> _active = new();
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;

        public int CountActive => _active.Count;
        public int CountInactive => _inactive.Count;
        public int CountAll => CountActive + CountInactive;

        public Pool(T prefab, Transform parent = null, int prewarm = 0, Action<T> onGet = null, Action<T> onRelease = null)
        {
            _prefab = prefab;
            _parent = parent;
            _onGet = onGet;
            _onRelease = onRelease;

            for (int i = 0; i < prewarm; i++)
                _inactive.Push(CreateNew());
        }

        private T CreateNew()
        {
            var instance = UnityEngine.Object.Instantiate(_prefab, _parent);
            instance.gameObject.SetActive(false);
            return instance;
        }

        public T Get()
        {
            T instance = _inactive.Count > 0 ? _inactive.Pop() : CreateNew();
            _active.Add(instance);
            instance.gameObject.SetActive(true);
            _onGet?.Invoke(instance);
            return instance;
        }

        public void Release(T instance)
        {
            if (instance == null || !_active.Remove(instance)) return;

            _onRelease?.Invoke(instance);
            instance.gameObject.SetActive(false);
            _inactive.Push(instance);
        }

        public void ReleaseAll()
        {
            foreach (var instance in _active.ToArray())
                Release(instance);
        }

        public void Clear()
        {
            ReleaseAll();
            while (_inactive.Count > 0)
            {
                var instance = _inactive.Pop();
                if (instance != null)
                    UnityEngine.Object.Destroy(instance.gameObject);
            }
        }
    }
}
