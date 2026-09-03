#if VADE_IAP
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Purchasing;
using VADE.DevTools.IAP;

namespace VADE.DevTools.Editor.IAP
{
    public class IAPProductEditorWindow : EditorWindow
    {
        [MenuItem("Tools/VADE/IAP/Product Editor")]
        private static void Open() => GetWindow<IAPProductEditorWindow>("IAP Products").Show();

        private string folder = "Assets/Resources/Configs/Shop";
        private readonly List<ProductData> products = new();
        private Vector2 scroll;

        private string newId = "";
        private ProductType newType = ProductType.Consumable;
        private Sprite newIcon;

        private void OnEnable() => Refresh();

        private void Refresh()
        {
            products.Clear();
            foreach (var guid in AssetDatabase.FindAssets("t:ProductData"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ProductData>(path);
                if (asset != null) products.Add(asset);
            }
            products.Sort((a, b) => string.CompareOrdinal(a.id, b.id));
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.Space();
            DrawCreateForm();
            EditorGUILayout.Space();
            DrawList();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Обновить", EditorStyles.toolbarButton, GUILayout.Width(90)))
                    Refresh();

                GUILayout.FlexibleSpace();
                GUILayout.Label($"{products.Count} товар(ов)", EditorStyles.miniLabel);
            }
        }

        private void DrawCreateForm()
        {
            EditorGUILayout.LabelField("Новый товар", EditorStyles.boldLabel);

            folder = EditorGUILayout.TextField("Папка", folder);
            newId = EditorGUILayout.TextField("Id", newId);
            newType = (ProductType)EditorGUILayout.EnumPopup("Тип", newType);
            newIcon = (Sprite)EditorGUILayout.ObjectField("Иконка", newIcon, typeof(Sprite), false);

            bool idTaken = !string.IsNullOrEmpty(newId) && products.Any(p => p.id == newId);
            if (idTaken)
                EditorGUILayout.HelpBox("Товар с таким id уже существует.", MessageType.Warning);

            GUI.enabled = !string.IsNullOrEmpty(newId) && !idTaken;
            if (GUILayout.Button("Создать"))
                CreateProduct();
            GUI.enabled = true;
        }

        private void CreateProduct()
        {
            EnsureFolder(folder);

            var product = ScriptableObject.CreateInstance<ProductData>();
            product.id = newId;
            product.type = newType;
            product.icon = newIcon;

            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{newId}.asset");
            AssetDatabase.CreateAsset(product, path);
            AssetDatabase.SaveAssets();

            newId = "";
            newIcon = null;

            Refresh();
            Selection.activeObject = product;
            EditorGUIUtility.PingObject(product);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private void DrawList()
        {
            EditorGUILayout.LabelField("Товары", EditorStyles.boldLabel);

            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var product in products)
            {
                if (product == null) continue;

                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(product.id, GUILayout.Width(160));
                    EditorGUILayout.LabelField(product.type.ToString(), GUILayout.Width(110));

                    if (GUILayout.Button("Выбрать", GUILayout.Width(80)))
                    {
                        Selection.activeObject = product;
                        EditorGUIUtility.PingObject(product);
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
#endif
