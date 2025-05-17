using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Chest/ChestData")]
public class ChestData : ScriptableObject
{
    public List<ChestRewardEntry> rewards;
} 