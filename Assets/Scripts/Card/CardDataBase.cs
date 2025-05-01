// using UnityEngine;

// public class CardDatabase : MonoBehaviour
// {
//     public static CardDatabase Instance;

//     public List<CardSO> allCards;

//     private Dictionary<string, CardSO> cardLookup = new();

//     void Awake()
//     {
//         if (Instance == null)
//             Instance = this;

//         foreach (var card in allCards)
//         {
//             cardLookup[card.cardId] = card;
//         }
//     }

//     public CardSO GetCardById(string id)
//     {
//         if (cardLookup.TryGetValue(id, out var card))
//             return card;

//         Debug.LogWarning($"Card not found: {id}");
//         return null;
//     }
// }
