using System.IO;
using UnityEngine;

namespace VADE.DevTools.Persistence
{

    public class FileAutoSaveStorage : IAutoSaveStorage
    {
        private readonly string _directory;

        public FileAutoSaveStorage(string directory = null)
        {
            _directory = string.IsNullOrEmpty(directory) ? Application.persistentDataPath : directory;
            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);
        }

        private string PathFor(string key) => Path.Combine(_directory, key + ".json");

        public bool HasKey(string key) => File.Exists(PathFor(key));

        public void Write(string key, string serialized) => File.WriteAllText(PathFor(key), serialized);

        public string Read(string key) => HasKey(key) ? File.ReadAllText(PathFor(key)) : null;

        public void Delete(string key)
        {
            string path = PathFor(key);
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
