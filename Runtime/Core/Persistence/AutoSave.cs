using System;
using UnityEngine;
using VADE.DevTools.Reactive;

namespace VADE.DevTools.Persistence
{

    public class AutoSave<T>
    {
        private const string VersionKeySuffix = "__v";

        private readonly string _key;
        private readonly IAutoSaveStorage _storage;
        private readonly Reactive<T> _reactive;
        private readonly int _version;
        private readonly Func<string, int, T> _migrate;

        public string Key => _key;

        public event Action<T> OnChanged
        {
            add => _reactive.OnChanged += value;
            remove => _reactive.OnChanged -= value;
        }

        public T value
        {
            get => _reactive.value;
            set => _reactive.value = value;
        }

        public AutoSave(string key, AutoSaveType type = AutoSaveType.PlayerPrefs, T defaultValue = default,
            IAutoSaveStorage storage = null, int version = 0, Func<string, int, T> migrate = null)
        {
            _key = key;
            _storage = storage ?? AutoSaveStorage.Get(type);
            _version = version;
            _migrate = migrate;

            _reactive = new Reactive<T>(Load(defaultValue));
            _reactive.OnChanged += Save;
        }

        private T Load(T defaultValue)
        {
            if (!_storage.HasKey(_key)) return defaultValue;

            string serialized = _storage.Read(_key);
            if (string.IsNullOrEmpty(serialized)) return defaultValue;

            int savedVersion = ReadSavedVersion();

            try
            {
                if (_migrate != null && savedVersion != _version)
                    return _migrate(serialized, savedVersion);

                return AutoSaveSerializer.Current.Deserialize<T>(serialized);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AutoSave] Не удалось загрузить '{_key}': {e.Message}. Использую значение по умолчанию.");
                return defaultValue;
            }
        }

        private int ReadSavedVersion()
        {
            string raw = _storage.Read(_key + VersionKeySuffix);
            return int.TryParse(raw, out int v) ? v : 0;
        }

        private void Save(T value)
        {
            try
            {
                _storage.Write(_key, AutoSaveSerializer.Current.Serialize(value));
                if (_version != 0)
                    _storage.Write(_key + VersionKeySuffix, _version.ToString());
            }
            catch (Exception e)
            {
                Debug.LogError($"[AutoSave] Не удалось сохранить '{_key}': {e.Message}");
            }
        }

        public void Delete()
        {
            _storage.Delete(_key);
            _storage.Delete(_key + VersionKeySuffix);
        }

        public void Flush() => Save(value);

        public IDisposable Subscribe(Action<T> onChanged) => _reactive.Subscribe(onChanged);

        public override string ToString() => _reactive.ToString();

        public static implicit operator T(AutoSave<T> autoSave) => autoSave.value;
    }
}
