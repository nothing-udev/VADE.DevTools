using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VADE.DevTools.Editor.Utilities
{
    public static class MeshBakerEditor
    {
        private enum Resolving { Not, Remove, Create }

        private const int MaxVertexCountPerObject = 65000;

        private static Resolving colorResolving;
        private static Resolving normalsResolving;
        private static Resolving tangentsResolving;
        private static Resolving uvResolving;

        private static int objectNum;

        [MenuItem("GameObject/VADE/Bake Selected Meshes", false, 0)]
        private static void BakeMeshes(MenuCommand menuCommand)
        {
            if (Selection.objects.Length == 0) return;

            if (Selection.objects.Length > 1 && menuCommand.context != Selection.objects[0])
                return;

            var vertexes = new List<Vector3>();
            var normals = new List<Vector3>();
            var tangents = new List<Vector4>();
            var colors = new List<Color>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            var meshesWithMaterials = new Dictionary<Material, List<Mesh>>();
            var meshesWithoutMaterials = new List<Mesh>();

            objectNum = 0;
            if (!FillDataAndCheckResolving(meshesWithMaterials, meshesWithoutMaterials))
                return;

            bool separateObjectsWithDifferentMaterials = false;
            if (meshesWithMaterials.Count + (meshesWithoutMaterials.Count > 0 ? 1 : 0) > 1)
            {
                separateObjectsWithDifferentMaterials = EditorUtility.DisplayDialog("Mesh Baker",
                    "Do you want to separate objects with different materials?", "Yes", "No");
            }

            if (!separateObjectsWithDifferentMaterials)
            {
                Material material = null;
                foreach (var meshToBake in meshesWithMaterials)
                {
                    material = meshToBake.Key;
                    if (material != null) break;
                }

                foreach (var mesh in meshesWithoutMaterials)
                    Bake(mesh, vertexes, normals, tangents, colors, uvs, triangles, material);

                foreach (var meshToBake in meshesWithMaterials)
                foreach (var mesh in meshToBake.Value)
                    Bake(mesh, vertexes, normals, tangents, colors, uvs, triangles, material);

                if (vertexes.Count > 0)
                {
                    CreateObject(vertexes, normals, tangents, colors, uvs, triangles, material);
                    vertexes.Clear(); normals.Clear(); tangents.Clear(); colors.Clear(); uvs.Clear(); triangles.Clear();
                }
            }
            else
            {
                foreach (var mesh in meshesWithoutMaterials)
                    Bake(mesh, vertexes, normals, tangents, colors, uvs, triangles, null);

                if (vertexes.Count > 0)
                {
                    CreateObject(vertexes, normals, tangents, colors, uvs, triangles, null);
                    vertexes.Clear(); normals.Clear(); tangents.Clear(); colors.Clear(); uvs.Clear(); triangles.Clear();
                }

                foreach (var meshToBake in meshesWithMaterials)
                {
                    foreach (var mesh in meshToBake.Value)
                        Bake(mesh, vertexes, normals, tangents, colors, uvs, triangles, meshToBake.Key);

                    CreateObject(vertexes, normals, tangents, colors, uvs, triangles, meshToBake.Key);
                    vertexes.Clear(); normals.Clear(); tangents.Clear(); colors.Clear(); uvs.Clear(); triangles.Clear();
                }
            }

            if (EditorUtility.DisplayDialog("Mesh Baker", "Do you want to remove sources?", "Yes", "No"))
            {
                foreach (var selected in Selection.gameObjects)
                    if (selected != null)
                        Undo.DestroyObjectImmediate(selected);
            }

            int meshesCount = meshesWithoutMaterials.Count;
            foreach (var bakedMeshes in meshesWithMaterials)
                meshesCount += bakedMeshes.Value.Count;

            EditorUtility.DisplayDialog("Mesh Baker", "Baked " + meshesCount + " meshes.", "Cool!");
        }

        private static bool FillDataAndCheckResolving(Dictionary<Material, List<Mesh>> meshesWithMaterials, List<Mesh> meshesWithoutMaterials)
        {
            colorResolving = Resolving.Not;
            normalsResolving = Resolving.Not;
            uvResolving = Resolving.Not;

            bool anyHasColors = false, anyHasNormals = false, anyHasUVs = false;
            bool anyHasNotColors = false, anyHasNotNormals = false, anyHasNotUVs = false;

            var transforms = new HashSet<Transform>();

            foreach (var selected in Selection.gameObjects)
            {
                var meshFilters = selected.GetComponentsInChildren<MeshFilter>();
                foreach (var meshFilter in meshFilters)
                {
                    if (transforms.Contains(meshFilter.transform)) continue;

                    Material material = null;
                    var mr = meshFilter.GetComponent<MeshRenderer>();
                    if (mr != null) material = mr.sharedMaterial;

                    var mesh = Object.Instantiate(meshFilter.sharedMesh);
                    HandleMesh(meshesWithMaterials, meshesWithoutMaterials, mesh, meshFilter.transform, material, transforms,
                        ref anyHasNotNormals, ref anyHasNotColors, ref anyHasNotUVs, ref anyHasNormals, ref anyHasColors, ref anyHasUVs);
                }
            }

            return SetResolving(anyHasNormals, anyHasNotNormals, ref normalsResolving, "normals") &&
                   SetResolving(anyHasColors, anyHasNotColors, ref colorResolving, "colors") &&
                   SetResolving(anyHasUVs, anyHasNotUVs, ref uvResolving, "uvs");
        }

        private static void HandleMesh(Dictionary<Material, List<Mesh>> meshesWithMaterials, List<Mesh> meshesWithoutMaterials, Mesh mesh,
            Transform transform, Material material, HashSet<Transform> transforms, ref bool anyHasNotNormals,
            ref bool anyHasNotColors, ref bool anyHasNotUVs, ref bool anyHasNormals, ref bool anyHasColors, ref bool anyHasUVs)
        {
            mesh.vertices = mesh.vertices.Select(transform.TransformPoint).ToArray();
            mesh.normals = mesh.normals.Select(transform.TransformDirection).ToArray();

            if (material == null)
            {
                meshesWithoutMaterials.Add(mesh);
            }
            else if (meshesWithMaterials.ContainsKey(material))
            {
                meshesWithMaterials[material].Add(mesh);
            }
            else
            {
                meshesWithMaterials.Add(material, new List<Mesh> { mesh });
            }

            transforms.Add(transform);

            CheckMeshAttributes(mesh, ref anyHasNotNormals, ref anyHasNotColors, ref anyHasNotUVs, ref anyHasNormals, ref anyHasColors, ref anyHasUVs);
        }

        private static void CheckMeshAttributes(Mesh mesh, ref bool anyHasNotNormals, ref bool anyHasNotColors, ref bool anyHasNotUVs,
            ref bool anyHasNormals, ref bool anyHasColors, ref bool anyHasUVs)
        {
            bool hasNormals = mesh.vertexCount == mesh.normals.Length;
            bool hasColors = mesh.vertexCount == mesh.colors.Length;
            bool hasUVs = mesh.vertexCount == mesh.uv.Length;

            anyHasNotNormals |= !hasNormals;
            anyHasNotColors |= !hasColors;
            anyHasNotUVs |= !hasUVs;

            anyHasNormals |= hasNormals;
            anyHasColors |= hasColors;
            anyHasUVs |= hasUVs;
        }

        private static bool SetResolving(bool has, bool hasNot, ref Resolving resolving, string property)
        {
            if (has && hasNot)
            {
                var result = EditorUtility.DisplayDialogComplex("Mesh Baker",
                    "Not all objects used " + property + ".",
                    "Don't use " + property, "Cancel", "Create fake " + property);

                if (result == 1) return false;
                resolving = result == 0 ? Resolving.Remove : Resolving.Create;
            }
            return true;
        }

        private static void CreateObject(List<Vector3> vertexes, List<Vector3> normals, List<Vector4> tangents, List<Color> colors,
            List<Vector2> uvs, List<int> triangles, Material material)
        {
            var go = new GameObject();
            Undo.RegisterCreatedObjectUndo(go, "Create a new baked gameobject");
            objectNum++;
            go.name = "Baked Mesh" + (objectNum == 1 ? "" : " " + objectNum);

            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            var newMesh = new Mesh();

            newMesh.SetVertices(vertexes);
            if (normals.Count != 0 && normalsResolving != Resolving.Remove) newMesh.SetNormals(normals);
            if (tangents.Count != 0 && tangentsResolving != Resolving.Remove) newMesh.SetTangents(tangents);
            if (colors.Count != 0 && colorResolving != Resolving.Remove) newMesh.SetColors(colors);
            if (uvs.Count != 0 || uvResolving != Resolving.Remove) newMesh.SetUVs(0, uvs);

            newMesh.SetTriangles(triangles, 0);
            mf.sharedMesh = newMesh;
            mr.material = material;
        }

        private static void Bake(Mesh mesh, List<Vector3> vertexes, List<Vector3> normals, List<Vector4> tangents,
            List<Color> colors, List<Vector2> uvs, List<int> triangles, Material material)
        {
            if (vertexes.Count + mesh.vertexCount > MaxVertexCountPerObject)
            {
                CreateObject(vertexes, normals, tangents, colors, uvs, triangles, material);
                vertexes.Clear(); normals.Clear(); tangents.Clear(); colors.Clear(); uvs.Clear(); triangles.Clear();
            }

            int startCount = vertexes.Count;
            foreach (var vertex in mesh.vertices) vertexes.Add(vertex);
            foreach (var triangle in mesh.triangles) triangles.Add(triangle + startCount);

            FillOrResolve(mesh.normals, normals, mesh.vertices.Length, normalsResolving);
            FillOrResolve(mesh.tangents, tangents, mesh.tangents.Length, tangentsResolving);
            FillOrResolve(mesh.colors, colors, mesh.vertices.Length, colorResolving);
            FillOrResolve(mesh.uv, uvs, mesh.vertices.Length, uvResolving);
        }

        private static void FillOrResolve<T>(T[] source, List<T> destination, int expectedCount, Resolving resolvingLogic)
        {
            if (source.Length == 0 && resolvingLogic == Resolving.Create)
            {
                for (int i = 0; i < expectedCount; i++)
                    destination.Add(default);
            }
            else
            {
                destination.AddRange(source);
            }
        }
    }
}
