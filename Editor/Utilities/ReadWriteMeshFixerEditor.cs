using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace VADE.DevTools.Editor.Utilities
{
    public class ReadWriteMeshFixerEditor : EditorWindow
    {
        private class ModelItem
        {
            public string Path;
            public bool IsSelected;

            public ModelItem(string path, bool isSelected = true)
            {
                Path = path;
                IsSelected = isSelected;
            }
        }

        private List<ModelItem> readableModels = new();
        private Vector2 scrollPosition;

        [MenuItem("Tools/VADE/Utilities/Read-Write Mesh Fixer")]
        public static void ShowWindow()
        {
            GetWindow<ReadWriteMeshFixerEditor>("R/W Mesh Fixer");
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "Скрипт находит модели с включенным Read/Write. Отметьте галочками те из них, для которых нужно отключить этот флаг.",
                MessageType.Info);
            GUILayout.Space(10);

            if (GUILayout.Button("1. Найти модели с Read/Write", GUILayout.Height(30)))
                ScanModels();

            if (readableModels.Count > 0)
            {
                EditorGUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Найдено моделей ({readableModels.Count}):", EditorStyles.boldLabel);
                if (GUILayout.Button("Выбрать все", GUILayout.Width(90))) SetAllSelected(true);
                if (GUILayout.Button("Снять все", GUILayout.Width(90))) SetAllSelected(false);
                EditorGUILayout.EndHorizontal();

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
                foreach (var item in readableModels)
                {
                    EditorGUILayout.BeginHorizontal();
                    item.IsSelected = EditorGUILayout.Toggle(item.IsSelected, GUILayout.Width(20));
                    EditorGUILayout.SelectableLabel(item.Path, EditorStyles.miniLabel, GUILayout.Height(18));
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space(10);

                GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
                if (GUILayout.Button("2. Отключить Read/Write для выбранных", GUILayout.Height(35)))
                    DisableReadWriteSelected();
                GUI.backgroundColor = Color.white;
            }
        }

        private void ScanModels()
        {
            readableModels.Clear();
            var guids = AssetDatabase.FindAssets("t:Model");

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;

                if (importer != null && importer.isReadable)
                    readableModels.Add(new ModelItem(path, true));
            }

            Debug.Log($"[VADE.DevTools] Поиск завершен. Найдено моделей с Read/Write: {readableModels.Count}");
        }

        private void SetAllSelected(bool state)
        {
            foreach (var item in readableModels)
                item.IsSelected = state;
        }

        private void DisableReadWriteSelected()
        {
            int count = 0;
            AssetDatabase.StartAssetEditing();

            try
            {
                foreach (var item in readableModels)
                {
                    if (!item.IsSelected) continue;

                    var importer = AssetImporter.GetAtPath(item.Path) as ModelImporter;
                    if (importer != null && importer.isReadable)
                    {
                        importer.isReadable = false;
                        importer.SaveAndReimport();
                        count++;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            Debug.Log($"[VADE.DevTools] Успешно отключен Read/Write для {count} выбранных моделей.");
            ScanModels();
        }
    }
}
