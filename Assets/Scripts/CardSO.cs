using UnityEngine;

public enum CardRarity { Common, Rare, Epic, Legendary }
public enum CardCategory { Van, Equipment, Menu, Skill, Environment, Event }
public enum EffectType { AddStorage, ReduceSpoil, AttractClients, ReducePrepTime, IncreaseQuality, AddRecipe, DiscountIngredients, OneTimeBonus, TemporaryBuff, UnlockTour }

[CreateAssetMenu(fileName = "Card_", menuName = "CoffeeVan/Card")]
public class CardSO : ScriptableObject
{
    public string id;
    public string displayName;
    public CardCategory category;
    public CardRarity rarity;
    public EffectType effectType;
    public float effectValue; // generic numeric value (interpretation by CardManager)
    public int durationDays; // 0 = permanent, >0 = temporary
    public string description;
    public Sprite artwork;
    public bool consumable; // true if usage is one-time
}
