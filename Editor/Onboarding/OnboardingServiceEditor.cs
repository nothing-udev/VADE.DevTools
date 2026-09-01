using UnityEditor;
using UnityEngine;
using VADE.DevTools.Editor.Attributes;
using VADE.DevTools.Onboarding;

namespace VADE.DevTools.Editor.Onboarding
{
    [CustomEditor(typeof(OnboardingService))]
    public class OnboardingServiceEditor : UnityEditor.Editor
    {
        private UnityEditor.Editor assetEditor;
        private bool foldout;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var onboardingProp = serializedObject.FindProperty("onboarding");
            EditorGUILayout.PropertyField(onboardingProp);

            if (onboardingProp.objectReferenceValue != null)
            {
                foldout = EditorGUILayout.InspectorTitlebar(foldout, onboardingProp.objectReferenceValue);
                if (foldout)
                {
                    UnityEditor.Editor.CreateCachedEditor(onboardingProp.objectReferenceValue, null, ref assetEditor);
                    EditorGUI.indentLevel++;
                    assetEditor.OnInspectorGUI();
                    EditorGUI.indentLevel--;
                }
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            EditorButtonDrawer.DrawButtons(target);

            if (Application.isPlaying && target is OnboardingService service)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(service.GetDebugInfo(), MessageType.None);
                Repaint();
            }
        }
    }
}
