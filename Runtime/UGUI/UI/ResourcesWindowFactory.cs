using System;
using UnityEngine;

namespace VADE.DevTools.UI
{

    public class ResourcesWindowFactory : IWindowFactory
    {
        public T Create<T>(Transform parent) where T : Window
        {
            var prefab = Resources.Load<T>($"UI/{typeof(T).Name}");
            if (prefab == null)
                throw new Exception($"Prefab UI/{typeof(T).Name} not found in Resources!");

            return UnityEngine.Object.Instantiate(prefab, parent);
        }
    }
}
