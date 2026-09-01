using UnityEngine;
using UnityEngine.UI;
using VADE.DevTools.Reactive;

namespace VADE.DevTools.UI
{
    public abstract class PopupWindow : Window
    {
        [Header("Buttons")]
        [SerializeField] private Button backgroundButton;
        [SerializeField] protected Button closeButton;

        protected override void OnShow(object data)
        {
            if (backgroundButton != null)
                Connections += backgroundButton.Subscribe(() => WindowService.Instance.CloseTopPopup());

            if (closeButton != null)
                Connections += closeButton.Subscribe(() => WindowService.Instance.CloseTopPopup());
        }

        public virtual void OnAnimationFinished() { }
    }
}
