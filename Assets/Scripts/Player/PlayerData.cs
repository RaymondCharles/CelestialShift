using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public float[] position;
    public List<InventorySlotData> inventorySlots;

    public PlayerData(FirstPersonController player)
    {
        // Save position
        position = new float[3];
        position[0] = player.transform.position.x;
        position[1] = player.transform.position.y;
        position[2] = player.transform.position.z;

        // Save inventory
        inventorySlots = new List<InventorySlotData>();

        Inventory inv = Inventory.Instance;

        if (inv == null)
        {
            Debug.LogError("Inventory.Instance is NULL during save!");
            return;
        }

        for (int i = 0; i < inv.inventoryItems.Length; i++)
        {
            SlotItem slot = inv.inventoryItems[i];
            if (slot != null)
            {
                inventorySlots.Add(
                    new InventorySlotData(
                        slot.itemDetails.itemName,
                        slot.quantity,
                        i
                    )
                );
            }
        }
    }
}
