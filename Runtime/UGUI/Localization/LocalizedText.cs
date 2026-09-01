using TMPro;
using UnityEngine;
using VADE.DevTools.Reactive;

namespace VADE.DevTools.Localization
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : ReactiveBehaviour
    {
        [SerializeField, LocalizationKey] private string key;

        private TMP_Text textComponent;

        private void Awake() => textComponent = GetComponent<TMP_Text>();

        private void OnEnable()
        {
            if (LocalizationService.Instance != null)
                Connections += LocalizationService.Instance.CurrentLanguage.Subscribe(_ => UpdateText());
        }

        private void OnDisable() => Connections.Dispose();

        public void SetKey(string newKey)
        {
            key = newKey;
            UpdateText();
        }

        private void UpdateText()
        {
            if (LocalizationService.Instance != null && !string.IsNullOrEmpty(key))
                textComponent.text = LocalizationService.Instance.GetText(key);
        }
    }
}
