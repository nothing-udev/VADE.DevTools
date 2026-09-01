using System;
using System.Collections.Generic;
using UnityEngine;
using VADE.DevTools.Reactive;

namespace VADE.DevTools.Extensions
{

    public static class UIListExtensions
    {

        public static void Present<TItem, TView>(
            this IReadOnlyList<TItem> list,
            Transform container,
            TView viewPrefab,
            Action<int, TItem, TView> bind) where TView : Component
        {
            if (container == null || viewPrefab == null)
            {
                Debug.LogWarning("[UIListExtensions] Present: container или viewPrefab не заданы.");
                return;
            }

            int needed = list.Count;
            int existing = container.childCount;

            for (int i = existing; i < needed; i++)
            {
                var view = UnityEngine.Object.Instantiate(viewPrefab, container);
                view.gameObject.SetActive(true);
            }

            for (int i = needed; i < existing; i++)
                container.GetChild(i).gameObject.SetActive(false);

            for (int i = 0; i < needed; i++)
            {
                var child = container.GetChild(i);
                if (!child.gameObject.activeSelf)
                    child.gameObject.SetActive(true);

                var view = child.GetComponent<TView>();
                bind(i, list[i], view);
            }
        }

        public static IDisposable BindTo<TItem, TView>(
            this ReactiveList<TItem> list,
            Transform container,
            TView viewPrefab,
            Action<int, TItem, TView> bind) where TView : Component
        {
            void Render() => ((IReadOnlyList<TItem>)list).Present(container, viewPrefab, bind);

            void OnAdd(TItem _, int __) => Render();
            void OnRemove(TItem _, int __) => Render();
            void OnSet(int _, TItem __, TItem ___) => Render();
            void OnReset() => Render();

            list.OnAdd += OnAdd;
            list.OnRemove += OnRemove;
            list.OnSet += OnSet;
            list.OnReset += OnReset;

            Render();

            return new ListSubscription(() =>
            {
                list.OnAdd -= OnAdd;
                list.OnRemove -= OnRemove;
                list.OnSet -= OnSet;
                list.OnReset -= OnReset;
            });
        }

        private sealed class ListSubscription : IDisposable
        {
            private Action _unsubscribe;
            public ListSubscription(Action unsubscribe) => _unsubscribe = unsubscribe;
            public void Dispose()
            {
                _unsubscribe?.Invoke();
                _unsubscribe = null;
            }
        }
    }
}
