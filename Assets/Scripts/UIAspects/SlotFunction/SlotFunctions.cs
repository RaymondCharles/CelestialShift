using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SlotFunctions : MonoBehaviour
{

    public static SlotItem GetItem(SlotType slotType, int index)
    {
        if (slotType == SlotType.Hotbar)
        {
            if (HotBarManager.Instance == null || index < 0 || index>= HotBarManager.Instance.slotItems.Length)
            {
                return null;
            }
            return HotBarManager.Instance.slotItems[index];

        }
        else if (slotType == SlotType.Inventory)
        {
            if (Inventory.Instance == null || index < 0 || index >= Inventory.Instance.inventoryItems.Length)
            {
                return null;
            }
            return Inventory.Instance.inventoryItems[index];
        }
        else if (slotType == SlotType.Crafting)
        {
            if (CraftingUI.Instance == null || index < 0 || index >= CraftingUI.Instance.craftingItems.Length)
            {
                return null;
            }
            return CraftingUI.Instance.craftingItems[index];
        }
        else
        {
            if (ArmorUI.Instance == null || index < 0 || index >= ArmorUI.Instance.armorItem.Length)
            {
                return null;
            }
            return ArmorUI.Instance.armorItem[index];
        }
    }


    public static void SetItem(SlotType slotType, int index, SlotItem item)
    {
        if (slotType == SlotType.Hotbar)
        {
            if (HotBarManager.Instance == null || index < 0 || index >= HotBarManager.Instance.slotItems.Length)
            {
                return;
            }
            HotBarManager.Instance.slotItems[index] = item;
            HotBarManager.Instance.UpdateSlot(index);
        }
        else if (slotType == SlotType.Inventory)
        {
            if (Inventory.Instance == null || index < 0 || index >= Inventory.Instance.inventoryItems.Length)
            {
                return;
            }

            Inventory.Instance.inventoryItems[index] = item;
            InventoryUI.Instance.UpdateSlot(index);
        }
        else if (slotType == SlotType.Inventory)
        {
            if (CraftingUI.Instance == null || index < 0 || index >= CraftingUI.Instance.craftingItems.Length)
            {
                return;
            }
            CraftingUI.Instance.craftingItems[index] = item;
            CraftingUI.Instance.UpdateSlot(index);
        }
        else
        {
            if (ArmorUI.Instance == null || index < 0 || index >= ArmorUI.Instance.armorItem.Length)
            {
                return;
            }
            ArmorUI.Instance.armorItem[index] = item;
            ArmorUI.Instance.UpdateSlot(index);
        }
    }

    public static void ClearItem(SlotType slotType, int index)
    {
        SetItem(slotType, index, null);
    }

    public void SlotSwap(SlotType a, int aindex, SlotType b, int bindex)
    {
        SlotItem aItem = GetItem(a, aindex);
        SlotItem bItem = GetItem(b, bindex);

        SetItem(a, aindex, bItem);
        SetItem(b, bindex, aItem);

    }

    public static SlotItem Clone(SlotItem item)
    {
        return item == null ? null : new SlotItem(ScriptableObject.Instantiate(item.itemDetails), item.quantity);

    }
    public static void HandleSwap(
        SlotType fromType, int fromIndex,
        SlotType toType, int toIndex,
        SlotItem draggedItem)
    {
        // Same slot type
        if (fromType == toType)
        {
            if (fromType == SlotType.Hotbar && HotBarManager.Instance != null)
            {
                HotBarManager.Instance.SlotSwap(fromIndex, toIndex);
            }
            else if (fromType == SlotType.Inventory && InventoryUI.Instance != null)
            {
                InventoryUI.Instance.SlotSwap(fromIndex, toIndex);
            }
            else if (fromType == SlotType.Crafting && CraftingUI.Instance != null)
            {
                CraftingUI.Instance.SlotSwap(fromIndex, toIndex);
            }
            return;
        }

        // Cross-type swap
        SlotItem fromItem = GetItem(fromType, fromIndex);
        SlotItem toItem = GetItem(toType, toIndex);

        if (fromItem == null && toItem == null)
            return;

        SetItem(toType, toIndex, fromItem);
        SetItem(fromType, fromIndex, toItem);
    }



}






    
