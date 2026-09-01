using System;
using System.Collections.Generic;
using UnityEngine;
using VADE.DevTools.DI;

namespace VADE.DevTools.UI
{
    public class WindowService : MonoBehaviour
    {
        public static WindowService Instance;

        public static IWindowFactory Factory { get; set; } = new ResourcesWindowFactory();

        private readonly Dictionary<Type, Window> cache = new();
        private readonly Stack<Window> windowStack = new();

        private struct PopupEntry
        {
            public Window window;
            public object data;
            public PopupEntry(Window window, object data)
            {
                this.window = window;
                this.data = data;
            }
        }

        private readonly Stack<PopupEntry> popupStack = new();

        private readonly Queue<Action> pendingPopupQueue = new();
        private Window queuedPopupWindow;

        [SerializeField] private Transform screensRoot;
        [SerializeField] private Transform popupsRoot;

        public Transform PopupsRoot => popupsRoot;

        public Window CurrentWindow => windowStack.Count > 0 ? windowStack.Peek() : null;

        public Window CurrentPopup => popupStack.Count > 0 ? popupStack.Peek().window : null;

        public event Action<Window> WindowOpened;

        public event Action<Window> WindowClosed;

        private void Awake()
        {
            Instance = this;
            Dependency.Register(this);
            WindowClosed += OnAnyWindowClosed;
        }

        private void OnDestroy()
        {
            WindowClosed -= OnAnyWindowClosed;

            if (Instance == this)
            {
                Instance = null;
                if (Dependency.IsRegistered<WindowService>())
                    Dependency.Unregister<WindowService>();
            }
        }

        public int PendingPopupCount => pendingPopupQueue.Count;

        public void EnqueuePopup<T>(object data = null) where T : PopupWindow
        {
            void OpenNow() => Open<T>(data);

            if (queuedPopupWindow == null && pendingPopupQueue.Count == 0)
            {
                OpenNow();
                queuedPopupWindow = CurrentPopup;
                return;
            }

            pendingPopupQueue.Enqueue(OpenNow);
        }

        private void OnAnyWindowClosed(Window closed)
        {
            if (queuedPopupWindow == null || !ReferenceEquals(closed, queuedPopupWindow)) return;

            queuedPopupWindow = null;

            if (pendingPopupQueue.Count == 0) return;

            var next = pendingPopupQueue.Dequeue();
            next.Invoke();
            queuedPopupWindow = CurrentPopup;
        }

        public T Open<T>(object data = null) where T : Window
        {
            CleanupDestroyedEntries();

            var type = typeof(T);
            bool isPopup = typeof(PopupWindow).IsAssignableFrom(type);

            if (!cache.TryGetValue(type, out var window) || window == null)
            {
                window = CreateWindow<T>();
                cache[type] = window;
            }

            if (isPopup)
            {
                if (popupStack.Count > 0 && ReferenceEquals(popupStack.Peek().window, window))
                {
                    window.Show(data);
                    WindowOpened?.Invoke(window);
                    return (T)window;
                }

                RemovePopupFromStack(window);

                if (popupStack.Count > 0)
                    popupStack.Peek().window.Hide();

                popupStack.Push(new PopupEntry(window, data));
            }
            else
            {
                if (windowStack.Count > 0 && ReferenceEquals(windowStack.Peek(), window))
                {
                    window.Show(data);
                    WindowOpened?.Invoke(window);
                    return (T)window;
                }

                RemoveWindowFromStack(window);

                if (windowStack.Count > 0)
                    windowStack.Peek().Hide();

                windowStack.Push(window);
            }

            window.Show(data);
            WindowOpened?.Invoke(window);
            return (T)window;
        }

        public void CloseTop()
        {
            CleanupDestroyedEntries();
            if (windowStack.Count == 0) return;

            var top = windowStack.Pop();
            if (top != null)
            {
                top.Hide();
                WindowClosed?.Invoke(top);
            }

            while (windowStack.Count > 0 && windowStack.Peek() == null)
                windowStack.Pop();

            if (windowStack.Count > 0 && windowStack.Peek() != null)
                windowStack.Peek().Show(null);
        }

        public void CloseTopWithoutReveal()
        {
            CleanupDestroyedEntries();
            if (windowStack.Count == 0)
                return;

            var top = windowStack.Pop();
            if (top != null)
            {
                top.Hide();
                WindowClosed?.Invoke(top);
            }

            while (windowStack.Count > 0 && windowStack.Peek() == null)
                windowStack.Pop();
        }

        public void CloseTopPopup()
        {
            CleanupDestroyedEntries();
            if (popupStack.Count == 0) return;

            var topEntry = popupStack.Pop();
            if (topEntry.window != null)
            {
                topEntry.window.Hide();
                WindowClosed?.Invoke(topEntry.window);
            }

            while (popupStack.Count > 0 && popupStack.Peek().window == null)
                popupStack.Pop();

            if (popupStack.Count > 0)
            {
                var prevEntry = popupStack.Peek();
                if (prevEntry.window != null)
                    prevEntry.window.Show(prevEntry.data);
            }
        }

        public void Close<T>() where T : Window
        {
            CleanupDestroyedEntries();
            var type = typeof(T);
            if (!cache.TryGetValue(type, out var window) || window == null) return;

            bool isPopup = typeof(PopupWindow).IsAssignableFrom(type);
            if (isPopup)
            {
                if (popupStack.Count > 0 && ReferenceEquals(popupStack.Peek().window, window))
                {
                    CloseTopPopup();
                    return;
                }
                RemovePopupFromStack(window);
                window.Hide();
                WindowClosed?.Invoke(window);
            }
            else
            {
                if (windowStack.Count > 0 && ReferenceEquals(windowStack.Peek(), window))
                {
                    CloseTop();
                    return;
                }
                RemoveWindowFromStack(window);
                window.Hide();
                WindowClosed?.Invoke(window);
            }
        }

        public bool IsOpen<T>() where T : Window
        {
            var type = typeof(T);
            return cache.TryGetValue(type, out var window) && window != null && window.IsVisible;
        }

        public bool HasOpenedPopup()
        {
            CleanupDestroyedEntries();
            return popupStack.Count > 0;
        }

        public void CloseAll()
        {
            CloseAllWindows();
            CloseAllPopups();
        }

        public void CloseAllWindows()
        {
            while (windowStack.Count > 0)
            {
                var window = windowStack.Pop();
                if (window != null)
                {
                    window.Hide();
                    WindowClosed?.Invoke(window);
                }
            }
        }

        public void CloseAllPopups()
        {
            while (popupStack.Count > 0)
            {
                var popupEntry = popupStack.Pop();
                if (popupEntry.window != null)
                {
                    popupEntry.window.Hide();
                    WindowClosed?.Invoke(popupEntry.window);
                }
            }
        }

        private void CleanupDestroyedEntries()
        {
            var deadTypes = new List<Type>();
            foreach (var kv in cache)
            {
                if (kv.Value == null)
                    deadTypes.Add(kv.Key);
            }

            foreach (var type in deadTypes)
                cache.Remove(type);

            RemoveDestroyedWindowsFromStack();
            RemoveDestroyedPopupsFromStack();
        }

        private void RemoveDestroyedWindowsFromStack()
        {
            if (windowStack.Count == 0)
                return;

            var temp = new List<Window>(windowStack.Count);
            while (windowStack.Count > 0)
            {
                var current = windowStack.Pop();
                if (current != null)
                    temp.Add(current);
            }

            for (int i = temp.Count - 1; i >= 0; i--)
                windowStack.Push(temp[i]);
        }

        private void RemoveDestroyedPopupsFromStack()
        {
            if (popupStack.Count == 0)
                return;

            var temp = new List<PopupEntry>(popupStack.Count);
            while (popupStack.Count > 0)
            {
                var current = popupStack.Pop();
                if (current.window != null)
                    temp.Add(current);
            }

            for (int i = temp.Count - 1; i >= 0; i--)
                popupStack.Push(temp[i]);
        }

        private void RemoveWindowFromStack(Window target)
        {
            if (windowStack.Count == 0)
                return;

            var temp = new List<Window>(windowStack.Count);
            while (windowStack.Count > 0)
            {
                var current = windowStack.Pop();
                if (!ReferenceEquals(current, target))
                    temp.Add(current);
            }

            for (int i = temp.Count - 1; i >= 0; i--)
                windowStack.Push(temp[i]);
        }

        private void RemovePopupFromStack(Window target)
        {
            if (popupStack.Count == 0)
                return;

            var temp = new List<PopupEntry>(popupStack.Count);
            while (popupStack.Count > 0)
            {
                var current = popupStack.Pop();
                if (!ReferenceEquals(current.window, target))
                    temp.Add(current);
            }

            for (int i = temp.Count - 1; i >= 0; i--)
                popupStack.Push(temp[i]);
        }

        private T CreateWindow<T>() where T : Window
        {
            bool isPopup = typeof(PopupWindow).IsAssignableFrom(typeof(T));
            var parent = isPopup ? popupsRoot : screensRoot;
            return Factory.Create<T>(parent);
        }

        public T Get<T>() where T : Window
        {
            CleanupDestroyedEntries();

            var type = typeof(T);

            if (!cache.TryGetValue(type, out var window) || window == null)
            {
                window = CreateWindow<T>();
                cache[type] = window;

                window.PreloadAndDisable();
            }

            return (T)window;
        }
    }
}
