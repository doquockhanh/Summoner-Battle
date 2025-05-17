using System.Collections.Generic;
using UnityEngine;

public static class ChestOpener
{
    public static void OpenChest(ChestData chest, object player)
    {
        List<ChestRewardEntry> granted = new List<ChestRewardEntry>();
        foreach (var entry in chest.rewards)
        {
            if (Random.value <= entry.probability)
                granted.Add(entry);
        }
        // Đảm bảo luôn có ít nhất 1 phần thưởng
        if (granted.Count == 0 && chest.rewards.Count > 0)
        {
            // Chọn phần thưởng có tỉ lệ cao nhất
            float maxProb = -1f;
            ChestRewardEntry fallback = null;
            foreach (var entry in chest.rewards)
            {
                if (entry.probability > maxProb)
                {
                    maxProb = entry.probability;
                    fallback = entry;
                }
            }
            if (fallback != null)
                granted.Add(fallback);
        }
        // Trao thưởng
        foreach (var entry in granted)
        {
            int amount = Random.Range(entry.minAmount, entry.maxAmount + 1);
            entry.rewardData.GrantReward(player, amount);
        }
    }
} 