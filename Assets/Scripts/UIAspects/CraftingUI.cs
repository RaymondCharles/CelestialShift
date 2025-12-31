using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUI : MonoBehaviour, UIManager
{
    public static CraftingUI Instance;

    public Image[] slots;
    public Image resultSlot;
    public SlotItem[] craftingItems;
    public Sprite emptySprite;

    private void Awake()
    {
        Instance = this;
        craftingItems = new SlotItem[slots.Length];
    }
    public void UpdateSlot(int slotIndex)
    {
        slots[slotIndex].sprite = craftingItems[slotIndex] != null ? craftingItems[slotIndex].itemDetails.itemImg : emptySprite;
    }

    public void SlotSwap(int slot1, int slot2)
    {
        SlotItem temp = craftingItems[slot1];
        craftingItems[slot1] = craftingItems[slot2];
        craftingItems[slot2] = temp;

        UpdateSlot(slot1);
        UpdateSlot(slot2);

    }
    public void ClearSlot(int index)
    {
        if (index < 0 || index >= craftingItems.Length) return;

        craftingItems[index] = null;
        UpdateSlot(index);
    }

    public SlotItem GetItem(int slotIndex)
    {
        return craftingItems[slotIndex];
    }

    public void SetItem(int slotIndex, SlotItem item)
    {
        craftingItems[slotIndex] = item;
        return;
    }

    public bool isSuperCrafting()
    {
        return craftingItems[2] != null;
    }
}
