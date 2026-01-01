using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;
    public HotBarManager hotBarManager;

  

    public void Awake()
    {
        Instance = this;
    }

    public int addItem(Item item, int quantity)
    {
        SlotItem[] items = InventoryUI.Instance.inventoryItems;
        int leftOver = quantity;
        foreach (SlotItem itemInList in items)
        {
            if (itemInList == null) continue;
            if (itemInList.itemDetails.itemName == item.itemName)
            {
                itemInList.quantity += quantity;
                leftOver = itemInList.quantity - item.quantityLimit;
                if (leftOver > 0)
                {
                    itemInList.quantity -= leftOver;
                }
                else if (leftOver <= 0)
                {
                    return 0;
                }
                Debug.Log(leftOver);
            }
        }
        int index;
        while ((index = GetFirstEmptySlot()) != -1 && leftOver > item.quantityLimit)
        {
            items[index] = (new SlotItem(item, item.quantityLimit));;
            leftOver -= item.quantityLimit;
            InventoryUI.Instance.UpdateSlot(index);
        }
        if ((index = GetFirstEmptySlot()) != -1 && leftOver != 0)
        {
            items[index] = (new SlotItem(item, leftOver));
            leftOver -= item.quantityLimit;
            InventoryUI.Instance.UpdateSlot(index);
            return 0;
        }
        else
        {  
            return leftOver;
        }
    }

    public void DropItem(SlotItem slotItem, Vector3 pos)
    {

        for (int i=0; i<InventoryUI.Instance.inventoryItems.Length; i++)
        {
            if (InventoryUI.Instance.inventoryItems[i] == slotItem)
            {
                InventoryUI.Instance.inventoryItems[i] = null;
                InventoryUI.Instance.UpdateSlot(i);
            }
        }

        if (slotItem.itemDetails.worldPrefab != null)
            GenerateItem(slotItem.itemDetails, slotItem.quantity, pos);
    }



    public void GenerateItem(Item item, int quantity, Vector3 pos)
    {
        GameObject newItem = Instantiate(item.worldPrefab, pos, Quaternion.identity);
        Debug.Log(quantity.ToString() + "QUANTITY TO PREFAB"); 
        newItem.GetComponent<ItemInstance>().hotBarManager = hotBarManager;
        newItem.GetComponent<ItemInstance>().quantity = quantity;
    }

    public int GetFirstEmptySlot()
    {
        for (int i = 0; i < InventoryUI.Instance.inventoryItems.Length; i++)
        {
            if (InventoryUI.Instance.inventoryItems[i] == null)
                return i;
        }
        return -1;
    }


}
