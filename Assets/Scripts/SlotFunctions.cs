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
            if (HotBarManager.Instance == null)
            {
                return null;
            }
            return HotBarManager.Instance.slotItems[index];

        }
        else
        {
            if (InventoryUI.Instance == null)
            {
                return null;
            }
            return InventoryUI.Instance.inventoryItems[index];


        }
    }


    public static void SetItem(SlotType slotType, int index, SlotItem item)
    {
        if (slotType == SlotType.Hotbar)
        {
            HotBarManager.Instance.slotItems[index] = item;
            HotBarManager.Instance.UpdateSlot(index);
        }
        else
        {
            InventoryUI.Instance.inventoryItems[index] = item;
            InventoryUI.Instance.UpdateSlot(index);
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
    public static void HandleSwap(SlotType fromType, int fromIndex, SlotType toType, int toIndex, SlotItem draggedItem)
    {
        ///Hotbar - Hotbar
        if (fromType == SlotType.Hotbar && toType == SlotType.Hotbar)
        {
            HotBarManager.Instance.SlotSwap(fromIndex, toIndex);
            return;
        }

        //Inventory - Inventory
        if (fromType == SlotType.Inventory && toType == SlotType.Inventory)
        {
            SlotItem temp = InventoryUI.Instance.inventoryItems[fromIndex];
            InventoryUI.Instance.inventoryItems[fromIndex] = InventoryUI.Instance.inventoryItems[toIndex];
            InventoryUI.Instance.inventoryItems[toIndex] = temp;

            InventoryUI.Instance.UpdateSlot(fromIndex);
            InventoryUI.Instance.UpdateSlot(toIndex);
            return;
        }

        //Hotbar - Inventory
        if (fromType == SlotType.Hotbar && toType == SlotType.Inventory)
        {
            InventoryUI.Instance.inventoryItems[toIndex] = new SlotItem(ScriptableObject.Instantiate(draggedItem.itemDetails), draggedItem.quantity);
            InventoryUI.Instance.UpdateSlot(toIndex);

            HotBarManager.Instance.slotItems[fromIndex] = null;
            HotBarManager.Instance.UpdateSlot(fromIndex);
            return;
        }

        //Inventory - Hotbar
        if (fromType == SlotType.Inventory && toType == SlotType.Hotbar)
        {
            SlotItem item = InventoryUI.Instance.inventoryItems[fromIndex];
            if (item == null)
            {
                return;
            }
            HotBarManager.Instance.slotItems[toIndex] = new SlotItem(ScriptableObject.Instantiate(item.itemDetails), item.quantity);
            HotBarManager.Instance.UpdateSlot(toIndex);

            InventoryUI.Instance.inventoryItems[fromIndex] = null;
            InventoryUI.Instance.UpdateSlot(fromIndex);
        }
    }






    }
