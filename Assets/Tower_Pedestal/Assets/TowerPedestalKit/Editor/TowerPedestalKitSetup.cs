#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class TowerPedestalKitSetup
{
    private const string Root = "Assets/TowerPedestalKit";
    private const string MaterialsPath = Root + "/Materials";
    private const string PrefabsPath = Root + "/Prefabs";

    static TowerPedestalKitSetup()
    {
        EditorApplication.delayCall += GenerateIfMissing;
    }

    [MenuItem("Tools/Tower Pedestal Kit/Regenerate Prefabs")]
    public static void RegenerateFromMenu()
    {
        GenerateAll(true);
    }

    private static void GenerateIfMissing()
    {
        if (!AssetDatabase.IsValidFolder(Root)) return;
        string testPrefab = PrefabsPath + "/Pedestal_Desert.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(testPrefab) == null)
        {
            GenerateAll(false);
        }
    }

    private static void GenerateAll(bool force)
    {
        Directory.CreateDirectory(MaterialsPath);
        Directory.CreateDirectory(PrefabsPath);
        AssetDatabase.Refresh();

        CreatePedestal("Pedestal_Desert", new Color(0.82f, 0.67f, 0.46f), new Color(0.70f, 0.54f, 0.34f), force);
        CreatePedestal("Pedestal_Snow",   new Color(0.88f, 0.92f, 0.98f), new Color(0.74f, 0.82f, 0.93f), force);
        CreatePedestal("Pedestal_Summer", new Color(0.84f, 0.79f, 0.66f), new Color(0.44f, 0.67f, 0.25f), force);
        CreatePedestal("Pedestal_Autumn", new Color(0.77f, 0.42f, 0.22f), new Color(0.53f, 0.28f, 0.14f), force);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("TowerPedestalKit: Materials and prefabs generated.");
    }

    private static void CreatePedestal(string name, Color topBaseColor, Color accentColor, bool force)
    {
        string matMainPath = MaterialsPath + "/" + name + "_Main.mat";
        string matAccentPath = MaterialsPath + "/" + name + "_Accent.mat";
        string prefabPath = PrefabsPath + "/" + name + ".prefab";

        Material matMain = AssetDatabase.LoadAssetAtPath<Material>(matMainPath);
        if (matMain == null)
        {
            matMain = new Material(FindBestShader());
            ApplyMaterialColor(matMain, topBaseColor);
            AssetDatabase.CreateAsset(matMain, matMainPath);
        }
        else if (force)
        {
            ApplyMaterialColor(matMain, topBaseColor);
            EditorUtility.SetDirty(matMain);
        }

        Material matAccent = AssetDatabase.LoadAssetAtPath<Material>(matAccentPath);
        if (matAccent == null)
        {
            matAccent = new Material(FindBestShader());
            ApplyMaterialColor(matAccent, accentColor);
            AssetDatabase.CreateAsset(matAccent, matAccentPath);
        }
        else if (force)
        {
            ApplyMaterialColor(matAccent, accentColor);
            EditorUtility.SetDirty(matAccent);
        }

        GameObject root = new GameObject(name);

        // Base layer
        GameObject baseLayer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseLayer.name = "Base";
        baseLayer.transform.SetParent(root.transform, false);
        baseLayer.transform.localScale = new Vector3(10f, 0.55f, 10f);
        baseLayer.transform.localPosition = new Vector3(0f, 0.275f, 0f);
        Assign(baseLayer, matMain);

        // Middle ring
        GameObject midLayer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        midLayer.name = "Middle";
        midLayer.transform.SetParent(root.transform, false);
        midLayer.transform.localScale = new Vector3(8.6f, 0.45f, 8.6f);
        midLayer.transform.localPosition = new Vector3(0f, 0.775f, 0f);
        Assign(midLayer, name.Contains("Summer") ? matAccent : matMain);

        // Top platform
        GameObject topLayer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        topLayer.name = "Top";
        topLayer.transform.SetParent(root.transform, false);
        topLayer.transform.localScale = new Vector3(7.3f, 0.50f, 7.3f);
        topLayer.transform.localPosition = new Vector3(0f, 1.25f, 0f);
        Assign(topLayer, matMain);

        // Decorative trim layer 1
        GameObject trim1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trim1.name = "Trim1";
        trim1.transform.SetParent(root.transform, false);
        trim1.transform.localScale = new Vector3(8.95f, 0.09f, 8.95f);
        trim1.transform.localPosition = new Vector3(0f, 0.56f, 0f);
        Assign(trim1, matAccent);

        // Decorative trim layer 2
        GameObject trim2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trim2.name = "Trim2";
        trim2.transform.SetParent(root.transform, false);
        trim2.transform.localScale = new Vector3(7.65f, 0.09f, 7.65f);
        trim2.transform.localPosition = new Vector3(0f, 1.02f, 0f);
        Assign(trim2, matAccent);

        // Remove colliders from trims, keep on main layers only
        Object.DestroyImmediate(trim1.GetComponent<Collider>());
        Object.DestroyImmediate(trim2.GetComponent<Collider>());

        // Add root helper component settings
        root.transform.position = Vector3.zero;

        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existing != null)
        {
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }
        else
        {
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }

        Object.DestroyImmediate(root);
    }

    private static void Assign(GameObject go, Material mat)
    {
        var renderer = go.GetComponent<MeshRenderer>();
        if (renderer != null) renderer.sharedMaterial = mat;
    }

    private static Shader FindBestShader()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        return shader;
    }

    private static void ApplyMaterialColor(Material mat, Color color)
    {
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.1f);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.1f);
    }
}
#endif
