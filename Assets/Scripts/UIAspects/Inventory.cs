using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;
    public List<SlotItem> items = new List<SlotItem>();
    public HotBarManager hotBarManager;

  

    public void Awake()
    {
        Instance = this;
    }

    public int addItem(Item item, int quantity)
    {
        int leftOver = quantity;
        foreach (SlotItem itemInList in items)
        {
            if (itemInList.itemDetails.itemName == item.itemName)
            {
                itemInList.quantity += quantity;
                leftOver = itemInList.quantity - item.quantityLimit;
                if (leftOver > 0)
                {
                    itemInList.quantity -= leftOver;
                }
            }
        }
        int index;
        while ((index = GetFirstEmptySlot()) != -1 && leftOver > item.quantityLimit)
        {
            items[index] = (new SlotItem(item, item.quantityLimit));
            InventoryUI.Instance.inventoryItems[index] = items[index];
            leftOver -= item.quantityLimit;
            InventoryUI.Instance.UpdateSlot(index);
        }
        if ((index = GetFirstEmptySlot()) != -1 && leftOver != 0)
        {
            items[index] = (new SlotItem(item, leftOver));
            InventoryUI.Instance.inventoryItems[index] = items[index];
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
        items.Remove(slotItem);

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
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
                return i;
        }
        if (items.Count < 8)
        {
            items.Add(null);
            return items.Count - 1;
        }
        return -1;
    }


}
