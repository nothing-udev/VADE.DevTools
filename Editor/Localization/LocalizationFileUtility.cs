using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace VADE.DevTools.Editor.Localization
{

    internal static class LocalizationFileUtility
    {

        public static Dictionary<string, string> FindLanguageFiles()
        {
            var result = new Dictionary<string, string>();

            foreach (var guid in AssetDatabase.FindAssets("t:TextAsset"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid).Replace('\\', '/');
                if (!path.Contains("/Resources/Localization/") || !path.EndsWith(".json")) continue;

                string code = Path.GetFileNameWithoutExtension(path);
                result[code] = path;
            }

            return result;
        }

        public static Dictionary<string, Dictionary<string, string>> LoadAllTables(out Dictionary<string, string> filePaths)
        {
            filePaths = FindLanguageFiles();
            var tables = new Dictionary<string, Dictionary<string, string>>();

            foreach (var kv in filePaths)
            {
                try
                {
                    string json = File.ReadAllText(kv.Value);
                    tables[kv.Key] = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[VADE.Localization] Не удалось разобрать {kv.Value}: {e.Message}");
                    tables[kv.Key] = new Dictionary<string, string>();
                }
            }

            return tables;
        }

        public static void SaveTable(string path, Dictionary<string, string> table)
        {
            string json = JsonConvert.SerializeObject(table, Formatting.Indented);
            File.WriteAllText(path, json);
            AssetDatabase.ImportAsset(path);
        }

        public static string[] GetAllKeysSorted()
        {
            var tables = LoadAllTables(out _);
            var keys = new SortedSet<string>(StringComparer.Ordinal);
            foreach (var table in tables.Values)
                foreach (var key in table.Keys)
                    keys.Add(key);

            var arr = new string[keys.Count];
            keys.CopyTo(arr);
            return arr;
        }
    }
}
