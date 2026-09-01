using UnityEngine;

namespace VADE.DevTools.Extensions
{
    public static class ComponentExtensions
    {
        public static T GetOrAdd<T>(this GameObject go) where T : Component
        {
            var comp = go.GetComponent<T>();
            if (comp == null)
                comp = go.AddComponent<T>();
            return comp;
        }
    }
}
