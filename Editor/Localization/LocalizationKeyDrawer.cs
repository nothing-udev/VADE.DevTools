using UnityEditor;
using UnityEngine;
using VADE.DevTools.Localization;

namespace VADE.DevTools.Editor.Localization
{
    [CustomPropertyDrawer(typeof(LocalizationKeyAttribute))]
    public class LocalizationKeyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var keys = LocalizationFileUtility.GetAllKeysSorted();

            if (keys.Length == 0)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            const float buttonWidth = 22f;
            var textRect = new Rect(position.x, position.y, position.width - buttonWidth - 2f, position.height);
            var buttonRect = new Rect(position.xMax - buttonWidth, position.y, buttonWidth, position.height);

            EditorGUI.PropertyField(textRect, property, label);

            if (GUI.Button(buttonRect, "▾"))
            {
                var menu = new GenericMenu();
                var propertyPath = property.propertyPath;
                var serializedObject = property.serializedObject;

                foreach (var key in keys)
                {
                    string keyCopy = key;
                    menu.AddItem(new GUIContent(keyCopy), keyCopy == property.stringValue, () =>
                    {
                        var prop = serializedObject.FindProperty(propertyPath);
                        serializedObject.Update();
                        prop.stringValue = keyCopy;
                        serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.DropDown(buttonRect);
            }
        }
    }
}
