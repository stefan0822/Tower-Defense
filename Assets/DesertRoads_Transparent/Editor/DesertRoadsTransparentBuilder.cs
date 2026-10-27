#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// Automatically prepares Unity-ready materials and 10x10 terrain overlay prefabs.
[InitializeOnLoad]
public static class DesertRoadsTransparentBuilder
{
    const string Root = "Assets/DesertRoads_Transparent";
    static readonly string[] Names = {
        "Road_Straight", "Road_Curve90", "Road_SCurve",
        "Road_TJunction", "Road_Crossroads", "Road_Curve45"
    };
    static readonly int[] Angles = { 0, 90, 180, 270 };

    static DesertRoadsTransparentBuilder()
    {
        EditorApplication.delayCall += AutoSetup;
    }

    static void AutoSetup()
    {
        if (!AssetDatabase.IsValidFolder(Root)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += AutoSetup;
            return;
        }
        // Leave already generated materials / prefabs untouched.
        if (AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/Road_Curve45_R270.prefab") == null)
            Build();
    }

    [MenuItem("Tools/Desert Roads Transparent/Create/Refresh Materials & Prefabs")]
    public static void Build()
    {
        if (!AssetDatabase.IsValidFolder(Root)) return;
        EnsureFolder(Root, "Materials");
        EnsureFolder(Root, "Meshes");
        EnsureFolder(Root, "Prefabs");

        Shader shader = null;
        if (GraphicsSettings.currentRenderPipeline != null)
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("HDRP/Unlit");
        }
        if (shader == null) shader = Shader.Find("Unlit/Transparent");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            Debug.LogError("Desert Roads Transparent: No suitable shader found.");
            return;
        }
        // Make one geometry asset per orientation. Rotating UVs avoids moving
        // the grid-corner pivot or changing the 10x10 world-space footprint.
        foreach (int angle in Angles)
        {
            string meshPath = Root + "/Meshes/RoadTile10_R" + angle + ".asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (mesh == null)
            {
                mesh = CreateFlatTile(angle);
                AssetDatabase.CreateAsset(mesh, meshPath);
            }
        }

        int created = 0;
        foreach (string name in Names)
        {
            string texPath = Root + "/Textures/" + name + ".png";
            TextureImporter importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (importer != null &&
                (importer.wrapMode != TextureWrapMode.Clamp || importer.filterMode != FilterMode.Bilinear || !importer.alphaIsTransparency))
            {
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (tex == null)
            {
                Debug.LogWarning("Desert Roads Transparent: texture missing: " + texPath);
                continue;
            }
            string matPath = Root + "/Materials/" + name + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                mat.name = name;
                AssetDatabase.CreateAsset(mat, matPath);
            }
            if (mat.shader != shader) mat.shader = shader;
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
            if (mat.HasProperty("_BaseColorMap")) mat.SetTexture("_BaseColorMap", tex);
            SetTransparent(mat);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0f);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0f);
            EditorUtility.SetDirty(mat);

            foreach (int angle in Angles)
            {
                string prefabPath = Root + "/Prefabs/" + name + "_R" + angle + ".prefab";
                Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(
                    Root + "/Meshes/RoadTile10_R" + angle + ".asset");
                if (mesh == null) continue;
                GameObject tile = new GameObject(name + "_R" + angle);
                try
                {
                    MeshFilter mf = tile.AddComponent<MeshFilter>();
                    mf.sharedMesh = mesh;
                    MeshRenderer mr = tile.AddComponent<MeshRenderer>();
                    mr.sharedMaterial = mat;
                    MeshCollider col = tile.AddComponent<MeshCollider>();
                    col.sharedMesh = mesh;
                    tile.transform.position = Vector3.zero;
                    tile.transform.rotation = Quaternion.identity;
                    tile.transform.localScale = Vector3.one;
                    PrefabUtility.SaveAsPrefabAsset(tile, prefabPath);
                    created++;
                }
                finally { UnityEngine.Object.DestroyImmediate(tile); }
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Desert Roads Transparent: ready! 6 materials and " + created +
                  " prefab tiles (four orientations each) in " + Root + "/Prefabs.");
    }

    static void SetTransparent(Material mat)
    {
        // Unity URP Unlit and HDRP Unlit expose surface type properties.
        if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
        if (mat.HasProperty("_SurfaceType")) mat.SetFloat("_SurfaceType", 1f);
        if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f);
        if (mat.HasProperty("_AlphaClip")) mat.SetFloat("_AlphaClip", 0f);
        if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0f);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.renderQueue = (int)RenderQueue.Transparent;
    }

    static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }

    static Vector2 RotatedUV(float u, float v, int angle)
    {
        switch (angle)
        {
            case 90: return new Vector2(v, 1f - u);
            case 180: return new Vector2(1f - u, 1f - v);
            case 270: return new Vector2(1f - v, u);
            default: return new Vector2(u, v);
        }
    }

    static Mesh CreateFlatTile(int angle)
    {
        const float y = 0.045f;
        Mesh mesh = new Mesh();
        mesh.name = "RoadTile10_R" + angle;
        // Position is local 0..10 on X and Z with pivot (0, 0, 0).
        mesh.vertices = new[] {
            new Vector3(0f, y, 0f), new Vector3(10f, y, 0f),
            new Vector3(0f, y, 10f), new Vector3(10f, y, 10f)
        };
        mesh.uv = new[] {
            RotatedUV(0f, 0f, angle), RotatedUV(1f, 0f, angle),
            RotatedUV(0f, 1f, angle), RotatedUV(1f, 1f, angle)
        };
        mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
#endif
