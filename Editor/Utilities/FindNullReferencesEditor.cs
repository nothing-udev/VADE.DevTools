using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;

namespace VADE.DevTools.Editor.Utilities
{
    public class FindNullReferencesEditor : EditorWindow
    {
        [MenuItem("Tools/VADE/Utilities/Find Null References")]
        public static void ShowWindow()
        {
            GetWindow<FindNullReferencesEditor>("Find Null References");
        }

        private Vector2 scrollPos;
        private List<string> nullFields = new();

        private void OnGUI()
        {
            if (GUILayout.Button("Scan Scene for Null References"))
                ScanScene();

            if (nullFields.Count > 0)
            {
                GUILayout.Label("Null References Found:", EditorStyles.boldLabel);
                scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.Height(400));
                foreach (var s in nullFields)
                    GUILayout.Label(s);
                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("No null references found.", EditorStyles.helpBox);
            }
        }

        private void ScanScene()
        {
            nullFields.Clear();
            var allObjects = Object.FindObjectsOfType<GameObject>();

            foreach (var go in allObjects)
            {
                var components = go.GetComponents<Component>();
                foreach (var c in components)
                {
                    if (c == null) continue;

                    var fields = c.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (var f in fields)
                    {
                        if (!f.IsPublic && f.GetCustomAttribute<SerializeField>() == null) continue;

                        if (f.GetValue(c) == null)
                            nullFields.Add(go.name + " -> " + c.GetType().Name + " -> " + f.Name);
                    }
                }
            }

            Debug.Log("[VADE.DevTools] Null references scan complete. Found: " + nullFields.Count);
        }
    }
}
