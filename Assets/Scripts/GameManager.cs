using System.Collections.Generic;
using UnityEngine;

// GameManager holds overall game state and provides helper methods for money, reputation, and unlocking recipes/tours.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentDay = 1;
    public int totalDaysInTour = 1;

    public float money = 100f;
    public int reputation = 0;

    // unlocked recipes and tours
    public List<string> unlockedRecipeIds = new List<string>();
    public List<string> unlockedTourIds = new List<string>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    public void AddMoney(float amount)
    {
        money += amount;
        // clamp, save, notify UI
    }

    public bool SpendMoney(float amount)
    {
        if (money >= amount)
        {
            money -= amount;
            return true;
        }
        return false;
    }

    public void AddReputation(int amt)
    {
        reputation += amt;
    }

    public void UnlockRecipeFromCard(CardSO card)
    {
        // Card effect may include info about recipe id in description or via tags in future iteration
        // For now, this is a placeholder where you'd add specific recipe ids
        Debug.Log($"UnlockRecipeFromCard called for card {card.displayName}");
    }

    public void UnlockTour(string tourId)
    {
        if (!unlockedTourIds.Contains(tourId)) unlockedTourIds.Add(tourId);
    }

    // Example daily tick called by higher-level TourManager/DayManager
    public void DailyTick()
    {
        currentDay++;
        EffectApplier.Instance?.DailyTick();
        CardManager.Instance?.DailyTick();
        // Inventory spoil
        var allRecipes = new List<RecipeSO>(Resources.LoadAll<RecipeSO>("Data/Recipes"));
        InventoryManager.Instance?.ApplySpoil(allRecipes);
    }
}
