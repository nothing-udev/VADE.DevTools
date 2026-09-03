using System.Collections.Generic;
using UnityEngine;

namespace VADE.DevTools.Utilities
{
    public struct WeightedItem<T>
    {
        public T item;
        public float weight;

        public WeightedItem(T item, float weight)
        {
            this.item = item;
            this.weight = weight;
        }
    }

    public class WeightedRandom<T>
    {
        private readonly List<WeightedItem<T>> items = new();
        private float totalWeight;

        public int Count => items.Count;

        public WeightedRandom<T> Add(T item, float weight)
        {
            if (weight <= 0f) return this;
            items.Add(new WeightedItem<T>(item, weight));
            totalWeight += weight;
            return this;
        }

        public void Clear()
        {
            items.Clear();
            totalWeight = 0f;
        }

        public T Get()
        {
            if (items.Count == 0) return default;
            return Pick(Random.value * totalWeight);
        }

        public T Get(int seed)
        {
            if (items.Count == 0) return default;
            var random = new System.Random(seed);
            return Pick((float)random.NextDouble() * totalWeight);
        }

        private T Pick(float roll)
        {
            float cumulative = 0f;
            foreach (var entry in items)
            {
                cumulative += entry.weight;
                if (roll <= cumulative) return entry.item;
            }
            return items[^1].item;
        }
    }
}
