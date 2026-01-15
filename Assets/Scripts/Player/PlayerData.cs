using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public float[] position;
    public List<InventorySlotData> inventorySlots;
    public List<InventorySlotData> HotBarSlots;
    public float hotBarSize;
    public float inventorySize;
    public float musicVolume;



    public PlayerData(FirstPersonController player)
    {
        // Save position
        position = new float[3];
        position[0] = player.transform.position.x;
        position[1] = player.transform.position.y + 5f;
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
        HotBarSlots = new List<InventorySlotData>();
        HotBarManager hotbar = HotBarManager.Instance;
        if (hotbar != null)
        {
            for (int i = 0; i < hotbar.slotItems.Length; i++)
            {
                SlotItem slot = hotbar.slotItems[i];
                if (slot != null)
                {
                    HotBarSlots.Add(
                        new InventorySlotData(
                            slot.itemDetails.itemName,
                            slot.quantity,
                            i
                        )
                    );
                }
            }
        }
        hotBarSize = PlayerPrefs.GetFloat("HotbARSize", 0.6f);
        inventorySize = PlayerPrefs.GetFloat("InvSize", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVol", 0.6f);
    }
}
