using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VADE.DevTools.Attributes;

namespace VADE.DevTools.Editor.Attributes
{
    [CustomPropertyDrawer(typeof(SerializeReferenceListAttribute))]
    public class SerializeReferenceListDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isArray) return EditorGUIUtility.singleLineHeight;

            float lineH = EditorGUIUtility.singleLineHeight;
            float height = lineH + 4f;

            for (int i = 0; i < property.arraySize; i++)
            {
                var element = property.GetArrayElementAtIndex(i);
                height += lineH + 2f;
                height += EditorGUI.GetPropertyHeight(element, GUIContent.none, true) + 4f;
            }

            height += lineH + 6f;
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!property.isArray)
            {
                EditorGUI.LabelField(position, label.text, "SerializeReferenceList требует List<T>");
                return;
            }

            var attr = (SerializeReferenceListAttribute)attribute;
            var types = GetConcreteTypes(attr.BaseType);

            float lineH = EditorGUIUtility.singleLineHeight;
            float y = position.y;

            EditorGUI.LabelField(new Rect(position.x, y, position.width, lineH), label, EditorStyles.boldLabel);
            y += lineH + 4f;

            int indexToRemove = -1;
            int indexToMoveUp = -1;
            int indexToMoveDown = -1;

            for (int i = 0; i < property.arraySize; i++)
            {
                var element = property.GetArrayElementAtIndex(i);
                string typeName = GetShortTypeName(element.managedReferenceFullTypename);

                float btnW = 22f;
                var headerRect = new Rect(position.x, y, position.width, lineH);
                var typeLabelRect = new Rect(headerRect.x, headerRect.y, headerRect.width - btnW * 4.5f - 8f, lineH);
                var changeTypeRect = new Rect(typeLabelRect.xMax + 2f, headerRect.y, btnW * 1.5f, lineH);
                var upRect = new Rect(changeTypeRect.xMax + 2f, headerRect.y, btnW, lineH);
                var downRect = new Rect(upRect.xMax + 2f, headerRect.y, btnW, lineH);
                var removeRect = new Rect(downRect.xMax + 2f, headerRect.y, btnW, lineH);

                EditorGUI.LabelField(typeLabelRect, string.IsNullOrEmpty(typeName) ? "<none>" : typeName, EditorStyles.miniBoldLabel);

                int capturedIndex = i;
                if (GUI.Button(changeTypeRect, "▾", EditorStyles.miniButton))
                    ShowTypeMenu(types, changeTypeRect, selected => AssignType(property, capturedIndex, selected));

                using (new EditorGUI.DisabledScope(i == 0))
                    if (GUI.Button(upRect, "↑", EditorStyles.miniButtonLeft)) indexToMoveUp = i;

                using (new EditorGUI.DisabledScope(i == property.arraySize - 1))
                    if (GUI.Button(downRect, "↓", EditorStyles.miniButtonMid)) indexToMoveDown = i;

                if (GUI.Button(removeRect, "x", EditorStyles.miniButtonRight)) indexToRemove = i;

                y += lineH + 2f;

                float elementHeight = EditorGUI.GetPropertyHeight(element, GUIContent.none, true);
                var elementRect = new Rect(position.x + 12f, y, position.width - 12f, elementHeight);
                EditorGUI.PropertyField(elementRect, element, GUIContent.none, true);
                y += elementHeight + 4f;
            }

            var addRect = new Rect(position.x, y, position.width, lineH);
            if (GUI.Button(addRect, $"+ Добавить {ObjectNames.NicifyVariableName(attr.BaseType.Name)}"))
                ShowTypeMenu(types, addRect, selected => AddNewElement(property, selected));

            if (indexToRemove >= 0) property.DeleteArrayElementAtIndex(indexToRemove);
            if (indexToMoveUp >= 0) property.MoveArrayElement(indexToMoveUp, indexToMoveUp - 1);
            if (indexToMoveDown >= 0) property.MoveArrayElement(indexToMoveDown, indexToMoveDown + 1);

            property.serializedObject.ApplyModifiedProperties();
        }

        private static void AssignType(SerializedProperty listProp, int index, Type type)
        {
            var element = listProp.GetArrayElementAtIndex(index);
            element.managedReferenceValue = Activator.CreateInstance(type);
            listProp.serializedObject.ApplyModifiedProperties();
        }

        private static void AddNewElement(SerializedProperty listProp, Type type)
        {
            listProp.arraySize++;
            var element = listProp.GetArrayElementAtIndex(listProp.arraySize - 1);
            element.managedReferenceValue = Activator.CreateInstance(type);
            listProp.serializedObject.ApplyModifiedProperties();
        }

        private static void ShowTypeMenu(List<Type> types, Rect rect, Action<Type> onPicked)
        {
            var menu = new GenericMenu();
            foreach (var type in types)
            {
                var t = type;
                menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(t.Name)), false, () => onPicked(t));
            }
            menu.DropDown(rect);
        }

        private static List<Type> GetConcreteTypes(Type baseType)
        {
            var result = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] assemblyTypes;
                try
                {
                    assemblyTypes = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    assemblyTypes = ex.Types.Where(t => t != null).ToArray();
                }
                catch
                {
                    continue;
                }

                foreach (var t in assemblyTypes)
                {
                    if (t.IsAbstract || t.IsInterface) continue;
                    if (!baseType.IsAssignableFrom(t)) continue;
                    if (t.GetConstructor(Type.EmptyTypes) == null) continue;
                    result.Add(t);
                }
            }

            result.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            return result;
        }

        private static string GetShortTypeName(string managedReferenceFullTypename)
        {
            if (string.IsNullOrEmpty(managedReferenceFullTypename)) return null;

            int spaceIdx = managedReferenceFullTypename.IndexOf(' ');
            string full = spaceIdx >= 0 ? managedReferenceFullTypename[(spaceIdx + 1)..] : managedReferenceFullTypename;
            int dotIdx = full.LastIndexOf('.');
            return dotIdx >= 0 ? full[(dotIdx + 1)..] : full;
        }
    }
}
