using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VADE.DevTools.Editor.Utils
{
    internal static class AsmdefPatcher
    {
        [Serializable]
        private class AsmdefModel
        {
            public string name;
            public string rootNamespace;
            public string[] references;
            public string[] includePlatforms;
            public string[] excludePlatforms;
            public bool allowUnsafeCode;
            public bool overrideReferences;
            public string[] precompiledReferences;
            public bool autoReferenced;
            public string[] defineConstraints;
            public string[] versionDefines;
            public bool noEngineReferences;
        }

        public static string FindAsmdefPath(string asmdefName)
        {
            foreach (var guid in AssetDatabase.FindAssets($"{asmdefName} t:AssemblyDefinitionAsset"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == asmdefName)
                    return path;
            }
            return null;
        }

        public static bool TryAddReferences(string asmdefName, IEnumerable<string> assemblyNames, out string error)
        {
            error = null;
            string path = FindAsmdefPath(asmdefName);
            if (path == null)
            {
                error = $"не найден {asmdefName}.asmdef";
                return false;
            }

            AsmdefModel model;
            try
            {
                model = JsonUtility.FromJson<AsmdefModel>(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }

            var refs = new List<string>(model.references ?? Array.Empty<string>());
            bool changed = false;
            foreach (var name in assemblyNames)
            {
                if (refs.Contains(name)) continue;
                refs.Add(name);
                changed = true;
            }

            if (!changed) return true;

            model.references = refs.ToArray();
            try
            {
                File.WriteAllText(path, JsonUtility.ToJson(model, true));
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }

            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();
            return true;
        }

        public static void SetDefine(string symbol, bool enabled)
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
#pragma warning disable CS0618
            string existing = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            var symbols = existing.Split(';').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();

            bool changed = false;
            if (enabled && !symbols.Contains(symbol)) { symbols.Add(symbol); changed = true; }
            if (!enabled && symbols.Remove(symbol)) changed = true;

            if (changed)
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", symbols));
#pragma warning restore CS0618
        }

        public static bool HasDefine(string symbol)
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
#pragma warning disable CS0618
            string existing = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
#pragma warning restore CS0618
            return existing.Split(';').Select(s => s.Trim()).Contains(symbol);
        }
    }
}
