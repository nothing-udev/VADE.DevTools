using UnityEngine;
using UnityEditor;

namespace VADE.DevTools.Editor.Utilities
{
    public static class FindStaticIssuesEditor
    {
        [MenuItem("Tools/VADE/Utilities/Find Static Issues")]
        public static void ScanScene()
        {
            var allObjects = Object.FindObjectsOfType<GameObject>();
            int issuesFound = 0;

            foreach (var go in allObjects)
            {
                var flags = GameObjectUtility.GetStaticEditorFlags(go);

                bool isOccluder = (flags & StaticEditorFlags.OccluderStatic) != 0;
                bool isOccludee = (flags & StaticEditorFlags.OccludeeStatic) != 0;
                bool isContributeGI = (flags & StaticEditorFlags.ContributeGI) != 0;

                if (!isOccluder && !isOccludee && !isContributeGI)
                    continue;

                var mf = go.GetComponent<MeshFilter>();
                var mr = go.GetComponent<MeshRenderer>();

                if (mf == null || mr == null)
                {
                    Debug.LogWarning($"[VADE.DevTools] '{go.name}' помечен как Static, но не имеет MeshFilter или MeshRenderer!", go);
                    issuesFound++;
                    continue;
                }

                if (mf.sharedMesh == null)
                {
                    Debug.LogWarning($"[VADE.DevTools] '{go.name}' помечен как Static, но ссылка на Mesh пуста (Missing)!", go);
                    issuesFound++;
                    continue;
                }

                var mesh = mf.sharedMesh;
                if (mesh.uv == null || mesh.uv.Length == 0)
                {
                    Debug.LogWarning($"[VADE.DevTools] У объекта '{go.name}' на меше '{mesh.name}' отсутствуют UV-координаты!", go);
                    issuesFound++;
                }
            }

            Debug.Log($"[VADE.DevTools] Сканирование завершено. Найдено проблемных объектов: {issuesFound}");
        }
    }
}
