using UnityEngine;

[System.Serializable]
public class InventorySlotData
{
    public string itemName;
    public int quantity;
    public int slotIndex;

    public InventorySlotData(string itemName, int quantity, int slotIndex)
    {
        this.itemName = itemName;
        this.quantity = quantity;
        this.slotIndex = slotIndex;
    }
}
