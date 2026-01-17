using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public float[] position;
    public List<InventorySlotData> inventorySlots;
    public List<InventorySlotData> HotBarSlots;

    public InventorySlotData armorSlot;

    public float hotBarSize;
    public float inventorySize;
    public float musicVolume;
    public bool isGameOver;

    public int day;
    public float timeOfDay;
    public float elapsedTime;
    public string timeText;





    public PlayerData(FirstPersonController player, DayNightCycle time)
    {
        day = time.dayNumber;
        timeOfDay = time.timeOfDay;
        elapsedTime = time.elapsedTime;
        timeText = time.clockText;



        
        isGameOver = player.isGameOver;



        // Save position
        position = new float[3];
        position[0] = player.transform.position.x;
        position[1] = player.transform.position.y + 20f;
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
                Debug.Log(slot.itemDetails.itemName);
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
                    Debug.Log(slot.itemDetails.itemName);
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

        if (ArmorUI.Instance != null && ArmorUI.Instance.armorItem[0] != null)
        {
            armorSlot = new InventorySlotData(ArmorUI.Instance.armorItem[0].itemDetails.itemName, ArmorUI.Instance.armorItem[0].quantity, 0);
        }

        hotBarSize = PlayerPrefs.GetFloat("HotbarSize", 0.6f);
        inventorySize = PlayerPrefs.GetFloat("InvSize", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVol", 0.6f);
    }
}
