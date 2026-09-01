using UnityEngine;

namespace VADE.DevTools.Persistence
{
    public class PlayerPrefsAutoSaveStorage : IAutoSaveStorage
    {
        public bool HasKey(string key) => PlayerPrefs.HasKey(key);

        public void Write(string key, string serialized)
        {
            PlayerPrefs.SetString(key, serialized);
            PlayerPrefs.Save();
        }

        public string Read(string key) => PlayerPrefs.GetString(key, null);

        public void Delete(string key) => PlayerPrefs.DeleteKey(key);
    }
}
