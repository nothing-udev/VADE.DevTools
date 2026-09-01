using System;
using UnityEditor;
using UnityEngine;
using VADE.DevTools.Onboarding;

namespace VADE.DevTools.Editor.Onboarding
{
    [CustomPropertyDrawer(typeof(TaskId))]
    public class TaskIdDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var valueProp = property.FindPropertyRelative("value");

            const float btnW = 45f;
            var fieldRect = new Rect(position.x, position.y, position.width - btnW * 3 - 6f, position.height);
            var genRect = new Rect(fieldRect.xMax + 2f, position.y, btnW, position.height);
            var copyRect = new Rect(genRect.xMax + 2f, position.y, btnW, position.height);
            var pasteRect = new Rect(copyRect.xMax + 2f, position.y, btnW, position.height);

            EditorGUI.PropertyField(fieldRect, valueProp, label);

            if (GUI.Button(genRect, "Gen"))
                valueProp.stringValue = Guid.NewGuid().ToString("N");

            if (GUI.Button(copyRect, "Copy"))
                EditorGUIUtility.systemCopyBuffer = valueProp.stringValue;

            if (GUI.Button(pasteRect, "Paste"))
                valueProp.stringValue = EditorGUIUtility.systemCopyBuffer;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;
    }
}
