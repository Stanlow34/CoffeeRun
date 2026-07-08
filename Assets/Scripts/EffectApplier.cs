using System.Collections.Generic;
using UnityEngine;

// EffectApplier is responsible for applying and removing the runtime effects of CardSO objects.
// It keeps a registry of active effects and provides helper APIs for other systems to query active modifiers.
public class EffectApplier : MonoBehaviour
{
    public static EffectApplier Instance;

    private Dictionary<string, CardSO> activeEffects = new Dictionary<string, CardSO>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    // Register a card effect (called by CardManager when granting/activating a card)
    public void ApplyEffect(CardSO card)
    {
        if (card == null) return;
        if (!activeEffects.ContainsKey(card.id))
        {
            activeEffects.Add(card.id, card);
            // handle immediate effect routing if necessary
            // e.g. if card.effectType == EffectType.ReduceSpoil -> update internal modifiers
        }
    }

    // Remove effect (called when temporary duration expires or a consumable is used)
    public void RemoveEffect(CardSO card)
    {
        if (card == null) return;
        if (activeEffects.ContainsKey(card.id))
        {
            activeEffects.Remove(card.id);
            // revert any persistent modifier applied
        }
    }

    // Query helpers
    public float GetReduceSpoilModifier()
    {
        float total = 0f;
        foreach (var c in activeEffects.Values)
        {
            if (c.effectType == EffectType.ReduceSpoil) total += c.effectValue;
        }
        return total; // additive modifier (e.g., 0.15 = 15%)
    }

    public int GetAdditionalStorage()
    {
        int total = 0;
        foreach (var c in activeEffects.Values)
        {
            if (c.effectType == EffectType.AddStorage) total += Mathf.FloorToInt(c.effectValue);
        }
        return total;
    }

    public float GetPrepTimeMultiplier(string tag)
    {
        // Example: cards that reduce prep time can be global or tag-specific in a later iteration.
        float multiplier = 1f;
        foreach (var c in activeEffects.Values)
        {
            if (c.effectType == EffectType.ReducePrepTime) multiplier *= (1f - c.effectValue);
        }
        return multiplier;
    }

    public float GetAttractClientsMultiplier()
    {
        float total = 0f;
        foreach (var c in activeEffects.Values)
        {
            if (c.effectType == EffectType.AttractClients) total += c.effectValue;
        }
        return 1f + total; // returns multiplier (1 + sum)
    }

    // Called from CardManager when a card is granted or consumed
    public void OnCardGranted(CardSO card)
    {
        ApplyEffect(card);
    }

    // Called on daily tick to decrement durations and remove expired temporary cards
    public void DailyTick()
    {
        List<CardSO> toRemove = new List<CardSO>();
        foreach (var kv in activeEffects)
        {
            var card = kv.Value;
            if (card.durationDays > 0)
            {
                // CardManager keeps track of remaining days; EffectApplier can rely on CardManager's removal call
            }
        }

        foreach (var c in toRemove) RemoveEffect(c);
    }
}
