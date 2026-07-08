using System;
using System.Collections.Generic;
using UnityEngine;

// Gère ingrédients, produits préparés, spoil et consommation.
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // capacité de stockage (modulable par cartes)
    public int baseStorageCapacity = 100;
    private int extraStorage = 0;

    // counts for ingredients (ingredientId -> qty)
    private Dictionary<string, int> ingredients = new Dictionary<string, int>();
    // prepared products (recipeId -> qty ready to serve)
    private Dictionary<string, int> preparedProducts = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public int GetTotalCapacity() => baseStorageCapacity + extraStorage;

    public void ModifyStorageCapacity(int delta)
    {
        extraStorage += delta;
        if (extraStorage < 0) extraStorage = 0;
    }

    // Add ingredient units (purchase or found)
    public void AddIngredient(string ingredientId, int qty)
    {
        if (!ingredients.ContainsKey(ingredientId)) ingredients[ingredientId] = 0;
        ingredients[ingredientId] += qty;
    }

    public bool HasIngredientsForRecipe(RecipeSO recipe)
    {
        foreach (var iq in recipe.ingredients)
        {
            if (!ingredients.TryGetValue(iq.ingredientId, out int have) || have < iq.quantity)
                return false;
        }
        return true;
    }

    // Consume ingredients to make one serving (instant or batch)
    public bool ConsumeIngredientsForRecipe(RecipeSO recipe, int servings = 1)
    {
        if (servings <= 0) return false;
        // check
        foreach (var iq in recipe.ingredients)
        {
            int need = iq.quantity * servings;
            if (!ingredients.TryGetValue(iq.ingredientId, out int have) || have < need) return false;
        }
        // consume
        foreach (var iq in recipe.ingredients)
        {
            int need = iq.quantity * servings;
            ingredients[iq.ingredientId] -= need;
        }
        return true;
    }

    // Prepare batch at night: consumes ingredients and adds preparedProducts
    public bool PrepareBatch(RecipeSO recipe, int units)
    {
        if (units <= 0) return false;
        if (!ConsumeIngredientsForRecipe(recipe, units)) return false;

        if (!preparedProducts.ContainsKey(recipe.id)) preparedProducts[recipe.id] = 0;
        preparedProducts[recipe.id] += units;
        return true;
    }

    // Serve an item: prefer prepared products if available, otherwise prepare on-demand
    public bool ServeRecipe(RecipeSO recipe)
    {
        if (preparedProducts.TryGetValue(recipe.id, out int ready) && ready > 0)
        {
            preparedProducts[recipe.id] = ready - 1;
            return true;
        }
        else
        {
            // try to make on demand
            if (ConsumeIngredientsForRecipe(recipe, 1))
            {
                return true;
            }
            return false;
        }
    }

    // Apply spoil each night: this uses IngredientSO.spoilRate or recipe tags to decide rate
    public void ApplySpoil(List<RecipeSO> allRecipes)
    {
        // Example simple spoil: for each prepared product, apply spoilRate from ingredients average
        var toUpdate = new Dictionary<string, int>(preparedProducts);
        foreach (var kv in toUpdate)
        {
            var recipe = allRecipes.Find(r => r.id == kv.Key);
            if (recipe == null) continue;

            float avgSpoil = 0f;
            int count = 0;
            foreach (var iq in recipe.ingredients)
            {
                var ing = Resources.Load<IngredientSO>($"Data/Ingredients/{iq.ingredientId}");
                if (ing != null)
                {
                    avgSpoil += ing.spoilRatePerDay;
                    count++;
                }
            }
            if (count > 0) avgSpoil /= count;
            int initial = kv.Value;
            int lost = Mathf.FloorToInt(initial * avgSpoil); // units lost
            preparedProducts[kv.Key] = Mathf.Max(0, initial - lost);
        }
    }

    public int GetPreparedCount(string recipeId)
    {
        return preparedProducts.TryGetValue(recipeId, out int v) ? v : 0;
    }

    public int GetIngredientCount(string ingredientId)
    {
        return ingredients.TryGetValue(ingredientId, out int v) ? v : 0;
    }

    // Save / Load simple (PlayerPrefs JSON). For prototype, acceptable.
    [Serializable]
    private class InventorySave
    {
        public KeyValuePairStringInt[] ingredients;
        public KeyValuePairStringInt[] prepared;
        public int extraStorage;
    }

    [Serializable]
    private struct KeyValuePairStringInt { public string key; public int value; }

    public void SaveInventory()
    {
        var save = new InventorySave();
        save.extraStorage = extraStorage;
        save.ingredients = new KeyValuePairStringInt[ingredients.Count];
        int i = 0;
        foreach (var kv in ingredients) { save.ingredients[i++] = new KeyValuePairStringInt { key = kv.Key, value = kv.Value }; }
        save.prepared = new KeyValuePairStringInt[preparedProducts.Count];
        i = 0;
        foreach (var kv in preparedProducts) { save.prepared[i++] = new KeyValuePairStringInt { key = kv.Key, value = kv.Value }; }
        var json = JsonUtility.ToJson(save);
        PlayerPrefs.SetString("inventory_save", json);
        PlayerPrefs.Save();
    }

    public void LoadInventory()
    {
        var json = PlayerPrefs.GetString("inventory_save", "");
        if (string.IsNullOrEmpty(json)) return;
        var save = JsonUtility.FromJson<InventorySave>(json);
        ingredients.Clear();
        preparedProducts.Clear();
        extraStorage = save.extraStorage;
        foreach (var kv in save.ingredients) ingredients[kv.key] = kv.value;
        foreach (var kv in save.prepared) preparedProducts[kv.key] = kv.value;
    }
}
