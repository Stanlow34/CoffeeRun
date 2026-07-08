using UnityEngine;

[CreateAssetMenu(fileName = "Recipe_", menuName = "CoffeeVan/Recipe")]
public class RecipeSO : ScriptableObject
{
    public string id;
    public string displayName;
    public IngredientQuantity[] ingredients; // ingredient id + qty
    public float salePrice;
    public float estimatedCost; // sum of ingredient cost (for quick balancing)
    public float prepTimeSeconds; // time to prepare in gameplay
    public string complexity; // "Low"/"Medium"/"High"
    public GameObject prefab; // optional visual prefab
    public string[] tags;
}

[System.Serializable]
public struct IngredientQuantity
{
    public string ingredientId;
    public int quantity;
}
