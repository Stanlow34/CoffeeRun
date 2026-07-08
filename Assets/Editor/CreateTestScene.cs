#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class CreateTestScene
{
    [MenuItem("Tools/CoffeeVan/Create Test Scene and Managers Prefab")]
    public static void Create()
    {
        // Ensure directories exist
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");

        // Create new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = "TestScene";

        // Create Managers GameObject
        var managersGO = new GameObject("Managers");
        managersGO.AddComponent<CardManager>();
        managersGO.AddComponent<InventoryManager>();
        managersGO.AddComponent<EffectApplier>();
        managersGO.AddComponent<GameManager>();

        // Save scene
        string scenePath = "Assets/Scenes/TestScene.unity";
        EditorSceneManager.SaveScene(scene, scenePath);

        // Create prefab of Managers
        string prefabPath = "Assets/Prefabs/Managers.prefab";
        PrefabUtility.SaveAsPrefabAsset(managersGO, prefabPath);

        EditorUtility.DisplayDialog("CoffeeVan", "TestScene and Managers prefab created:\n" + scenePath + "\n" + prefabPath, "OK");

        // Focus project window on created assets
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        Selection.activeObject = sceneAsset;
    }
}
#endif
