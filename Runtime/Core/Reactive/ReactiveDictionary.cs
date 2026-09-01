using System;
using System.Collections;
using System.Collections.Generic;

namespace VADE.DevTools.Reactive
{

    public class ReactiveDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _items;

        public event Action<TKey, TValue> OnAdd;
        public event Action<TKey, TValue> OnRemove;
        public event Action<TKey, TValue, TValue> OnSet;
        public event Action OnReset;

        public ReactiveDictionary(IEqualityComparer<TKey> comparer = null)
        {
            _items = comparer != null ? new Dictionary<TKey, TValue>(comparer) : new Dictionary<TKey, TValue>();
        }

        public int Count => _items.Count;
        public IEnumerable<TKey> Keys => _items.Keys;
        public IEnumerable<TValue> Values => _items.Values;

        public TValue this[TKey key]
        {
            get => _items[key];
            set
            {
                if (_items.TryGetValue(key, out var old))
                {
                    _items[key] = value;
                    OnSet?.Invoke(key, old, value);
                }
                else
                {
                    _items[key] = value;
                    OnAdd?.Invoke(key, value);
                }
            }
        }

        public bool ContainsKey(TKey key) => _items.ContainsKey(key);

        public bool TryGetValue(TKey key, out TValue value) => _items.TryGetValue(key, out value);

        public bool Remove(TKey key)
        {
            if (!_items.TryGetValue(key, out var value)) return false;
            _items.Remove(key);
            OnRemove?.Invoke(key, value);
            return true;
        }

        public void Clear()
        {
            _items.Clear();
            OnReset?.Invoke();
        }

        public Dictionary<TKey, TValue> ToDictionary() => new Dictionary<TKey, TValue>(_items);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
