using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;



public class InventoryUI : MonoBehaviour, UIManager
{
    public static InventoryUI Instance;

    public Image[] slots;
    public Sprite emptySprite;
    public GameObject InventoryPanel;

    private void Awake()
    {
        Instance = this;
    }
    public void UpdateSlot(int slotIndex)
    {
        slots[slotIndex].sprite = Inventory.Instance.inventoryItems[slotIndex] != null ? Inventory.Instance.inventoryItems[slotIndex].itemDetails.itemImg : emptySprite;
        slots[slotIndex].GetComponentInChildren<TMP_Text>().text = Inventory.Instance.inventoryItems[slotIndex] != null ? Inventory.Instance.inventoryItems[slotIndex].quantity.ToString() : "";
    }

    public void SlotSwap(int slot1, int slot2)
    {
        SlotItem temp = Inventory.Instance.inventoryItems[slot1];
        Inventory.Instance.inventoryItems[slot1] = Inventory.Instance.inventoryItems[slot2];
        Inventory.Instance.inventoryItems[slot2] = temp;

        UpdateSlot(slot1);
        UpdateSlot(slot2);

    }
    public void ClearSlot(int index)
    {
        if (index < 0 || index >= Inventory.Instance.inventoryItems.Length) return;

        Inventory.Instance.inventoryItems[index] = null;
        UpdateSlot(index);
    }

    public SlotItem GetItem(int slotIndex)
    {
        return Inventory.Instance.inventoryItems[slotIndex];
    }

    public void SetItem(int slotIndex, SlotItem item)
    {
        Inventory.Instance.inventoryItems[slotIndex] = item;
        return;
    }

    public int GetFirstEmptySlot()
    {
        for (int i = 0; i < Inventory.Instance.inventoryItems.Length; i++)
        {
            if (Inventory.Instance.inventoryItems[i] == null)
                return i;
        }
        return -1;
    }

}
