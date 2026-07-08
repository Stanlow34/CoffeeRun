#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class JSONToScriptableObjectsEditor : EditorWindow
{
    [MenuItem("Tools/CoffeeVan/Import Game Data")]
    public static void ShowWindow()
    {
        GetWindow<JSONToScriptableObjectsEditor>("Import Game Data");
    }

    private string dataPath = "Assets/Resources/Data";

    void OnGUI()
    {
        GUILayout.Label("Import JSON -> ScriptableObjects", EditorStyles.boldLabel);
        dataPath = EditorGUILayout.TextField("Data folder", dataPath);

        if (GUILayout.Button("Import Ingredients (ingredients.json)"))
            ImportIngredients();

        if (GUILayout.Button("Import Recipes (recipes.json)"))
            ImportRecipes();

        if (GUILayout.Button("Import Cards (cards.json)"))
            ImportCards();
    }

    private void ImportIngredients()
    {
        var path = Path.Combine(dataPath, "ingredients.json");
        if (!File.Exists(path)) { Debug.LogError("ingredients.json not found at " + path); return; }
        var json = File.ReadAllText(path);
        var list = JsonUtility.FromJson<IngredientListWrapper>("{\"items\":" + json + "}");
        string outFolder = "Assets/Data/Ingredients";
        if (!AssetDatabase.IsValidFolder(outFolder)) AssetDatabase.CreateFolder("Assets/Data", "Ingredients");
        foreach (var it in list.items)
        {
            var asset = ScriptableObject.CreateInstance<IngredientSO>();
            asset.id = it.id;
            asset.displayName = it.displayName;
            asset.cost = it.cost;
            asset.spoilRatePerDay = it.spoilRatePerDay;
            asset.tags = it.tags;
            string assetPath = $"{outFolder}/{it.id}.asset";
            AssetDatabase.CreateAsset(asset, assetPath);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Imported Ingredients: " + list.items.Length);
    }

    private void ImportRecipes()
    {
        var path = Path.Combine(dataPath, "recipes.json");
        if (!File.Exists(path)) { Debug.LogError("recipes.json not found at " + path); return; }
        var json = File.ReadAllText(path);
        var list = JsonUtility.FromJson<RecipeListWrapper>("{\"items\":" + json + "}");
        string outFolder = "Assets/Data/Recipes";
        if (!AssetDatabase.IsValidFolder(outFolder)) AssetDatabase.CreateFolder("Assets/Data", "Recipes");
        foreach (var it in list.items)
        {
            var asset = ScriptableObject.CreateInstance<RecipeSO>();
            asset.id = it.id;
            asset.displayName = it.displayName;
            asset.salePrice = it.salePrice;
            asset.estimatedCost = it.estimatedCost;
            asset.prepTimeSeconds = it.prepTimeSeconds;
            asset.complexity = it.complexity;
            asset.tags = it.tags;
            if (it.ingredients != null)
            {
                asset.ingredients = new IngredientQuantity[it.ingredients.Length];
                for (int i = 0; i < it.ingredients.Length; i++)
                {
                    asset.ingredients[i].ingredientId = it.ingredients[i].ingredientId;
                    asset.ingredients[i].quantity = it.ingredients[i].quantity;
                }
            }
            string assetPath = $"{outFolder}/{it.id}.asset";
            AssetDatabase.CreateAsset(asset, assetPath);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Imported Recipes: " + list.items.Length);
    }

    private void ImportCards()
    {
        var path = Path.Combine(dataPath, "cards.json");
        if (!File.Exists(path)) { Debug.LogError("cards.json not found at " + path); return; }
        var json = File.ReadAllText(path);
        var list = JsonUtility.FromJson<CardListWrapper>("{\"items\":" + json + "}");
        string outFolder = "Assets/Data/Cards";
        if (!AssetDatabase.IsValidFolder(outFolder)) AssetDatabase.CreateFolder("Assets/Data", "Cards");
        foreach (var it in list.items)
        {
            var asset = ScriptableObject.CreateInstance<CardSO>();
            asset.id = it.id;
            asset.displayName = it.displayName;
            asset.category = (CardCategory)System.Enum.Parse(typeof(CardCategory), it.category);
            asset.rarity = (CardRarity)System.Enum.Parse(typeof(CardRarity), it.rarity);
            asset.effectType = (EffectType)System.Enum.Parse(typeof(EffectType), it.effectType);
            asset.effectValue = it.effectValue;
            asset.durationDays = it.durationDays;
            asset.description = it.description;
            asset.consumable = it.consumable;
            string assetPath = $"{outFolder}/{it.id}.asset";
            AssetDatabase.CreateAsset(asset, assetPath);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Imported Cards: " + list.items.Length);
    }

    [System.Serializable]
    private class IngredientItem { public string id; public string displayName; public float cost; public float spoilRatePerDay; public string[] tags; }
    [System.Serializable]
    private class IngredientListWrapper { public IngredientItem[] items; }

    [System.Serializable]
    private class RecipeIngredient { public string ingredientId; public int quantity; }
    [System.Serializable]
    private class RecipeItem { public string id; public string displayName; public RecipeIngredient[] ingredients; public float salePrice; public float estimatedCost; public float prepTimeSeconds; public string complexity; public string[] tags; }
    [System.Serializable]
    private class RecipeListWrapper { public RecipeItem[] items; }

    [System.Serializable]
    private class CardItem { public string id; public string displayName; public string category; public string rarity; public string effectType; public float effectValue; public int durationDays; public string description; public bool consumable; }
    [System.Serializable]
    private class CardListWrapper { public CardItem[] items; }
}
#endif