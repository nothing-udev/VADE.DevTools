using System;
using System.Collections;
using System.Collections.Generic;

namespace VADE.DevTools.Reactive
{

    public class ReactiveList<T> : IReadOnlyList<T>
    {
        private readonly List<T> _items;

        public event Action<T, int> OnAdd;

        public event Action<T, int> OnRemove;

        public event Action<int, T, T> OnSet;

        public event Action OnReset;

        public ReactiveList(IEnumerable<T> initial = null)
        {
            _items = initial != null ? new List<T>(initial) : new List<T>();
        }

        public int Count => _items.Count;

        public T this[int index]
        {
            get => _items[index];
            set
            {
                T old = _items[index];
                _items[index] = value;
                OnSet?.Invoke(index, old, value);
            }
        }

        public void Add(T item)
        {
            _items.Add(item);
            OnAdd?.Invoke(item, _items.Count - 1);
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
                Add(item);
        }

        public void Insert(int index, T item)
        {
            _items.Insert(index, item);
            OnAdd?.Invoke(item, index);
        }

        public bool Remove(T item)
        {
            int index = _items.IndexOf(item);
            if (index < 0) return false;
            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            T item = _items[index];
            _items.RemoveAt(index);
            OnRemove?.Invoke(item, index);
        }

        public void Clear()
        {
            _items.Clear();
            OnReset?.Invoke();
        }

        public void Reset(IEnumerable<T> newItems)
        {
            _items.Clear();
            if (newItems != null)
                _items.AddRange(newItems);
            OnReset?.Invoke();
        }

        public int IndexOf(T item) => _items.IndexOf(item);
        public bool Contains(T item) => _items.Contains(item);
        public List<T> ToList() => new List<T>(_items);

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
