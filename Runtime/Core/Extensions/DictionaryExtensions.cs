using System;
using System.Collections.Generic;
using System.Text;

namespace VADE.DevTools.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> factory)
        {
            if (!dict.TryGetValue(key, out var value))
            {
                value = factory();
                dict[key] = value;
            }
            return value;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue fallback = default)
        {
            return dict.TryGetValue(key, out var value) ? value : fallback;
        }

        public static string GetToString<TKey, TValue>(this Dictionary<TKey, TValue> dict, string separator = ", ")
        {
            var sb = new StringBuilder();
            sb.Append($"[{dict.Count}]");
            foreach (var kv in dict)
            {
                sb.Append(separator);
                sb.Append($"{kv.Key}: {kv.Value}");
            }
            return sb.ToString();
        }
    }
}
