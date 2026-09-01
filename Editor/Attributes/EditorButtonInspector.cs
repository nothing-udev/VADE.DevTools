using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VADE.DevTools.Attributes;

namespace VADE.DevTools.Editor.Attributes
{

    internal static class EditorButtonDrawer
    {
        private static readonly Dictionary<(object target, MethodInfo method), object[]> _paramValues = new();

        public static void DrawButtons(Object target)
        {
            var methods = target.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttribute<EditorButtonAttribute>() != null);

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<EditorButtonAttribute>();

                bool enabled = attr.Mode switch
                {
                    ButtonMode.EditModeOnly => !Application.isPlaying,
                    ButtonMode.PlayModeOnly => Application.isPlaying,
                    _ => true
                };

                string label = string.IsNullOrEmpty(attr.Label)
                    ? ObjectNames.NicifyVariableName(method.Name)
                    : attr.Label;

                var parameters = method.GetParameters();

                using (new EditorGUI.DisabledScope(!enabled))
                {
                    if (parameters.Length == 0)
                    {
                        if (GUILayout.Button(label))
                            method.Invoke(target, null);
                        continue;
                    }

                    if (!parameters.All(p => IsSupportedParamType(p.ParameterType)))
                    {
                        using (new EditorGUI.DisabledScope(true))
                            GUILayout.Button($"{label} (неподдерживаемые параметры)");
                        continue;
                    }

                    var key = (target, method);
                    if (!_paramValues.TryGetValue(key, out var values) || values.Length != parameters.Length)
                    {
                        values = parameters.Select(p => DefaultFor(p.ParameterType)).ToArray();
                        _paramValues[key] = values;
                    }

                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    for (int i = 0; i < parameters.Length; i++)
                        values[i] = DrawField(parameters[i], values[i]);

                    if (GUILayout.Button(label))
                        method.Invoke(target, values);
                    EditorGUILayout.EndVertical();
                }
            }
        }

        private static bool IsSupportedParamType(Type t) =>
            t == typeof(int) || t == typeof(float) || t == typeof(string) || t == typeof(bool);

        private static object DefaultFor(Type t)
        {
            if (t == typeof(int)) return 0;
            if (t == typeof(float)) return 0f;
            if (t == typeof(bool)) return false;
            return string.Empty;
        }

        private static object DrawField(ParameterInfo p, object value)
        {
            string label = ObjectNames.NicifyVariableName(p.Name);
            if (p.ParameterType == typeof(int)) return EditorGUILayout.IntField(label, (int)value);
            if (p.ParameterType == typeof(float)) return EditorGUILayout.FloatField(label, (float)value);
            if (p.ParameterType == typeof(bool)) return EditorGUILayout.Toggle(label, (bool)value);
            return EditorGUILayout.TextField(label, (string)value);
        }
    }

    [CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
    public class EditorButtonMonoBehaviourEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorButtonDrawer.DrawButtons(target);
        }
    }

    [CustomEditor(typeof(ScriptableObject), true, isFallback = true)]
    public class EditorButtonScriptableObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorButtonDrawer.DrawButtons(target);
        }
    }
}
