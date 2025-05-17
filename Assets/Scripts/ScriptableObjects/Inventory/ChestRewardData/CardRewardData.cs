using UnityEngine;

[CreateAssetMenu(menuName = "ChestReward/CardReward")]
public class CardRewardData : ChestRewardData
{
    public Card cardData;
    public override void GrantReward(object player, int amount)
    {
        // TODO: Thêm card vào bộ sưu tập của player
        Debug.Log($"Player nhận được {amount} x card {cardData.name}");
    }
} 