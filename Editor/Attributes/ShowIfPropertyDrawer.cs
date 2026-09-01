using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VADE.DevTools.Attributes;

namespace VADE.DevTools.Editor.Attributes
{

    [CustomPropertyDrawer(typeof(ShowIfAttribute), true)]
    public class ShowIfPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return IsVisible(property) ? EditorGUI.GetPropertyHeight(property, label, true) : 0f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!IsVisible(property)) return;
            EditorGUI.PropertyField(position, property, label, true);
        }

        private bool IsVisible(SerializedProperty property)
        {
            var showIf = (ShowIfAttribute)attribute;
            bool isHideIf = showIf is HideIfAttribute;

            object target = GetTargetObjectWithProperty(property);
            if (target == null) return true;

            object value = GetMemberValue(target, showIf.Condition);
            bool conditionMet = value != null && Equals(value, showIf.CompareValue);

            return isHideIf ? !conditionMet : conditionMet;
        }

        private static readonly System.Collections.Generic.HashSet<string> _warnedMissingMembers = new();

        private static object GetMemberValue(object target, string name)
        {
            var type = target.GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var searchType = type;
            while (searchType != null)
            {
                var field = searchType.GetField(name, flags);
                if (field != null) return field.GetValue(target);

                var property = searchType.GetProperty(name, flags);
                if (property != null) return property.GetValue(target);

                var method = searchType.GetMethod(name, flags, null, Type.EmptyTypes, null);
                if (method != null && method.ReturnType != typeof(void)) return method.Invoke(target, null);

                searchType = searchType.BaseType;
            }

            string warningKey = $"{type.FullName}.{name}";
            if (_warnedMissingMembers.Add(warningKey))
                Debug.LogWarning($"[ShowIf/HideIf] Не найден член '{name}' в '{type.Name}' для условия. Проверьте имя в nameof(...).");

            return null;
        }

        private static object GetTargetObjectWithProperty(SerializedProperty property)
        {
            string path = property.propertyPath.Replace(".Array.data[", "[");
            object obj = property.serializedObject.targetObject;
            var elements = path.Split('.');

            for (int i = 0; i < elements.Length - 1; i++)
            {
                string element = elements[i];
                if (element.Contains("["))
                {
                    string elementName = element.Substring(0, element.IndexOf("["));
                    int index = Convert.ToInt32(element.Substring(element.IndexOf("["))
                        .Replace("[", "").Replace("]", ""));
                    obj = GetIndexedValue(obj, elementName, index);
                }
                else
                {
                    obj = GetFieldOrPropertyValue(obj, element);
                }

                if (obj == null) return null;
            }

            return obj;
        }

        private static object GetFieldOrPropertyValue(object source, string name)
        {
            if (source == null) return null;
            var type = source.GetType();
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

            while (type != null)
            {
                var field = type.GetField(name, flags);
                if (field != null) return field.GetValue(source);

                var property = type.GetProperty(name, flags);
                if (property != null) return property.GetValue(source, null);

                type = type.BaseType;
            }

            return null;
        }

        private static object GetIndexedValue(object source, string name, int index)
        {
            if (GetFieldOrPropertyValue(source, name) is not IEnumerable enumerable) return null;

            var enumerator = enumerable.GetEnumerator();
            for (int i = 0; i <= index; i++)
            {
                if (!enumerator.MoveNext()) return null;
            }
            return enumerator.Current;
        }
    }
}
