using System.Collections.Generic;
using UnityEngine;

public class BattleDataManager : MonoBehaviour
{
    public static BattleDataManager Instance;

    public List<string> defenderIDs;
    public List<string> attackerIDs;
    public CardInventory cardInventory;
    public CardStorage cardStorageHolder;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public List<Card> GetAttackerCards()
    {
        List<Card> attackerCards = new List<Card>();
        foreach (string id in attackerIDs)
        {
            Card card = GetCard(cardInventory.availableCards, id);
            if (card == null)
            {
                Debug.Log($"Attacker Cant find card with id: {id}");
                continue;
            }
            attackerCards.Add(card);
        }

        return attackerCards;
    }

    public List<Card> GetDefenderCards()
    {
        List<Card> defenderCards = new List<Card>();
        foreach (string id in defenderIDs)
        {
            Card card = GetCard(cardStorageHolder.cards, id);
            if (card == null)
            {
                Debug.Log($"Defender Cant find card with id: {id}");
                continue;
            }
            defenderCards.Add(card);
        }

        return defenderCards;
    }

    public Card GetCard(List<Card> cards, string id)
    {
        return cards.Find(c => c.id == id);
    }
}
