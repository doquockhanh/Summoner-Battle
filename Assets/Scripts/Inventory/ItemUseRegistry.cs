using System.Collections.Generic;

public static class ItemUseRegistry
{
    private static Dictionary<int, IItemUsable> _behaviours = new Dictionary<int, IItemUsable>
    {
        { 4, new SilverChestBehaviour() }, // id 4 là Hòm bạc
        // Thêm các item đặc biệt khác ở đây
    };

    public static IItemUsable GetBehaviour(int itemId)
    {
        _behaviours.TryGetValue(itemId, out var behaviour);
        return behaviour;
    }
} 