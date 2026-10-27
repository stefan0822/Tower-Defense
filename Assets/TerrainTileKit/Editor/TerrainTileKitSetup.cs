#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class TerrainTileKitSetup
{
    const string Root = "Assets/TerrainTileKit";
    static readonly string[] Categories = {
        "Grass_Flat", "Grass_Hills", "Grass_RockyGround", "Grass_RockyMountains",
        "Desert_Flat", "Desert_Mountains", "Snow_Flat", "Snow_Mountains"
    };

    [MenuItem("Tools/Terrain Tile Kit/1. Generate Materials and Prefabs")]
    public static void BuildAssets()
    {
        Directory.CreateDirectory("Assets/TerrainTileKit/Materials");
        Directory.CreateDirectory("Assets/TerrainTileKit/Prefabs");
        AssetDatabase.Refresh();
        int count = 0;
        foreach (string category in Categories)
        {
            Directory.CreateDirectory(Root + "/Materials/" + category);
            Directory.CreateDirectory(Root + "/Prefabs/" + category);
            AssetDatabase.Refresh();
            for (int i = 1; i <= 12; i++)
            {
                string name = category + "_" + i.ToString("00");
                string modelPath = Root + "/Models/" + category + "/" + name + ".obj";
                string texturePath = Root + "/Textures/" + category + "/" + name + ".png";
                string matPath = Root + "/Materials/" + category + "/" + name + ".mat";
                string prefabPath = Root + "/Prefabs/" + category + "/" + name + ".prefab";
                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (model == null || texture == null)
                {
                    Debug.LogWarning("Missing model or texture: " + name);
                    continue;
                }
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
                {
                    r.sharedMaterials = new[] {material};
                }
                foreach (MeshFilter filter in instance.GetComponentsInChildren<MeshFilter>())
                {
                    MeshCollider collider = filter.GetComponent<MeshCollider>();
                    if (collider == null) collider = filter.gameObject.AddComponent<MeshCollider>();
                    collider.sharedMesh = filter.sharedMesh;
                }
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                UnityEngine.Object.DestroyImmediate(instance);
                count++;
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Terrain Tile Kit: " + count + " terrain prefabs ready.");
        EditorUtility.DisplayDialog("Terrain Tile Kit", "Created " + count + " prefabs with materials and mesh colliders.", "OK");
    }

    [MenuItem("Tools/Terrain Tile Kit/2. Create Demo Scene")]
    public static void BuildDemoScene()
    {
        // First run the materials-and-prefabs menu option.
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        int total = 0;
        for (int row = 0; row < Categories.Length; row++)
        {
            GameObject group = new GameObject(Categories[row]);
            for (int col = 0; col < 12; col++)
            {
                string name = Categories[row] + "_" + (col + 1).ToString("00");
                string assetPath = Root + "/Prefabs/" + Categories[row] + "/" + name + ".prefab";
                GameObject tile = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (tile == null) continue;
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(tile);
                instance.transform.SetParent(group.transform);
                instance.transform.position = new Vector3(col * 11.5f, 0, row * 13.0f);
                total++;
            }
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
        Vector3 center = new Vector3(68,0,50);
        camObj.transform.position = center + new Vector3(-35,90,-88);
        camObj.transform.LookAt(center);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(.24f,.25f,.29f);
        cam.orthographic = true;
        cam.orthographicSize = 85f;
        Directory.CreateDirectory(Root + "/Demo");
        EditorSceneManager.SaveScene(scene, Root + "/Demo/TerrainTileKit_Demo.unity");
        Debug.Log("Terrain Tile Kit: saved demo scene with " + total + " tiles.");
    }
}
#endif
