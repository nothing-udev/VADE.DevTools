using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace VADE.DevTools.UI
{

    [RequireComponent(typeof(Toggle))]
    public class ToggleInput : MonoBehaviour
    {
        [Serializable]
        public class ToggleEvent : UnityEvent<bool> { }

        public ToggleEvent toggleOutputEvent = new();

        private Toggle _toggle;

        public bool IsOn => _toggle != null ? _toggle.isOn : false;

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(bool value) => toggleOutputEvent.Invoke(value);

        public void SetIsOn(bool value, bool notify = true)
        {
            if (_toggle == null)
                _toggle = GetComponent<Toggle>();

            if (notify)
                _toggle.isOn = value;
            else
                _toggle.SetIsOnWithoutNotify(value);
        }

        private void OnDestroy()
        {
            if (_toggle != null)
                _toggle.onValueChanged.RemoveListener(OnValueChanged);
        }
    }
}
