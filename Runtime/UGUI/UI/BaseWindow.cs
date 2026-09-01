using UnityEngine;
using UnityEngine.UI;
using VADE.DevTools.Reactive;

namespace VADE.DevTools.UI
{
    public abstract class BaseWindow : Window
    {
        [Header("Buttons")]
        [SerializeField] protected Button closeButton;

        protected override void OnShow(object data)
        {
            if (closeButton != null)
                Connections += closeButton.Subscribe(() => WindowService.Instance.CloseTop());
        }
    }
}
