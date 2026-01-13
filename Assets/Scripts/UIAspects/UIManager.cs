using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface UIManager
{
    // Get the item at a specific slot index
    SlotItem GetItem(int slotIndex);

    void SetItem(int slotIndex, SlotItem item);

    // Swap items between two slots
    void SlotSwap(int fromSlotIndex, int toSlotIndex);

    void UpdateSlot(int slotIndex);
}
