using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VADE.DevTools.Reactive;

namespace VADE.DevTools.Onboarding
{
    public class UiClickComponent : TaskComponentBase, IVisibleComponent
    {
        [SerializeField] private List<Object> viewComponents = new();
        [SerializeField] private Button eventButton;

        private void Awake() => SetButton(eventButton);

        public void SetButton(Button btn)
        {
            if (btn == null) return;
            Connections += btn.Subscribe(() => TaskEvents.RaiseUiClick(Id));
        }

        public void SetEvent(UnityEvent onClickEvent) =>
            Connections += onClickEvent.Subscribe(() => TaskEvents.RaiseUiClick(Id));

        public void EmulateButtonCall() => TaskEvents.RaiseUiClick(Id);

        public void Show() => SetVisibility(true);
        public void Hide() => SetVisibility(false);

        private void SetVisibility(bool state)
        {
            foreach (var obj in viewComponents)
            {
                if (obj == null) continue;

                switch (obj)
                {
                    case GameObject go:
                        go.SetActive(state);
                        break;
                    case Behaviour behaviour:
                        behaviour.enabled = state;
                        break;
                    default:
                        Debug.LogWarning($"[UiClickComponent] Unsupported type in viewComponents: {obj.GetType()}");
                        break;
                }
            }
        }
    }
}
