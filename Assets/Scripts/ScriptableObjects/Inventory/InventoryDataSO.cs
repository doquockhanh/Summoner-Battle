using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/InventoryDataSO")]
public class InventoryDataSO : ScriptableObject
{
    public List<Item> items = new List<Item>();
}
