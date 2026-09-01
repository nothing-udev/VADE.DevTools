using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace VADE.DevTools.Editor.Utilities
{
    public class FindMissingScriptsEditor : EditorWindow
    {
        [MenuItem("Tools/VADE/Utilities/Find Missing Scripts")]
        public static void ShowWindow()
        {
            GetWindow<FindMissingScriptsEditor>("Find Missing Scripts");
        }

        private Vector2 scrollPos;
        private List<GameObject> missingObjectsScene = new();
        private List<GameObject> missingObjectsPrefabs = new();
        private List<GameObject> missingObjectsResources = new();

        private void OnGUI()
        {
            if (GUILayout.Button("Scan Scene for Missing Scripts"))
                ScanScene();

            if (GUILayout.Button("Scan Project Prefabs for Missing Scripts"))
                ScanPrefabs();

            if (GUILayout.Button("Scan Resources for Missing Scripts"))
                ScanResources();

            GUILayout.Space(10);

            if (missingObjectsScene.Count > 0)
            {
                GUILayout.Label("Scene Objects with Missing Scripts:", EditorStyles.boldLabel);
                scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.Height(150));
                foreach (var go in missingObjectsScene)
                {
                    if (GUILayout.Button(go.name))
                        Selection.activeGameObject = go;
                }
                GUILayout.EndScrollView();
            }

            if (missingObjectsPrefabs.Count > 0)
            {
                GUILayout.Label("Prefabs with Missing Scripts:", EditorStyles.boldLabel);
                scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.Height(150));
                foreach (var go in missingObjectsPrefabs)
                {
                    if (GUILayout.Button(go.name))
                    {
                        string path = AssetDatabase.GetAssetPath(go);
                        Object prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        Selection.activeObject = prefab;
                    }
                }
                GUILayout.EndScrollView();
            }

            if (missingObjectsResources.Count > 0)
            {
                GUILayout.Label("Resources Objects with Missing Scripts:", EditorStyles.boldLabel);
                scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.Height(150));
                foreach (var go in missingObjectsResources)
                {
                    if (GUILayout.Button(go.name))
                        Selection.activeObject = go;
                }
                GUILayout.EndScrollView();
            }

            if (missingObjectsScene.Count == 0 && missingObjectsPrefabs.Count == 0 && missingObjectsResources.Count == 0)
                GUILayout.Label("No missing scripts found.", EditorStyles.helpBox);
        }

        private void ScanScene()
        {
            missingObjectsScene.Clear();
            var allObjects = Object.FindObjectsOfType<GameObject>();

            foreach (var go in allObjects)
            {
                var components = go.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        missingObjectsScene.Add(go);
                        break;
                    }
                }
            }

            Debug.Log("[VADE.DevTools] Scene scan complete. Missing scripts found: " + missingObjectsScene.Count);
        }

        private void ScanPrefabs()
        {
            missingObjectsPrefabs.Clear();
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });

            foreach (var guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                var components = prefab.GetComponentsInChildren<Component>(true);
                foreach (var c in components)
                {
                    if (c == null)
                    {
                        missingObjectsPrefabs.Add(prefab);
                        break;
                    }
                }
            }

            Debug.Log("[VADE.DevTools] Prefab scan complete. Missing scripts found: " + missingObjectsPrefabs.Count);
        }

        private void ScanResources()
        {
            missingObjectsResources.Clear();
            var resources = Resources.LoadAll("");

            foreach (var obj in resources)
            {
                if (obj is not GameObject go) continue;

                var components = go.GetComponentsInChildren<Component>(true);
                foreach (var c in components)
                {
                    if (c == null)
                    {
                        missingObjectsResources.Add(go);
                        break;
                    }
                }
            }

            Debug.Log("[VADE.DevTools] Resources scan complete. Missing scripts found: " + missingObjectsResources.Count);
        }
    }
}
