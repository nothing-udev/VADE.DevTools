using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using VADE.DevTools.Editor.Utils;

namespace VADE.DevTools.Editor.Dependencies
{
    public static class LevelPlaySetup
    {
        private const string PackageId = "com.unity.services.levelplay";
        private const string DefineSymbol = "VADE_LEVELPLAY";
        private const string CoreAsmdefName = "VADE.DevTools.Core";

        private static AddRequest addRequest;

        [MenuItem("Tools/VADE/Dependencies/Enable LevelPlay Ads Support")]
        public static void EnableLevelPlay()
        {
            if (OptionalTypeUtility.FindType("Unity.Services.LevelPlay.LevelPlay") != null)
            {
                FinishSetup();
                return;
            }

            Debug.Log($"[VADE.DevTools] Устанавливаю {PackageId}...");
            addRequest = Client.Add(PackageId);
            EditorApplication.update += TrackRequest;
        }

        [MenuItem("Tools/VADE/Dependencies/Disable LevelPlay Ads Support")]
        public static void DisableLevelPlay()
        {
            AsmdefPatcher.SetDefine(DefineSymbol, false);
            Debug.Log($"[VADE.DevTools] {DefineSymbol} убран из Scripting Define Symbols.");
        }

        private static void TrackRequest()
        {
            if (addRequest == null || !addRequest.IsCompleted) return;
            EditorApplication.update -= TrackRequest;

            if (addRequest.Status != StatusCode.Success)
            {
                Debug.LogError($"[VADE.DevTools] Ошибка установки {PackageId}: {addRequest.Error?.message}");
                addRequest = null;
                return;
            }

            addRequest = null;
            FinishSetup();
        }

        private static void FinishSetup()
        {
            var type = OptionalTypeUtility.FindType("Unity.Services.LevelPlay.LevelPlay");
            if (type == null)
            {
                Debug.LogError("[VADE.DevTools] Пакет установлен, но сборка LevelPlay ещё не скомпилирована — повторите команду после перекомпиляции.");
                return;
            }

            string assemblyName = type.Assembly.GetName().Name;

            if (!AsmdefPatcher.TryAddReferences(CoreAsmdefName, new[] { assemblyName }, out string error))
            {
                Debug.LogError($"[VADE.DevTools] Не удалось прописать ссылку на '{assemblyName}' в {CoreAsmdefName}.asmdef: {error}\n" +
                                $"Добавьте вручную через инспектор Runtime/Core/{CoreAsmdefName}.asmdef.");
                return;
            }

            AsmdefPatcher.SetDefine(DefineSymbol, true);

            Debug.Log($"[VADE.DevTools] LevelPlay включён: {DefineSymbol}, ссылка на '{assemblyName}' добавлена в {CoreAsmdefName}.asmdef. " +
                      "Дальше: Ads Mediation > LevelPlay Network Manager — установить нужные сети и выполнить Resolve зависимостей.");
        }
    }
}
