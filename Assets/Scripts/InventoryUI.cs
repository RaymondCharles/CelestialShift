using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    public Image[] slots;
    public SlotItem[] inventoryItems;
    public Sprite emptySprite;

    private void Awake()
    {
        Instance = this;
        inventoryItems = new SlotItem[slots.Length];
    }
    public void UpdateSlot(int index)
    {
        slots[index].sprite = inventoryItems[index] != null ? inventoryItems[index].itemDetails.itemImg : emptySprite;
    }

    public void SlotSwap(int slot1, int slot2)
    {
        SlotItem temp = inventoryItems[slot1];
        inventoryItems[slot1] = inventoryItems[slot2];
        inventoryItems[slot2] = temp;

        UpdateSlot(slot1);
        UpdateSlot(slot2);

    }
    public void ClearSlot(int index)
    {
        if (index < 0 || index >= inventoryItems.Length) return;

        inventoryItems[index] = null;
        UpdateSlot(index);
    }

}
