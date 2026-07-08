using System;
using System.Collections.Generic;
using UnityEngine;

// Gère le stockage, l'application et la durée des cartes (ScriptableObjects CardSO)
public class CardManager : MonoBehaviour
{
    public static CardManager Instance;

    // cartes possédées par le joueur
    [Serializable]
    public class OwnedCard
    {
        public string cardId;
        public int remainingDays; // 0 = permanent, >0 = temporaire, -1 = consumable/unused
    }

    public List<OwnedCard> ownedCards = new List<OwnedCard>();

    // Références données (chargées via Resources ou assignées depuis l'éditeur)
    private Dictionary<string, CardSO> cardLookup;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadCardDefinitions();
    }

    private void LoadCardDefinitions()
    {
        cardLookup = new Dictionary<string, CardSO>();
        var cards = Resources.LoadAll<CardSO>("Data/Cards");
        foreach (var c in cards)
        {
            if (!cardLookup.ContainsKey(c.id)) cardLookup.Add(c.id, c);
        }
    }

    // Tirer une carte depuis le pool (simulé ici : pass in a CardSO)
    public OwnedCard GrantCard(CardSO card)
    {
        var oc = new OwnedCard();
        oc.cardId = card.id;
        oc.remainingDays = card.durationDays > 0 ? card.durationDays : 0; // 0 = permanent
        ownedCards.Add(oc);

        ApplyCardImmediateEffect(card, oc);
        SaveState();
        return oc;
    }

    // Appliquer les effets qui doivent s'appliquer immédiatement (ex: ajouter recette, storage)
    private void ApplyCardImmediateEffect(CardSO card, OwnedCard owned)
    {
        switch (card.effectType)
        {
            case EffectType.AddStorage:
                InventoryManager.Instance?.ModifyStorageCapacity((int)card.effectValue);
                break;
            case EffectType.AddRecipe:
                // Le CardSO peut contenir un tag ou id pour la recette ; implémentation d'exemple :
                // Recipe unlock management
                GameManager.Instance?.UnlockRecipeFromCard(card);
                break;
            case EffectType.OneTimeBonus:
                GameManager.Instance?.AddMoney(card.effectValue);
                break;
            case EffectType.ReduceSpoil:
            case EffectType.IncreaseQuality:
            case EffectType.ReducePrepTime:
            case EffectType.AttractClients:
            case EffectType.DiscountIngredients:
            case EffectType.TemporaryBuff:
            case EffectType.UnlockTour:
                // Ces effets sont gérés via pipeline d'effets (ex: EffectApplier) chaque jour/chaque tick.
                break;
            default:
                Debug.LogWarning($"Card effect not handled: {card.effectType}");
                break;
        }
    }

    // Appelé en fin de journée / tick journalier pour décrémenter durations et retirer consommables expirés
    public void DailyTick()
    {
        bool dirty = false;
        for (int i = ownedCards.Count - 1; i >= 0; i--)
        {
            var oc = ownedCards[i];
            var card = GetCardSO(oc.cardId);
            if (card == null) { ownedCards.RemoveAt(i); dirty = true; continue; }

            if (card.durationDays > 0)
            {
                if (oc.remainingDays > 0)
                {
                    oc.remainingDays--;
                    if (oc.remainingDays == 0)
                    {
                        // duration finished: remove temporary buff effects if needed
                        RemoveCardEffects(card, oc);
                        // keep card in collection as "expired" or remove depending on design
                    }
                    dirty = true;
                }
            }
        }
        if (dirty) SaveState();
    }

    private void RemoveCardEffects(CardSO card, OwnedCard oc)
    {
        // Implémentation: retirer effets temporaires (ex: remettre spoil à la normale)
        EffectApplier.Instance?.RemoveEffect(card);
    }

    public CardSO GetCardSO(string id)
    {
        cardLookup.TryGetValue(id, out var c);
        return c;
    }

    // Exemple d'appel quand on veut consommer une carte one-time
    public bool UseConsumable(string cardId)
    {
        var oc = ownedCards.Find(x => x.cardId == cardId);
        if (oc == null) return false;
        var card = GetCardSO(cardId);
        if (card == null) return false;
        if (!card.consumable) return false;

        // Appliquer effet unique
        ApplyCardImmediateEffect(card, oc);

        // Retirer la carte
        ownedCards.Remove(oc);
        SaveState();
        return true;
    }

    // Sauvegarde simple en JSON (persistance locale)
    public void SaveState()
    {
        var json = JsonUtility.ToJson(new OwnedCardList { items = ownedCards.ToArray() });
        PlayerPrefs.SetString("owned_cards", json);
        PlayerPrefs.Save();
    }

    public void LoadState()
    {
        var json = PlayerPrefs.GetString("owned_cards", "");
        if (string.IsNullOrEmpty(json)) return;
        var list = JsonUtility.FromJson<OwnedCardList>(json);
        ownedCards = new List<OwnedCard>(list.items);
    }

    [Serializable]
    private class OwnedCardList { public OwnedCard[] items; }
}

// Remarques : EffectApplier et GameManager sont des utilitaires à créer pour lier les effets des cartes au jeu.
