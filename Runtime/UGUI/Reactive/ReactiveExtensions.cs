using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VADE.DevTools.Reactive
{
    public static class ReactiveExtensions
    {
        public static IDisposable BindTo(this Reactive<string> prop, Text text)
        {
            void Update(string v) => text.text = v;
            prop.OnChanged += Update;
            text.text = prop.value;
            return new Subscription(() => prop.OnChanged -= Update);
        }

        public static IDisposable BindTo(this Reactive<int> prop, Text text)
        {
            void Update(int v) => text.text = v.ToString();
            prop.OnChanged += Update;
            text.text = prop.value.ToString();
            return new Subscription(() => prop.OnChanged -= Update);
        }

        public static IDisposable BindTo<T>(this Reactive<T> prop, TMP_Text text)
        {
            void Update(T v) => text.text = v?.ToString();

            prop.OnChanged += Update;
            text.text = prop.value?.ToString();

            return new Subscription(() => prop.OnChanged -= Update);
        }

        public static IDisposable BindTo(this Reactive<float> prop, Slider slider)
        {
            void Update(float v) => slider.value = v;
            prop.OnChanged += Update;
            slider.value = prop.value;
            return new Subscription(() => prop.OnChanged -= Update);
        }

        public static IDisposable BindTo(this Reactive<Color> prop, Image image)
        {
            void Update(Color v) => image.color = v;
            prop.OnChanged += Update;
            image.color = prop.value;
            return new Subscription(() => prop.OnChanged -= Update);
        }

        public static IDisposable BindTo(this Reactive<Sprite> prop, Image image)
        {
            void Update(Sprite v) => image.sprite = v;
            prop.OnChanged += Update;
            image.sprite = prop.value;
            return new Subscription(() => prop.OnChanged -= Update);
        }

        public static IDisposable BindTo(this Reactive<bool> prop, Button button)
        {
            void Update(bool v) => button.interactable = v;
            prop.OnChanged += Update;
            button.interactable = prop.value;
            return new Subscription(() => prop.OnChanged -= Update);
        }

        public static IDisposable BindTwoWay(this Reactive<string> prop, TMP_InputField inputField)
        {
            void UpdateField(string v)
            {
                if (inputField.text != v)
                    inputField.text = v;
            }

            void UpdateProp(string v) => prop.value = v;

            prop.OnChanged += UpdateField;
            inputField.text = prop.value;
            inputField.onValueChanged.AddListener(UpdateProp);

            return new Subscription(() =>
            {
                prop.OnChanged -= UpdateField;
                inputField.onValueChanged.RemoveListener(UpdateProp);
            });
        }

        public static IDisposable Subscribe(this Button button, Action action)
        {
            UnityEngine.Events.UnityAction unityAction = () => action();

            button.onClick.AddListener(unityAction);
            return new Subscription(() => button.onClick.RemoveListener(unityAction));
        }

        public static IDisposable Subscribe(this TMP_InputField inputField, Action<string> onChanged)
        {
            UnityEngine.Events.UnityAction<string> unityAction = (val) => onChanged(val);

            inputField.onValueChanged.AddListener(unityAction);
            return new Subscription(() => inputField.onValueChanged.RemoveListener(unityAction));
        }

        internal sealed class Subscription : IDisposable
        {
            private Action _unsubscribe;
            public Subscription(Action unsubscribe) => _unsubscribe = unsubscribe;
            public void Dispose()
            {
                _unsubscribe?.Invoke();
                _unsubscribe = null;
            }
        }
    }
}
