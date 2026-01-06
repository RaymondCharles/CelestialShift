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

    // ---------- SAFETY CHECK ----------
    bool IsValidIndex(int index)
    {
        return Inventory.Instance != null &&
               index >= 0 &&
               index < Inventory.Instance.inventoryItems.Length &&
               index < slots.Length;
    }

    public void UpdateSlot(int slotIndex)
    {
        if (!IsValidIndex(slotIndex))
        {
            Debug.LogWarning($"[InventoryUI.UpdateSlot] Invalid index {slotIndex}");
            return;
        }

        SlotItem item = Inventory.Instance.inventoryItems[slotIndex];

        slots[slotIndex].sprite =
            item != null ? item.itemDetails.itemImg : emptySprite;

        slots[slotIndex]
            .GetComponentInChildren<TMP_Text>()
            .text = item != null ? item.quantity.ToString() : "";
    }

    public void SlotSwap(int slot1, int slot2)
    {
        if (!IsValidIndex(slot1) || !IsValidIndex(slot2))
        {
            Debug.LogError($"[InventoryUI.SlotSwap] Invalid indices {slot1}, {slot2}");
            return;
        }

        SlotItem temp = Inventory.Instance.inventoryItems[slot1];
        Inventory.Instance.inventoryItems[slot1] = Inventory.Instance.inventoryItems[slot2];
        Inventory.Instance.inventoryItems[slot2] = temp;

        UpdateSlot(slot1);
        UpdateSlot(slot2);
    }

    public void ClearSlot(int index)
    {
        if (!IsValidIndex(index)) return;

        Inventory.Instance.inventoryItems[index] = null;
        UpdateSlot(index);
    }

    public SlotItem GetItem(int slotIndex)
    {
        if (!IsValidIndex(slotIndex))
        {
            Debug.LogWarning($"[InventoryUI.GetItem] Invalid index {slotIndex}");
            return null;
        }

        return Inventory.Instance.inventoryItems[slotIndex];
    }

    public void SetItem(int slotIndex, SlotItem item)
    {
        if (!IsValidIndex(slotIndex))
        {
            Debug.LogWarning($"[InventoryUI.SetItem] Invalid index {slotIndex}");
            return;
        }

        Inventory.Instance.inventoryItems[slotIndex] = item;
        UpdateSlot(slotIndex);
    }

    public int GetFirstEmptySlot()
    {
        if (Inventory.Instance == null) return -1;

        for (int i = 0; i < Inventory.Instance.inventoryItems.Length; i++)
        {
            if (Inventory.Instance.inventoryItems[i] == null)
                return i;
        }
        return -1;
    }
}
