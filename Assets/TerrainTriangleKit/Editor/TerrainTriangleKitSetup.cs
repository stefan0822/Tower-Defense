#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class TerrainTriangleKitSetup
{
    const string Root = "Assets/TerrainTriangleKit";
    static readonly string[] Categories = {
        "Grass_Flat", "Grass_Hills", "Grass_RockyGround", "Grass_RockyMountains",
        "Desert_Flat", "Desert_Mountains", "Snow_Flat", "Snow_Mountains"
    };
    static readonly string[] Orientations = { "SW", "SE", "NW", "NE" };

    [MenuItem("Tools/Terrain Triangle Kit/1. Generate Materials and Prefabs")]
    public static void BuildAssets()
    {
        Directory.CreateDirectory(Root + "/Materials");
        Directory.CreateDirectory(Root + "/Prefabs");
        AssetDatabase.Refresh();
        int count = 0;
        foreach (string category in Categories)
        {
            Directory.CreateDirectory(Root + "/Materials/" + category);
            Directory.CreateDirectory(Root + "/Prefabs/" + category);
            AssetDatabase.Refresh();
            for (int i = 1; i <= 12; i++)
            {
                string baseName = category + "_" + i.ToString("00");
                string texturePath = Root + "/Textures/" + category + "/" + baseName + ".png";
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (texture == null)
                {
                    Debug.LogWarning("Missing texture: " + baseName);
                    continue;
                }
                foreach (string orient in Orientations)
                {
                    string name = baseName + "_" + orient;
                    string modelPath = Root + "/Models/" + category + "/" + name + ".obj";
                    string matPath = Root + "/Materials/" + category + "/" + name + ".mat";
                    string prefabPath = Root + "/Prefabs/" + category + "/" + name + ".prefab";
                    GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                    if (model == null) { Debug.LogWarning("Missing model: " + name); continue; }
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                    if (material == null)
                    {
                        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                        if (shader == null) shader = Shader.Find("Standard");
                        if (shader == null) shader = Shader.Find("HDRP/Lit");
                        if (shader == null) shader = Shader.Find("Unlit/Texture");
                        material = new Material(shader);
                        AssetDatabase.CreateAsset(material, matPath);
                    }
                    if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
                    if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
                    if (material.HasProperty("_BaseColorMap")) material.SetTexture("_BaseColorMap", texture);
                    if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
                    if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0f);
                    if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", 0f);
                    EditorUtility.SetDirty(material);

                    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
                    instance.name = name;
                    foreach (MeshRenderer r in instance.GetComponentsInChildren<MeshRenderer>())
                        r.sharedMaterials = new[] { material };
                    foreach (MeshFilter filter in instance.GetComponentsInChildren<MeshFilter>())
                    {
                        MeshCollider collider = filter.GetComponent<MeshCollider>();
                        if (collider == null) collider = filter.gameObject.AddComponent<MeshCollider>();
                        collider.sharedMesh = filter.sharedMesh;
                    }
                    PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                    Object.DestroyImmediate(instance);
                    count++;
                }
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Terrain Triangle Kit", "Created " + count + " triangle prefabs.", "OK");
    }

    [MenuItem("Tools/Terrain Triangle Kit/2. Create Demo Scene")]
    public static void BuildDemoScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        int row = 0;
        foreach (string category in Categories)
        {
            var group = new GameObject(category);
            int col = 0;
            for (int i = 1; i <= 12; i++)
            {
                foreach (string orient in Orientations)
                {
                    string name = category + "_" + i.ToString("00") + "_" + orient;
                    string path = Root + "/Prefabs/" + category + "/" + name + ".prefab";
                    GameObject tile = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (tile == null) continue;
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(tile);
                    instance.transform.SetParent(group.transform);
                    instance.transform.position = new Vector3(col * 10f, 0f, row * 10f);
                    col++;
                }
            }
            row++;
        }
        GameObject lightObj = new GameObject("Main Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.25f;
        light.shadows = LightShadows.Soft;
        lightObj.transform.rotation = Quaternion.Euler(45, -35, 0);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.78f,0.80f,0.85f);
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        Vector3 center = new Vector3(240,0,35);
        camObj.transform.position = center + new Vector3(-80,120,-120);
        camObj.transform.LookAt(center);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(.24f,.25f,.29f);
        EditorSceneManager.SaveScene(scene, "Assets/TerrainTriangleKit/Scenes/TerrainTriangleDemo.unity");
    }
}
#endif
