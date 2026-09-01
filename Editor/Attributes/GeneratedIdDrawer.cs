using System;
using UnityEditor;
using UnityEngine;
using VADE.DevTools.Attributes;

namespace VADE.DevTools.Editor.Attributes
{
    [CustomPropertyDrawer(typeof(GeneratedIdAttribute))]
    public class GeneratedIdDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (string.IsNullOrEmpty(property.stringValue))
                property.stringValue = Guid.NewGuid().ToString("N");

            const float btnW = 60f;
            var fieldRect = new Rect(position.x, position.y, position.width - btnW * 2 - 4f, position.height);
            var genRect = new Rect(fieldRect.xMax + 2f, position.y, btnW, position.height);
            var copyRect = new Rect(genRect.xMax + 2f, position.y, btnW, position.height);

            using (new EditorGUI.DisabledScope(true))
                EditorGUI.PropertyField(fieldRect, property, label);

            if (GUI.Button(genRect, "Generate"))
                property.stringValue = Guid.NewGuid().ToString("N");

            if (GUI.Button(copyRect, "Copy"))
                EditorGUIUtility.systemCopyBuffer = property.stringValue;
        }
    }
}
