using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardInventory", menuName = "Game/CardInventory")]
public class CardInventory : ScriptableObject
{
    [Header("Danh sách thẻ bài")]
    public List<Card> availableCards = new List<Card>();
} 