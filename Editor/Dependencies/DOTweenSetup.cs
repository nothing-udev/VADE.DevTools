using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VADE.DevTools.Editor.Utils;

namespace VADE.DevTools.Editor.Dependencies
{

    public static class DOTweenSetup
    {
        private const string DOTweenTypeName = "DG.Tweening.DOTween";
        private const string DefineSymbol = "VADE_DOTWEEN";
        private const string RuntimeAsmdefName = "VADE.DevTools.UGUI";

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

        [MenuItem("Tools/VADE/Dependencies/Enable DOTween Support")]
        public static void EnableDOTween()
        {
            var doTweenType = OptionalTypeUtility.FindType(DOTweenTypeName);

            if (doTweenType == null)
            {
                bool openDownload = EditorUtility.DisplayDialog(
                    "DOTween не найден",
                    "В проекте не найден DG.Tweening.DOTween. У DOTween нет единого " +
                    "официального UPM-пакета — обычно его ставят с Asset Store или сайта " +
                    "разработчика (dotween.demigiant.com).\n\n" +
                    "Установите DOTween, запустите Tools > Demigiant > DOTween Utility Panel > " +
                    "\"Setup DOTween...\", затем повторите эту команду.\n\nОткрыть страницу загрузки?",
                    "Открыть", "Отмена");

                if (openDownload)
                    Application.OpenURL("https://dotween.demigiant.com/download.php");
                return;
            }

            string assemblyName = doTweenType.Assembly.GetName().Name;

            if (assemblyName.StartsWith("Assembly-CSharp"))
            {
                bool openPanel = EditorUtility.DisplayDialog(
                    "У DOTween нет своей сборки (asmdef)",
                    "DOTween найден, но скомпилирован прямо в " + assemblyName + " — сослаться на " +
                    "него из отдельного пакета (asmdef) нельзя, пока у него нет своей Assembly Definition.\n\n" +
                    "Откройте Tools > Demigiant > DOTween Utility Panel и нажмите \"Create ASMDEF...\", " +
                    "затем повторите эту команду.\n\nОткрыть панель DOTween сейчас?",
                    "Открыть", "Отмена");

                if (openPanel)
                    EditorApplication.ExecuteMenuItem("Tools/Demigiant/DOTween Utility Panel");
                return;
            }

            if (!TryAddAsmdefReference(assemblyName, out string error))
            {
                Debug.LogError(
                    $"[VADE.DevTools] Не удалось прописать ссылку на '{assemblyName}' в asmdef: {error}\n" +
                    "Добавьте вручную: выделите Runtime/UGUI/VADE.DevTools.UGUI.asmdef -> Assembly Definition " +
                    "References -> '+' -> выбрать сборку DOTween.");
                return;
            }

            AddDefineSymbol();

            EditorUtility.DisplayDialog(
                "DOTween подключен",
                $"В VADE.DevTools.UGUI.asmdef добавлена ссылка на сборку '{assemblyName}'.\n" +
                $"В Scripting Define Symbols добавлен {DefineSymbol}.\n\n" +
                "Unity сейчас перекомпилирует скрипты.",
                "Ок");
        }

        [MenuItem("Tools/VADE/Dependencies/Disable DOTween Support")]
        public static void DisableDOTween()
        {
            RemoveDefineSymbol();
            Debug.Log($"[VADE.DevTools] {DefineSymbol} убран из Scripting Define Symbols. " +
                      "Ссылку на сборку DOTween в asmdef (если добавлялась) можно оставить — она не мешает, " +
                      "но при желании уберите её вручную в инспекторе Runtime/UGUI/VADE.DevTools.UGUI.asmdef.");
        }

        private static string FindRuntimeAsmdefPath()
        {
            var guids = AssetDatabase.FindAssets($"{RuntimeAsmdefName} t:AssemblyDefinitionAsset");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == RuntimeAsmdefName)
                    return path;
            }
            return null;
        }

        private static bool TryAddAsmdefReference(string assemblyName, out string error)
        {
            error = null;
            string path = FindRuntimeAsmdefPath();
            if (path == null)
            {
                error = $"не найден {RuntimeAsmdefName}.asmdef через AssetDatabase";
                return false;
            }

            string json;
            try
            {
                json = File.ReadAllText(path);
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }

            AsmdefModel model;
            try
            {
                model = JsonUtility.FromJson<AsmdefModel>(json);
            }
            catch (Exception e)
            {
                error = $"не удалось разобрать JSON: {e.Message}";
                return false;
            }

            var refs = new List<string>(model.references ?? Array.Empty<string>());
            if (!refs.Contains(assemblyName))
            {
                refs.Add(assemblyName);
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
            }

            return true;
        }

        private static void AddDefineSymbol() => SetDefineSymbol(add: true);
        private static void RemoveDefineSymbol() => SetDefineSymbol(add: false);

        private static void SetDefineSymbol(bool add)
        {
#pragma warning disable CS0618
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            string existing = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            var symbols = existing.Split(';').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();

            bool changed = false;
            if (add && !symbols.Contains(DefineSymbol))
            {
                symbols.Add(DefineSymbol);
                changed = true;
            }
            else if (!add && symbols.Remove(DefineSymbol))
            {
                changed = true;
            }

            if (changed)
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", symbols));
#pragma warning restore CS0618

            Debug.Log($"[VADE.DevTools] {DefineSymbol} обновлён только для текущей платформы " +
                      $"({group}). Для других build targets (если собираете под них) проверьте " +
                      "Player Settings -> Other Settings -> Scripting Define Symbols отдельно.");
        }
    }
}
