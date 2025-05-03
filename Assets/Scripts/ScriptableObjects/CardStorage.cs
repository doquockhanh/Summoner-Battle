using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardStorage", menuName = "Game/CardStorage")]
public class CardStorage: ScriptableObject
{
    [Header("Danh sách thẻ bài")]
    public List<Card> cards = new List<Card>();
} 