using System;
using UnityEditor;
using UnityEngine;
using VADE.DevTools.Editor.Bootstrap;
using VADE.DevTools.Editor.Utils;

namespace VADE.DevTools.Editor.Setup
{
    public class VADESetupWindow : EditorWindow
    {
        [MenuItem("Tools/VADE/Setup Window")]
        private static void Open() => GetWindow<VADESetupWindow>("VADE Setup").Show();

        private Vector2 scroll;

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);

            EditorGUILayout.LabelField("Core", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Reactive, DI, Bootstrap, AutoSave, Extensions — всегда доступны, отдельной установки не требуют.", MessageType.None);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Сцена", EditorStyles.boldLabel);
            DrawActionRow("Bootstrap + WindowService + EventSystem", HasSceneObjects(), "Установить в сцену",
                SceneBootstrapSetup.SetupCurrentScene);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Опциональные зависимости", EditorStyles.boldLabel);

            DrawToggleRow("DOTween (анимации окон)", AsmdefPatcher.HasDefine("VADE_DOTWEEN"),
                Dependencies.DOTweenSetup.EnableDOTween, Dependencies.DOTweenSetup.DisableDOTween);

            DrawToggleRow("In-App Purchases", AsmdefPatcher.HasDefine("VADE_IAP"),
                Dependencies.IAPSetup.EnableIAP, Dependencies.IAPSetup.DisableIAP);

            DrawToggleRow("LevelPlay Ads", AsmdefPatcher.HasDefine("VADE_LEVELPLAY"),
                Dependencies.LevelPlaySetup.EnableLevelPlay, Dependencies.LevelPlaySetup.DisableLevelPlay);

            EditorGUILayout.EndScrollView();
        }

        private void DrawActionRow(string label, bool done, string buttonLabel, Action action)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawStatusDot(done);
                GUILayout.Label(label);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(buttonLabel, GUILayout.Width(140)))
                    action();
            }
        }

        private void DrawToggleRow(string label, bool enabled, Action onEnable, Action onDisable)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawStatusDot(enabled);
                GUILayout.Label(label);
                GUILayout.FlexibleSpace();

                GUI.enabled = !enabled;
                if (GUILayout.Button("Install", GUILayout.Width(70))) onEnable();
                GUI.enabled = enabled;
                if (GUILayout.Button("Remove", GUILayout.Width(70))) onDisable();
                GUI.enabled = true;
            }
        }

        private void DrawStatusDot(bool on)
        {
            var prevColor = GUI.color;
            GUI.color = on ? new Color(0.45f, 0.85f, 0.45f) : new Color(0.6f, 0.6f, 0.6f);
            GUILayout.Label(on ? "●" : "○", GUILayout.Width(16));
            GUI.color = prevColor;
        }

        private static bool HasSceneObjects()
        {
#if UNITY_2022_2_OR_NEWER
            return Object.FindAnyObjectByType<VADE.DevTools.UI.WindowService>() != null;
#else
            return Object.FindObjectOfType<VADE.DevTools.UI.WindowService>() != null;
#endif
        }
    }
}
