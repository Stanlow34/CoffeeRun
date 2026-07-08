using UnityEngine;

[CreateAssetMenu(fileName = "Ingredient_", menuName = "CoffeeVan/Ingredient")]
public class IngredientSO : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;
    public float cost; // cost per unit
    public float spoilRatePerDay; // ex: 0.10 = 10% per night
    public string[] tags;
}