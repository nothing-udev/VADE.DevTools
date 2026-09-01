using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using VADE.DevTools.DI;
using VADE.DevTools.Persistence;
using VADE.DevTools.Reactive;

namespace VADE.DevTools.Localization
{
    [Serializable]
    public class LanguageConfig
    {
        public string code;
        public Sprite icon;
    }

    public class LocalizationService : MonoBehaviour
    {
        public static LocalizationService Instance { get; private set; }

        [SerializeField] private List<LanguageConfig> languageConfigs = new();
        public IReadOnlyList<LanguageConfig> LanguageConfigs => languageConfigs;

        public readonly Reactive<string> CurrentLanguage = new(null);

        private Dictionary<string, string> localizedText = new();
        private AutoSave<string> savedLanguage;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Dependency.Register(this);

            savedLanguage = new AutoSave<string>("vade_language", AutoSaveType.PlayerPrefs, defaultValue: null);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                if (Dependency.IsRegistered<LocalizationService>())
                    Dependency.Unregister<LocalizationService>();
            }
        }

        private void Start()
        {
            string lang = string.IsNullOrEmpty(savedLanguage.value) ? GetDefaultDeviceLanguage() : savedLanguage.value;
            SetLanguage(lang);
        }

        public string GetDefaultDeviceLanguage()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Russian: return "ru";
                case SystemLanguage.Ukrainian: return "uk";
                case SystemLanguage.German: return "de";
                case SystemLanguage.French: return "fr";
                case SystemLanguage.English: return "en";
                case SystemLanguage.Portuguese: return "pt-BR";
                case SystemLanguage.Thai: return "th";
                case SystemLanguage.Indonesian: return "id";
                case SystemLanguage.Chinese: return "ch";
                case SystemLanguage.Japanese: return "ja";
                default: return "en";
            }
        }

        public void SetLanguage(string languageCode)
        {
            TextAsset jsonFile = Resources.Load<TextAsset>($"Localization/{languageCode}");

            if (jsonFile == null)
            {
                Debug.LogError($"[LocalizationService] Файл не найден: Resources/Localization/{languageCode}.json");
                return;
            }

            try
            {
                localizedText = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonFile.text) ?? new Dictionary<string, string>();
                savedLanguage.value = languageCode;
                CurrentLanguage.value = languageCode;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalizationService] Ошибка разбора JSON ({languageCode}): {e.Message}");
            }
        }

        public string GetText(string key)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            if (localizedText != null && localizedText.TryGetValue(key, out string value))
                return value;

            Debug.LogWarning($"[LocalizationService] Ключ не найден: '{key}'");
            return $"#{key}#";
        }
    }
}
