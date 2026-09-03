using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using VADE.DevTools.Editor.Utils;

namespace VADE.DevTools.Editor.Dependencies
{
    public static class IAPSetup
    {
        private const string DefineSymbol = "VADE_IAP";
        private const string CoreAsmdefName = "VADE.DevTools.Core";
        private const string EditorAsmdefName = "VADE.DevTools.Editor";

        private static readonly string[] PackageIds =
        {
            "com.unity.services.core",
            "com.unity.purchasing"
        };

        private static readonly Queue<string> pendingPackages = new();
        private static AddRequest currentRequest;

        [MenuItem("Tools/VADE/Dependencies/Enable IAP Support")]
        public static void EnableIAP()
        {
            if (OptionalTypeUtility.FindType("UnityEngine.Purchasing.ProductType") != null)
            {
                FinishSetup();
                return;
            }

            pendingPackages.Clear();
            foreach (var id in PackageIds)
                pendingPackages.Enqueue(id);

            InstallNext();
        }

        [MenuItem("Tools/VADE/Dependencies/Disable IAP Support")]
        public static void DisableIAP()
        {
            AsmdefPatcher.SetDefine(DefineSymbol, false);
            Debug.Log($"[VADE.DevTools] {DefineSymbol} убран из Scripting Define Symbols.");
        }

        private static void InstallNext()
        {
            if (pendingPackages.Count == 0)
            {
                FinishSetup();
                return;
            }

            string id = pendingPackages.Dequeue();
            Debug.Log($"[VADE.DevTools] Устанавливаю {id}...");
            currentRequest = Client.Add(id);
            EditorApplication.update += TrackRequest;
        }

        private static void TrackRequest()
        {
            if (currentRequest == null || !currentRequest.IsCompleted) return;
            EditorApplication.update -= TrackRequest;

            if (currentRequest.Status != StatusCode.Success)
            {
                Debug.LogError($"[VADE.DevTools] Ошибка установки пакета: {currentRequest.Error?.message}");
                currentRequest = null;
                return;
            }

            currentRequest = null;
            InstallNext();
        }

        private static void FinishSetup()
        {
            var assemblyNames = new List<string>();

            foreach (var typeName in new[]
            {
                "UnityEngine.Purchasing.ProductType",
                "Unity.Services.Core.UnityServices",
                "UnityEngine.Purchasing.Security.CrossPlatformValidator"
            })
            {
                var type = OptionalTypeUtility.FindType(typeName);
                if (type != null && !assemblyNames.Contains(type.Assembly.GetName().Name))
                    assemblyNames.Add(type.Assembly.GetName().Name);
            }

            if (assemblyNames.Count == 0)
            {
                Debug.LogError("[VADE.DevTools] Не удалось найти сборки Unity IAP после установки пакетов.");
                return;
            }

            if (!AsmdefPatcher.TryAddReferences(CoreAsmdefName, assemblyNames, out string error))
            {
                Debug.LogError($"[VADE.DevTools] Не удалось прописать ссылки {string.Join(", ", assemblyNames)} в {CoreAsmdefName}.asmdef: {error}\n" +
                                $"Добавьте вручную через инспектор Runtime/Core/{CoreAsmdefName}.asmdef.");
                return;
            }

            if (!AsmdefPatcher.TryAddReferences(EditorAsmdefName, assemblyNames, out string editorError))
            {
                Debug.LogError($"[VADE.DevTools] Не удалось прописать ссылки {string.Join(", ", assemblyNames)} в {EditorAsmdefName}.asmdef: {editorError}\n" +
                                $"Добавьте вручную через инспектор Editor/{EditorAsmdefName}.asmdef.");
                return;
            }

            AsmdefPatcher.SetDefine(DefineSymbol, true);

            Debug.Log($"[VADE.DevTools] IAP включён: {DefineSymbol}, ссылки [{string.Join(", ", assemblyNames)}] добавлены в {CoreAsmdefName}.asmdef и {EditorAsmdefName}.asmdef. " +
                      "Не забудьте настроить продукты в Unity Dashboard и (для валидации чеков) Window > Unity IAP > IAP Receipt Validation Obfuscator.");
        }
    }
}
