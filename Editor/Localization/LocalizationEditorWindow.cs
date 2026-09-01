using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VADE.DevTools.Editor.Localization
{

    public class LocalizationEditorWindow : EditorWindow
    {
        [MenuItem("Tools/VADE/Localization/Editor")]
        private static void Open() => GetWindow<LocalizationEditorWindow>("Localization").Show();

        private Dictionary<string, Dictionary<string, string>> _tables = new();
        private Dictionary<string, string> _filePaths = new();
        private Vector2 _scroll;
        private string _newKey = "";
        private string _newLanguageCode = "";
        private bool _dirty;

        private void OnEnable() => Rescan();

        private void Rescan()
        {
            _tables = LocalizationFileUtility.LoadAllTables(out _filePaths);
            _dirty = false;
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_filePaths.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "Не найдено ни одного файла Resources/Localization/*.json. Создайте первый ниже.",
                    MessageType.Info);
            }
            else
            {
                DrawTable();
            }

            EditorGUILayout.Space();
            DrawAddKeyRow();
            DrawAddLanguageRow();
            DrawValidation();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Обновить", EditorStyles.toolbarButton, GUILayout.Width(80)))
                    Rescan();

                GUI.enabled = _dirty;
                if (GUILayout.Button("Сохранить всё", EditorStyles.toolbarButton, GUILayout.Width(100)))
                    SaveAll();
                GUI.enabled = true;

                GUILayout.FlexibleSpace();
                GUILayout.Label($"{_filePaths.Count} язык(а/ов)" + (_dirty ? " *" : ""), EditorStyles.miniLabel);
            }
        }

        private void DrawTable()
        {
            var languages = _filePaths.Keys.OrderBy(l => l, StringComparer.Ordinal).ToList();
            var allKeys = CollectAllKeys();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Key", EditorStyles.boldLabel, GUILayout.Width(180));
                foreach (var lang in languages)
                    GUILayout.Label(lang, EditorStyles.boldLabel, GUILayout.Width(200));
                GUILayout.Label("", GUILayout.Width(20));
            }

            string keyToDelete = null;

            foreach (var key in allKeys)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(key, GUILayout.Width(180));

                    foreach (var lang in languages)
                    {
                        var table = _tables[lang];
                        table.TryGetValue(key, out string value);

                        EditorGUI.BeginChangeCheck();
                        string newValue = EditorGUILayout.TextField(value ?? "", GUILayout.Width(200));
                        if (EditorGUI.EndChangeCheck())
                        {
                            table[key] = newValue;
                            _dirty = true;
                        }
                    }

                    if (GUILayout.Button("x", GUILayout.Width(20)))
                        keyToDelete = key;
                }
            }

            if (keyToDelete != null)
            {
                foreach (var table in _tables.Values)
                    table.Remove(keyToDelete);
                _dirty = true;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawAddKeyRow()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                _newKey = EditorGUILayout.TextField("Новый ключ", _newKey);

                GUI.enabled = !string.IsNullOrEmpty(_newKey) && _filePaths.Count > 0;
                if (GUILayout.Button("+ Добавить ключ", GUILayout.Width(140)))
                {
                    foreach (var table in _tables.Values)
                        if (!table.ContainsKey(_newKey))
                            table[_newKey] = "";

                    _newKey = "";
                    _dirty = true;
                    GUI.FocusControl(null);
                }
                GUI.enabled = true;
            }
        }

        private void DrawAddLanguageRow()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                _newLanguageCode = EditorGUILayout.TextField("Новый язык (код)", _newLanguageCode);

                GUI.enabled = !string.IsNullOrEmpty(_newLanguageCode) && !_filePaths.ContainsKey(_newLanguageCode);
                if (GUILayout.Button("Создать файл", GUILayout.Width(140)))
                {
                    CreateLanguageFile(_newLanguageCode);
                    _newLanguageCode = "";
                }
                GUI.enabled = true;
            }
        }

        private void DrawValidation()
        {
            if (_filePaths.Count < 2) return;

            var languages = _filePaths.Keys.ToList();
            var allKeys = CollectAllKeys();

            var missing = new List<string>();
            foreach (var key in allKeys)
            {
                foreach (var lang in languages)
                {
                    if (!_tables[lang].TryGetValue(key, out var val) || string.IsNullOrEmpty(val))
                        missing.Add($"{key} -> {lang}");
                }
            }

            if (missing.Count == 0) return;

            string text = $"Не хватает {missing.Count} перевод(ов):\n" +
                          string.Join("\n", missing.Take(20)) +
                          (missing.Count > 20 ? $"\n... и ещё {missing.Count - 20}" : "");

            EditorGUILayout.HelpBox(text, MessageType.Warning);
        }

        private List<string> CollectAllKeys()
        {
            var allKeys = new SortedSet<string>(StringComparer.Ordinal);
            foreach (var table in _tables.Values)
                foreach (var key in table.Keys)
                    allKeys.Add(key);
            return allKeys.ToList();
        }

        private void CreateLanguageFile(string code)
        {
            string folder = _filePaths.Count > 0
                ? Path.GetDirectoryName(_filePaths.Values.First())
                : "Assets/Resources/Localization";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var newTable = new Dictionary<string, string>();
            foreach (var key in CollectAllKeys())
                newTable[key] = "";

            string path = Path.Combine(folder, code + ".json").Replace('\\', '/');
            LocalizationFileUtility.SaveTable(path, newTable);
            AssetDatabase.Refresh();
            Rescan();
        }

        private void SaveAll()
        {
            foreach (var kv in _filePaths)
                LocalizationFileUtility.SaveTable(kv.Value, _tables[kv.Key]);

            AssetDatabase.Refresh();
            _dirty = false;
        }
    }
}
