#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

public class VertexColorImporter : AssetPostprocessor
{
    private Dictionary<string, Color[]> colors = new Dictionary<string, Color[]>();
    private Dictionary<Vector3, Color> sourcePositionToColor = new Dictionary<Vector3, Color>();
    private static string vertexColorMaterial = "Assets/Materials/VertexColorMaterial.mat";

    void OnPreprocessModel()
    {
        if (!assetPath.EndsWith(".obj")) return;
        
        var sourceColor = new Dictionary<string, List<Color>> { { "default", new List<Color>() } };
        var geometryIndices = new Dictionary<string, List<int>> { { "default", new List<int>() } };
        var currentGeometrySharedVerticesDetector = new HashSet<Vector3Int>();
        string meshName = "default";
        
        using (var reader = new StreamReader(Application.dataPath + assetPath.Replace("Assets", "")))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] words = line.Split(' ');
                switch (words[0])
                {
                    case "v" when words.Length == 7:
                        float x = float.Parse(words[1], NumberStyles.Float, CultureInfo.InvariantCulture);
                        float y = float.Parse(words[2], NumberStyles.Float, CultureInfo.InvariantCulture);
                        float z = float.Parse(words[3], NumberStyles.Float, CultureInfo.InvariantCulture);
                        float red = float.Parse(words[4], NumberStyles.Float, CultureInfo.InvariantCulture);
                        float green = float.Parse(words[5], NumberStyles.Float, CultureInfo.InvariantCulture);
                        float blue = float.Parse(words[6], NumberStyles.Float, CultureInfo.InvariantCulture);

                        Vector3 position = new Vector3(x, y, z);
                        Color color = new Color(red, green, blue);
                        sourcePositionToColor[position] = color;
                        sourceColor[meshName].Add(new Color(red, green, blue));
                        break;
                    case "f" when words.Length > 1:
                        for (int i = 1; i < words.Length; i++)
                        {
                            int vertexIndex = int.Parse(words[i], NumberStyles.Integer, CultureInfo.InvariantCulture) - 1;
                            if (currentGeometrySharedVerticesDetector.Add(new Vector3Int(vertexIndex, 0, 0)))
                            {
                                geometryIndices[meshName].Add(vertexIndex);
                            }
                        }
                        break;
                }
            }
        }

        ApplyImportSettings();

        foreach (var entry in sourceColor)
        {
            meshName = entry.Key;
            List<Color> colors = entry.Value;
            List<int> indices = geometryIndices[meshName];
            Color[] finalColors = new Color[indices.Count];

            for (int i = 0; i < indices.Count; i++)
            {
                int index = indices[i];
                if (index < colors.Count)
                {
                    finalColors[i] = colors[index];
                }
                else
                {
                    Debug.LogWarning($"Invalid vertex index: {index} for mesh '{meshName}'. Using default color.");
                    finalColors[i] = Color.white;
                }
            }

            this.colors.Add(meshName, finalColors);
        }
    }

    private void ApplyImportSettings()
    {
        ModelImporter importer = assetImporter as ModelImporter;
        if (importer != null)
        {
            importer.optimizeMeshVertices = false;
            importer.optimizeMeshPolygons = false;
            importer.weldVertices = false;
            importer.importNormals = ModelImporterNormals.None;
            importer.importTangents = ModelImporterTangents.None;
            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
        }
    }

    void OnPostprocessModel(GameObject gameObject)
    {
        if (!assetPath.EndsWith(".obj")) return;

        Material materialAsset = AssetDatabase.LoadAssetAtPath<Material>(vertexColorMaterial);

        MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
        foreach (var mf in meshFilters)
        {
            Mesh mesh = mf.sharedMesh;
            if (colors.TryGetValue(mesh.name, out Color[] vertexColors)) {
                mesh.SetColors(new List<Color>(vertexColors));
                MeshRenderer meshRenderer = mf.GetComponent<MeshRenderer>();
                if (meshRenderer != null) meshRenderer.sharedMaterial = materialAsset;
            }
        }
    }
}

#endif